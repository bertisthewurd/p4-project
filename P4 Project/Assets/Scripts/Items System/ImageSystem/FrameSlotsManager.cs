using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSlotsManager : MonoBehaviour
{
    [Tooltip("FrameSlots in unlock order. Index 0 starts unlocked; each next one unlocks when the previous receives a frame. Leave empty to auto-discover children in hierarchy order.")]
    public List<FrameSlot> frameSlots = new();

    [Tooltip("Stagger delay between ejects, relocks, and unlocks (right-to-left cascade).")]
    public float cascadeStagger = 0.25f;

    private Coroutine _cascadeRoutine;

    void Awake()
    {
        if (frameSlots.Count == 0)
            frameSlots.AddRange(GetComponentsInChildren<FrameSlot>(true));
    }

    void Start()
    {
        for (int i = 0; i < frameSlots.Count; i++)
        {
            FrameSlot slot = frameSlots[i];
            if (slot == null) continue;

            if (i > 0)
                slot.LockImmediate();

            slot.OnFramePlaced += HandleFramePlaced;
            slot.OnFrameRemoved += HandleFrameRemoved;
        }
    }

    void OnDestroy()
    {
        foreach (var slot in frameSlots)
        {
            if (slot != null)
            {
                slot.OnFramePlaced -= HandleFramePlaced;
                slot.OnFrameRemoved -= HandleFrameRemoved;
            }
        }
    }

    private void HandleFramePlaced(FrameSlot slot)
    {
        int index = frameSlots.IndexOf(slot);
        if (index < 0) return;
        int nextIndex = index + 1;
        if (nextIndex >= frameSlots.Count) return;
        FrameSlot next = frameSlots[nextIndex];
        if (next != null && next.IsLocked) next.Unlock();
    }

    private void HandleFrameRemoved(FrameSlot slot)
    {
        int index = frameSlots.IndexOf(slot);
        if (index < 0) return;

        if (_cascadeRoutine != null) StopCoroutine(_cascadeRoutine);
        _cascadeRoutine = StartCoroutine(EjectAndRelockCascade(index));
    }

    private IEnumerator EjectAndRelockCascade(int removedFromIndex)
    {
        // Lock state on all slots beyond the new frontier up front so the player
        // can't slip a frame in during the eject/relock stagger.
        for (int i = removedFromIndex + 1; i < frameSlots.Count; i++)
        {
            FrameSlot slot = frameSlots[i];
            if (slot != null) slot.MarkLocked();
        }

        // Eject all frames in slots > removedFromIndex (left-to-right).
        // Only stagger when something actually ejects, so empty slots don't
        // pad the cascade with silent waits.
        for (int i = removedFromIndex + 1; i < frameSlots.Count; i++)
        {
            FrameSlot slot = frameSlots[i];
            if (slot == null || slot.IsEmpty) continue;
            slot.EjectFrame();
            yield return new WaitForSeconds(Mathf.Max(0f, cascadeStagger));
        }

        // Run the relock cascade based on new frontier
        yield return StartCoroutine(RelockCascade());
        _cascadeRoutine = null;
    }

    private IEnumerator RelockCascade()
    {
        // Find rightmost filled slot to determine frontier
        int rightmostFilled = -1;
        for (int i = frameSlots.Count - 1; i >= 0; i--)
        {
            if (frameSlots[i] != null && !frameSlots[i].IsEmpty)
            {
                rightmostFilled = i;
                break;
            }
        }
        int frontier = rightmostFilled + 1;

        // Play the lock visual right-to-left. Skip slots that are already fully
        // ghosted — Lock() would be a no-op on those, so we shouldn't burn
        // stagger time waiting for an animation that won't happen.
        for (int i = frameSlots.Count - 1; i > frontier; i--)
        {
            FrameSlot slot = frameSlots[i];
            if (slot == null || slot.IsVisuallyLocked) continue;
            slot.Lock();
            yield return new WaitForSeconds(Mathf.Max(0f, cascadeStagger));
        }
    }
}

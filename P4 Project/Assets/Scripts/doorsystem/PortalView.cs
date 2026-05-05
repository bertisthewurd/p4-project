using UnityEngine;
public class PortalView : MonoBehaviour
{
    public PortalView otherPortal;
    public Camera portalView;
    public Shader portalShader;
    [SerializeField] private MeshRenderer portalMesh;
    private Material portalMaterial;
    private void Start()
    {
        otherPortal.portalView.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        portalMaterial = new Material(portalShader);
        portalMaterial.mainTexture = otherPortal.portalView.targetTexture;
        portalMesh.material = portalMaterial;
    }
    private void LateUpdate()
    {
        //position
        Vector3 lookerPosition = otherPortal.transform.worldToLocalMatrix.MultiplyPoint3x4(Camera.main.transform.position);
        lookerPosition = new Vector3(-lookerPosition.x, lookerPosition.y, -lookerPosition.z);
        portalView.transform.localPosition = lookerPosition;

        //rotation
        Quaternion diffrence = transform.rotation * Quaternion.Inverse(otherPortal.transform.rotation * Quaternion.Euler(0, 180, 0));
        portalView.transform.rotation = diffrence * Camera.main.transform.rotation;
        
    }
}
# REMEMBRANCE (P4-Project)

> A narrative exploration game about curiosity and the fragments of a forgotten past.

**[Watch the project description video](https://www.youtube.com/watch?v=WoLrpaAf5f0&t=4s)**

REMEMBRANCE is our P4 project — a 4th-semester game built for the Bachelor's Programme in Medialogy at Aalborg University, Campus Copenhagen. It is centered around curiosity and narrative.

## About the game

The game opens with an introductory, interactive cinematic sequence that introduces the player to Conan.

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/8fb835d3-266a-4c1c-a32f-47a62edc590d" />

The second half takes place in an open-world environment where you explore the fragmented mind of Conan, finding clues and solving audio puzzles that shed light on his past.

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/438d0172-012c-4080-8241-b72785609656" />

REMEMBRANCE was designed with curiosity in mind. It leaves narrative questions open-ended and rewards players for exploring and pursuing knowledge on their own initiative.

Playtests suggest that players find the experience curiosity-inducing and immersive. The audio puzzles, however, were often confusing, which is something further iterative development would likely address.

# Making a build

REMEMBRANCE is a Unity project. Before making a build:

- Make sure the video files are in the StreamingAssets folder (`\Assets\StreamingAssets`); the cinematic sequences depend on them.
- If the game runs slowly, reduce the kernel size of the post-processing shader by editing the **Kuwahara** material in the **Materials & Shaders** folder.

The video files are available here: **[Public Google Drive folder](https://drive.google.com/drive/folders/19Wmbce6KiKpqHqB2XCSj6Tp2JmasHZZA?usp=sharing)**

# Unity Client for 내 손안의 헤어샵

## overview

- client-side (application) implementation of 내 손안의 헤어샵
- developed with Unity3D version 2019.4.2f1
- demo run
  - https://drive.google.com/file/d/11s83sOdoEx2piHmpw-u77zHkfrEBjJ1C/view?usp=sharing


## UI Component

- Buttons
  - 2D image: access native gallery and load 2D image
  - 3D model: send 2D image to server and get 3D model (.data file)
    - send image via HTTP post
    - get 3D face & hair models as a file
  - Cut / Perm / Dye: modify hairstyle
    - Slider attached to control magnitude

## 3D Object Component

- Model: parent object for hair, face, and upper torso
- HairManager: generates **Hair** (saved in prefab) 
  - Hair: models a hair spline with LineRenderer
  - HairCache: saves the rendered hair statically
- FaceManager: anchor for the child component (face)


## Rendering

- Cut: change hair strand length
- Dye: pick color from palette and update model
- Perm: TODO
- Comb: TODO


## Editing

- HDRP (high definition render pipeline) for realistic hair rendering


## Third Party Assets

- ImageAndVideoPicker: gallery access in Android platform
- HairShader-HDRP: high quality hair shading 
- HairStudio: human torso



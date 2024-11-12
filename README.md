# Unity Tool: Convert Sprite-Bone Animation to Image Sequence Animation

This Unity tool allows you to convert an object with sprite-bone animation into an object with an image sequence animation. It enables easier runtime management and can help reduce performance costs associated with skeletal animations.

## Table of Contents
- [Unity Tool: Convert Sprite-Bone Animation to Image Sequence Animation](#unity-tool-convert-sprite-bone-animation-to-image-sequence-animation)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
  - [Usage](#usage)
  - [Contributing](#contributing)
  - [Contact](#contact)

## Features
- **Automated Conversion**: Easily convert sprite-bone animations to image sequences.
- **Performance Optimization**: Image sequence animations are often lighter on CPU, making them more suitable for mobile games and high-performance games.
- **User-Friendly Interface**: Simple UI within the Unity Editor for quick and easy conversion.

## Getting Started
These instructions will help you integrate and use the tool in your Unity project.

### Prerequisites
- Unity 2022.3 or later (may vary based on compatibility)
- Basic knowledge of Unity's Animation system and Sprite Editor

### Installation
1. Download or clone this repository:
    ```bash
    git clone https://github.com/sily-patel/2DAnimationConverter.git
    ```
2. This is a Unity project so you can open it directly as new or Just copy Editor file into your project's Editor folder.
3. After the import is complete, you should see the tool available in the Unity Editor.

## Usage
1. Open the Unity Editor and locate the **Convert Sprite-Bone to Image Sequence** tool under `Window > Tools > 2D Animation Converter`.
2. Prepare a Camera for capture.
   1.  Select one camera and drop it into Target Camera. This camera will have one Output texture that we will convert in to images.
   2.  In Camera, Environment, Set background type to solid color & Set alpha channel of the color to zero.
3. Select the GameObject with the sprite-bone animation you wish to convert. And drop it into Character Object field.
4. Select the desire output image size range from 32X32 to 4096X4096.
5. Click **Preview**. To see our character is with in bound of camera.
6. Click **Convert to Image Sequence**. The tool will get necessary information from the animation clip and start Unity to process each frame of the sprite-bone animation and generate an image sequence.
7. Click **Force stop task**. To stop process.
8. The converted image sequence will be saved in a desired folder in your project assets with new Prefab, Sprite atlas, animation controller and animation clip.
9. Use newly created Prefab in your game in place of old GameObject.
10. Whenever you convert again, it will replace previously created prefab object with new animation controller and new animation clip so you don't have to manually change every prefab you have used in your game scene. 

## Contributing
Contributions are welcome! If you find a bug or have a feature request, please open an issue. To contribute:
1. Fork the repository.
2. Create a new branch (`feature/my-feature`).
3. Commit your changes.
4. Push to the branch.
5. Open a pull request.


## Contact
For questions, reach out to Me [Sahil Patel](https://www.linkedin.com/in/sahil-patel-6ba064270).(sahil.patel.no3@gmail.com)

---

Thank you for using this tool! I hope it simplifies your animation process in Unity.

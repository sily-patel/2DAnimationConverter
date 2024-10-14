using UnityEngine;
using UnityEditor;
using System.IO;

//* Drag and drop your game object with animation on it
//* Hit convert
//* Generate temp texture and give it to camera
//* Use recorder to get bunch of images
//* Pack images into 2D sprite atlas
//* Create new object with new animator and controller
//* Create sprite renderer on the object
//* put image on to key frame
//* read sprite size from original file and put on to images

public class Convert2DAnimation : EditorWindow
{
    private RenderTexture renderTexture;
    private Camera selectedCamera; // Camera object to be selected by the user
    private string[] textureSizes = new string[] { "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };
    private int selectedTextureSizeIndex = 3; // Default to "1024x1024"
    private string saveFolderPath = "Assets/2D Animation Converter/Output";
    private int startFrame = 0, stopFrame = 60, currentFrameCounter = 0; // Internal frame tracking
    private bool isRecording = false;

    [MenuItem("Tools/2D Animation Converter")]
    public static void ShowWindow()
    {
        GetWindow<Convert2DAnimation>("2D Animation Converter");
    }

    private void OnGUI()
    {
        #region Initialize
        GUILayout.Label("2D Animation Converter Tool", EditorStyles.boldLabel);


        // Dropdown for texture size selection
        selectedTextureSizeIndex = EditorGUILayout.Popup("Texture Size", selectedTextureSizeIndex, textureSizes);

        // Drag and drop Camera field
        selectedCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", selectedCamera, typeof(Camera), true);







        GUILayout.Label($"Start at Frame A: {startFrame}", EditorStyles.label);
        // Select or create folder to save recorded images
        GUILayout.Label("Output Folder Path:", EditorStyles.label);
        saveFolderPath = EditorGUILayout.TextField(saveFolderPath);

        if (GUILayout.Button("Preview"))
        {
            CreateRenderTexture();
        }
        #endregion

        if (GUILayout.Button("Start Game and Start Recording"))
        {
            StartGameAndRecording();
        }

        if (GUILayout.Button("Stop Game"))
        {
            EditorApplication.isPlaying = false; // Stop the game
            isRecording = false;  // Ensure recording is stopped
        }

        if (isRecording)
        {
            GUILayout.Label($"Recording... Capturing frame {currentFrameCounter - startFrame}");
        }
        if (renderTexture != null)
        {
            GUILayout.Label("Recorder Render Texture Preview", EditorStyles.boldLabel);
            GUILayout.Box(renderTexture, GUILayout.Width(200), GUILayout.Height(200));
        }
    }

    #region Initialize
    private void CreateRenderTexture()
    {
        // Parse the selected texture size from the dropdown menu
        string[] dimensions = textureSizes[selectedTextureSizeIndex].Split('x');
        int textureWidth = int.Parse(dimensions[0]);
        int textureHeight = int.Parse(dimensions[1]);

        // Release and destroy any previous texture
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }

        renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        renderTexture.name = "GeneratedRenderTexture";

        // Ensure the directory exists
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            Debug.Log("Created folder: " + saveFolderPath);
        }

        Debug.Log($"Render Texture Created: {textureWidth}x{textureHeight}");
    }
    #endregion

    #region Recording
    private void StartGameAndRecording()
    {
        if (selectedCamera == null)
        {
            Debug.LogError("No Camera selected. Please drag and drop a camera into the field.");
            return;
        }

        EditorApplication.isPlaying = true; // Start the game
        EditorApplication.update += RecordFrames; // Start listening to frame updates
        isRecording = true;
        currentFrameCounter = 0;
        Debug.Log("Recording started. Capturing frames from A=0 to B=60.");
    }

    private void RecordFrames()
    {
        // Render the selected camera's view into the render texture
        // if (Time.frameCount >= startFrame && Time.frameCount <= stopFrame)
        {
            selectedCamera.targetTexture = renderTexture; // Set the render texture as the target
            selectedCamera.Render(); // Render the camera's view into the render texture

            // Save the render texture to a PNG file
            RenderTexture.active = renderTexture; // Set the active render texture
            Texture2D image = new Texture2D(renderTexture.width, renderTexture.height);
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply();

            byte[] bytes = image.EncodeToPNG();
            string frameFileName = Path.Combine(saveFolderPath, $"frame_{Time.frameCount}.png");
            File.WriteAllBytes(frameFileName, bytes);
            Debug.Log($"Captured Frame {Time.frameCount} at {frameFileName}");

            // Clean up
            RenderTexture.active = null; // Reset the active render texture
            DestroyImmediate(image); // Destroy the temporary texture
        }

        // Stop recording after reaching stopFrame (B)
        if (Time.frameCount >= stopFrame)
        {
            Debug.Log("Recording completed.");
            EditorApplication.update -= RecordFrames; // Stop listening to frame updates
            isRecording = false;
        }

        if (!EditorApplication.isPlaying || !isRecording)
        {
            EditorApplication.update -= RecordFrames; // Stop recording when game stops or recording stops
            return;
        }
    }
    #endregion
}

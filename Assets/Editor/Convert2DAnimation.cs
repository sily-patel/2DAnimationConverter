using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;

//* Drag and drop your game object with animation on it
//* Hit convert
//* Generate temp texture and give it to camera
//* Use recorder to get bunch of images
//* Pack images into 2D sprite atlas
//* Create new object with new animator and controller
//* Create sprite renderer on the object
//* put image on to key frame
//* read sprite size from original file and put on to images
public class EditorValues
{
    public static string CharacterObjectInstanceIDKey = "CharacterObjectPath";
    public static string SelectedCameraInstanceIDKey = "SelectedCameraPath";

    public static string OutputFolderPathKey = "OutputFolderPath";
    public static string DefaultOutputFolderPath = "Assets/2D Animation Converter/Output";

}
public class Convert2DAnimation : EditorWindow
{
    private RenderTexture renderTexture;
    private Camera selectedCamera; // Camera object to be selected by the user
    private GameObject characterObject;
    private string[] textureSizes = new string[] { "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };
    private int selectedTextureSizeIndex = 3; // Default to "1024x1024"

    private int startFrame = 0, stopFrame = 60, currentFrameCounter = 0; // Internal frame tracking
    private bool isRecording = false;
    string outputFolderPath = "", finalOutputPath = "";

    [MenuItem("Tools/2D Animation Converter")]
    public static void ShowWindow()
    {
        GetWindow<Convert2DAnimation>("2D Animation Converter");
    }
    private void OnEnable()
    {
        LoadWindowState();
    }
    private void OnDisable()
    {
        SaveWindowState();
    }

    private void OnGUI()
    {
        #region Initialize

        GUILayout.Label("2D Animation Converter Tool", EditorStyles.boldLabel);
        selectedTextureSizeIndex = EditorGUILayout.Popup("Texture Size", selectedTextureSizeIndex, textureSizes);
        selectedCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", selectedCamera, typeof(Camera), true);
        EditorGUILayout.Space();

        characterObject = (GameObject)EditorGUILayout.ObjectField("Character Object", characterObject, typeof(GameObject), true);
        EditorGUILayout.Space();

        finalOutputPath = characterObject != null ? outputFolderPath + ("/" + characterObject.name) : outputFolderPath;
        EditorGUILayout.LabelField("Output Folder:", finalOutputPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Select Output Folder", GUILayout.Width(150)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets/", "");
            if (selectedPath.StartsWith(Application.dataPath))
                outputFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
            else
                Debug.LogWarning("Selected folder is not within the Assets folder.");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        #endregion


        GUILayout.Label($"Start at Frame A: {startFrame}", EditorStyles.label);
        EditorGUILayout.Space();

        // if (GUILayout.Button("Preview"))
        // {
        //     CreateRenderTexture();
        // }
        // #endregion

        // if (GUILayout.Button("Start Game and Start Recording"))
        // {
        //     StartGameAndRecording();
        // }

        // if (GUILayout.Button("Stop Game"))
        // {
        //     EditorApplication.isPlaying = false; // Stop the game
        //     isRecording = false;  // Ensure recording is stopped
        // }

        // if (isRecording)
        // {
        //     GUILayout.Label($"Recording... Capturing frame {currentFrameCounter - startFrame}");
        // }
        // if (renderTexture != null)
        // {
        //     GUILayout.Label("Recorder Render Texture Preview", EditorStyles.boldLabel);
        //     GUILayout.Box(renderTexture, GUILayout.Width(200), GUILayout.Height(200));
        // }
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

        Debug.Log($"Render Texture Created: {textureWidth}x{textureHeight}");
    }
    // Saving the data using EditorPrefs (works even after closing the editor)
    private void SaveWindowState()
    {
        if (characterObject != null)
            EditorPrefs.SetInt(EditorValues.CharacterObjectInstanceIDKey, characterObject.GetInstanceID());
        else
            EditorPrefs.DeleteKey(EditorValues.CharacterObjectInstanceIDKey);

        if (selectedCamera != null)
            EditorPrefs.SetInt(EditorValues.SelectedCameraInstanceIDKey, selectedCamera.GetInstanceID());
        else
            EditorPrefs.DeleteKey(EditorValues.SelectedCameraInstanceIDKey);

        if (outputFolderPath != "")
            EditorPrefs.SetString(EditorValues.OutputFolderPathKey, outputFolderPath);

    }
    private void LoadWindowState()
    {
        int objID = EditorPrefs.GetInt(EditorValues.CharacterObjectInstanceIDKey, 0);
        if (objID != 0)
            characterObject = (GameObject)EditorUtility.InstanceIDToObject(objID);

        objID = EditorPrefs.GetInt(EditorValues.SelectedCameraInstanceIDKey, 0);
        if (objID != 0)
            selectedCamera = EditorUtility.InstanceIDToObject(objID).GetComponent<Camera>();

        outputFolderPath = EditorPrefs.GetString(EditorValues.OutputFolderPathKey, EditorValues.DefaultOutputFolderPath);
    }

    private void MakeFolderAvailable(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
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
            string frameFileName = Path.Combine(outputFolderPath, $"frame_{Time.frameCount}.png");
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

using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEditor.VersionControl;
using UnityEditor.Animations;

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

    public static int TargetFrameRate = 60;
}
public class Convert2DAnimation : EditorWindow
{
    private RenderTexture renderTexture;
    private Camera selectedCamera; // Camera object to be selected by the user
    private GameObject characterObject;
    private Animator characterAnimator;
    AnimationClip characterAnimationClip;
    private string[] textureSizes = new string[] { "32X32", "64X64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };
    private int selectedTextureSizeIndex = 2; // Default to "128x128"

    private int startFrame = 0, stopFrame = 60, currentFrameCounter = 0; // Internal frame tracking
    private bool isRecording = false, recordingSuccessful = false;
    string outputFolderPath = "", finalOutputPath = "";

    [MenuItem("Tools/2D Animation Converter")]
    public static void ShowWindow()
    {
        GetWindow<Convert2DAnimation>("2D Animation Converter");
    }
    private void OnEnable()
    {
        LoadWindowState();
        Application.targetFrameRate = EditorValues.TargetFrameRate;
    }
    private void OnDisable()
    {
        SaveWindowState();
    }

    private void OnGUI()
    {
        #region Initialize

        GUILayout.Label("2D Sprite Bones Animation to Image Sequence Animation Converter Tool", EditorStyles.boldLabel);
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

        if (GUILayout.Button("Preview"))
        {
            CreateRenderTexture();
            PrepareCamera();
        }

        if (GUILayout.Button("Take image"))
        {
            TakeImageFromTextureAndSave();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Convert to Image Sequence"))
        {
            StartGameAndRecording();
            // recordingSuccessful = true;
            // FinalStep();
        }

        if (GUILayout.Button("Force Stop Task"))
        {
            EditorApplication.isPlaying = false; // Stop the game
            isRecording = false;  // Ensure recording is stopped
        }

        if (renderTexture != null)
        {
            EditorGUILayout.Space();

            // GUILayout.Label("Recorder Render Texture Preview", EditorStyles.label);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(renderTexture, GUILayout.Width(150), GUILayout.Height(150));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.Label($"Target FrameRate: {Application.targetFrameRate}", EditorStyles.label);
        if (isRecording)
        {
            GUILayout.Label($"Capturing frames from {startFrame} to {stopFrame}.");
            GUILayout.Label($"Capturing frame {currentFrameCounter}");
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
            if (selectedCamera != null) selectedCamera.targetTexture = null;
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }

        renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        renderTexture.name = "GeneratedRenderTexture";

        Debug.Log($"Render Texture Created: {textureWidth}x{textureHeight}");


    }
    private void PrepareCamera()
    {
        if (selectedCamera != null)
        {
            selectedCamera.targetTexture = renderTexture; // Set the render texture as the target
            selectedCamera.Render(); // Render the camera's view into the render texture
        }
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
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
    }

    private void DeleteFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            FileUtil.DeleteFileOrDirectory(folderPath);
            AssetDatabase.Refresh();
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
        if (characterObject == null)
        {
            Debug.LogError("No character object selected. Please drag and drop a character object into the field.");
            return;
        }
        characterAnimator = characterObject.GetComponent<Animator>();
        if (characterAnimator == null)
        {
            Debug.LogError("No animator attached to character object");
            return;
        }

        characterAnimationClip = characterAnimator.runtimeAnimatorController.animationClips[0];

        EditorApplication.isPlaying = true; // Start the game
        EditorApplication.update += FrameUpdate; // Start listening to frame updates
        recordingSuccessful = false;
        isRecording = true;
        currentFrameCounter = 0;
        startFrame = Mathf.RoundToInt(characterAnimationClip.length * characterAnimationClip.frameRate);
        stopFrame = startFrame * 2;
        Application.targetFrameRate = (int)characterAnimationClip.frameRate;

        string savePath = outputFolderPath + ("/" + characterObject.name) + "/images";
        DeleteFolder(savePath);
        MakeFolderAvailable(savePath);

        savePath = outputFolderPath + ("/" + characterObject.name) + "/animation";
        DeleteFolder(savePath);
        MakeFolderAvailable(savePath);

        PrepareCamera();
    }

    private void TakeImageFromTextureAndSave()
    {
        string savePath = outputFolderPath + ("/" + characterObject.name) + "/images";

        RenderTexture.active = renderTexture; // Set the active render texture
        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();
        byte[] bytes = image.EncodeToPNG();
        string frameFileName = Path.Combine(savePath, $"frame_{Time.frameCount}.png");
        File.WriteAllBytes(frameFileName, bytes);
        // Debug.Log($"Captured Frame {Time.frameCount} at {frameFileName}");

        // Clean up
        RenderTexture.active = null; // Reset the active render texture
        DestroyImmediate(image); // Destroy the temporary texture

    }
    private void FrameUpdate()
    {
        RecordFrames();
    }
    private void RecordFrames()
    {
        // Render the selected camera's view into the render texture
        if (Time.frameCount >= startFrame && Time.frameCount < stopFrame)
        {
            TakeImageFromTextureAndSave();
        }

        // Stop recording after reaching stopFrame (B)
        if (Time.frameCount >= stopFrame || !isRecording)
        {
            Debug.Log("Recording completed.");
            EditorApplication.update -= FrameUpdate; // Stop listening to frame updates
            EditorApplication.isPlaying = false;
            recordingSuccessful = true;
            FinalStep();
            isRecording = false;
        }
        currentFrameCounter++;
    }

    private void FinalStep()
    {
        if (recordingSuccessful)
        {
            AssetDatabase.Refresh();
            string imagesPath = outputFolderPath + ("/" + characterObject.name) + "/images";
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { imagesPath });

            if (guids.Length == 0)
            {
                Debug.LogError("No sprites found in " + imagesPath);
                return;
            }

            string spriteAtlasPath = outputFolderPath + ("/" + characterObject.name) + ("/" + characterObject.name) + ".spriteatlas";
            SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasPath);
            if (spriteAtlas == null)
            {
                spriteAtlas = new SpriteAtlas();
            }

            SpriteAtlasPackingSettings spriteAtlasPackingSettings = new SpriteAtlasPackingSettings
            {
                enableRotation = false,
                enableTightPacking = false,
                padding = 2
            };
            spriteAtlas.SetPackingSettings(spriteAtlasPackingSettings);
            spriteAtlas.Remove(spriteAtlas.GetPackables());

            float pixelPerUnit = -1;

            List<Sprite> sprites = new List<Sprite>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    if (pixelPerUnit != -1)
                        textureImporter.spritePixelsPerUnit = pixelPerUnit;
                    textureImporter.SaveAndReimport();
                    spriteAtlas.Add(new[] { sprite });
                    sprites.Add(sprite);
                }
            }
            if (AssetDatabase.Contains(spriteAtlas))
                EditorUtility.SetDirty(spriteAtlas);
            else
                AssetDatabase.CreateAsset(spriteAtlas, spriteAtlasPath);

            Debug.Log($"SpriteAtlas created at {spriteAtlasPath}");


            AnimationClip animationClip = new AnimationClip();
            animationClip.frameRate = characterAnimationClip.frameRate;
            animationClip.wrapMode = WrapMode.Loop;
            SerializedObject serializedClip = new SerializedObject(animationClip);
            serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = true;
            serializedClip.ApplyModifiedProperties();

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
            float animationClipFrameRate = animationClip.frameRate;
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / animationClipFrameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, keyframes);

            string animationPath = outputFolderPath + ("/" + characterObject.name) + "/animation";

            // Create Animator Controller
            AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(Path.Combine(animationPath, characterObject.name + ".controller"));
            animatorController.AddMotion(animationClip);

            // Save the Animation Clip
            AssetDatabase.CreateAsset(animationClip, Path.Combine(animationPath, characterAnimationClip.name + ".anim"));

            string prefabPath = Path.Combine(outputFolderPath + ("/" + characterObject.name), characterObject.name + "_.prefab");

            GameObject finalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (finalPrefab == null)
            {
                // If prefab doesn't exist, create a new one
                finalPrefab = new GameObject(characterObject.name + "_");
                finalPrefab.AddComponent<SpriteRenderer>();
            }

            SpriteRenderer spriteRenderer = finalPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = finalPrefab.AddComponent<SpriteRenderer>();

            Animator animator = finalPrefab.GetComponent<Animator>();
            if (animator == null)
                animator = finalPrefab.AddComponent<Animator>();

            animator.runtimeAnimatorController = animatorController;

            PrefabUtility.SaveAsPrefabAsset(finalPrefab, prefabPath);

            // Clean up
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    #endregion

    // SpriteRenderer characterSpriteRenderer = null;
    // if (characterObject != null)
    // {
    //     if (characterObject.transform.childCount > 0)
    //     {
    //         foreach (var spriteRenderer in characterObject.GetComponentsInChildren<SpriteRenderer>())
    //         {
    //             characterSpriteRenderer = spriteRenderer;
    //             break;
    //         }
    //     }
    //     else
    //     {
    //         characterSpriteRenderer = characterObject.transform.GetComponent<SpriteRenderer>();
    //     }
    // }

    // if (characterSpriteRenderer != null)
    // {
    //     string characterSpritePath = AssetDatabase.GetAssetPath(characterSpriteRenderer.sprite.texture);
    //     TextureImporter textureImporter = AssetImporter.GetAtPath(characterSpritePath) as TextureImporter;
    //     if (textureImporter != null)
    //         pixelPerUnit = textureImporter.spritePixelsPerUnit;
    // }

    // Debug.Log("pixelPerUnit:" + pixelPerUnit);
}

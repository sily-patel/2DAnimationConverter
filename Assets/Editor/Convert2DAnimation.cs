using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEditor.Animations;

public class EditorValues
{
    public static string CharacterObjectInstanceIDKey = "CharacterObjectInstanceIDKey";
    public static string SelectedCameraInstanceIDKey = "SelectedCameraInstanceIDKey";
    public static string selectedTextureSizeIndexKey = "selectedTextureSizeIndexKey";
    public static string OutputFolderPathKey = "OutputFolderPathKey";
    public static string DefaultOutputFolderPath = "Assets/2D Animation Converter/Output";
    public static int DefaultTargetFrameRate = 60;
    public static int DefaultSelectedTextureSizeIndex = 2;// Default to "128x128"
}
public class Convert2DAnimation : EditorWindow
{
    RenderTexture renderTexture;
    Camera selectedCamera;
    GameObject characterObject;
    Animator characterAnimator;
    AnimationClip characterAnimationClip;

    string[] textureSizes = new string[] { "32X32", "64X64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };
    int selectedTextureSizeIndex = 2; // Default to "128x128"
    int startFrame = 0, stopFrame = 60, currentFrameCounter = 0;
    bool isRecording = false, recordingSuccessful = false;
    string outputFolderPath = "";

    [MenuItem("Tools/2D Animation Converter")]
    public static void ShowWindow()
    {
        GetWindow<Convert2DAnimation>("2D Animation Converter");
    }

    void OnEnable()
    {
        loadWindowState();
        Application.targetFrameRate = EditorValues.DefaultTargetFrameRate;
    }
    void OnDisable()
    {
        saveWindowState();
    }

    void OnGUI()
    {
        GUILayout.Label("2D Sprite Bones Animation to Image Sequence Animation Converter Tool", EditorStyles.boldLabel);

        ui_RequiredComponents();

        ui_FolderPath();

        EditorGUILayout.Space();

        button_Preview();

        button_StartRecording();

        button_StopRecording();

        display_Texture();

        display_RecordingStatus();
    }

    #region GUI
    void ui_RequiredComponents()
    {
        selectedTextureSizeIndex = EditorGUILayout.Popup("Texture Size", selectedTextureSizeIndex, textureSizes);
        selectedCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", selectedCamera, typeof(Camera), true);
        characterObject = (GameObject)EditorGUILayout.ObjectField("Character Object", characterObject, typeof(GameObject), true);
        EditorGUILayout.Space();
    }
    void ui_FolderPath()
    {
        string finalOutputPath = characterObject != null ? outputFolderPath + ("/" + characterObject.name) : outputFolderPath;
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
    }
    void display_Texture()
    {
        if (renderTexture != null)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(renderTexture, GUILayout.Width(150), GUILayout.Height(150));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
    }
    void display_RecordingStatus()
    {
        GUILayout.Label($"Target FrameRate: {Application.targetFrameRate}", EditorStyles.label);
        if (isRecording)
        {
            GUILayout.Label($"Capturing frames from {startFrame} to {stopFrame}.");
            GUILayout.Label($"Capturing frame {currentFrameCounter}");
        }
    }
    void button_Preview()
    {
        if (GUILayout.Button("Preview"))
        {
            createRenderTexture();
            prepareCamera();
        }
        EditorGUILayout.Space();
    }
    void button_StartRecording()
    {
        if (GUILayout.Button("Convert to Image Sequence"))
        {
            startGameAndRecording();
        }
        EditorGUILayout.Space();
    }
    void button_StopRecording()
    {
        if (GUILayout.Button("Force Stop Task"))
        {
            EditorApplication.isPlaying = false;
            isRecording = false;
        }
        EditorGUILayout.Space();
    }
    #endregion

    #region Initialize
    void createRenderTexture()
    {
        string[] dimensions = textureSizes[selectedTextureSizeIndex].Split('x');
        int textureWidth = int.Parse(dimensions[0]);
        int textureHeight = int.Parse(dimensions[1]);

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
    void prepareCamera()
    {
        if (selectedCamera != null)
        {
            selectedCamera.targetTexture = renderTexture;
            selectedCamera.Render();
        }
    }
    void saveWindowState()
    {
        if (characterObject != null)
            EditorPrefs.SetInt(EditorValues.CharacterObjectInstanceIDKey, characterObject.GetInstanceID());
        else
            EditorPrefs.DeleteKey(EditorValues.CharacterObjectInstanceIDKey);

        if (selectedCamera != null)
            EditorPrefs.SetInt(EditorValues.SelectedCameraInstanceIDKey, selectedCamera.GetInstanceID());
        else
            EditorPrefs.DeleteKey(EditorValues.SelectedCameraInstanceIDKey);

        EditorPrefs.SetInt(EditorValues.selectedTextureSizeIndexKey, selectedTextureSizeIndex);

        if (outputFolderPath != "")
            EditorPrefs.SetString(EditorValues.OutputFolderPathKey, outputFolderPath);

    }
    void loadWindowState()
    {
        int objID = EditorPrefs.GetInt(EditorValues.CharacterObjectInstanceIDKey, 0);
        if (objID != 0)
            characterObject = (GameObject)EditorUtility.InstanceIDToObject(objID);

        objID = EditorPrefs.GetInt(EditorValues.SelectedCameraInstanceIDKey, 0);
        if (objID != 0)
            selectedCamera = EditorUtility.InstanceIDToObject(objID).GetComponent<Camera>();

        selectedTextureSizeIndex = EditorPrefs.GetInt(EditorValues.selectedTextureSizeIndexKey, EditorValues.DefaultSelectedTextureSizeIndex);


        outputFolderPath = EditorPrefs.GetString(EditorValues.OutputFolderPathKey, EditorValues.DefaultOutputFolderPath);
    }
    void makeFolderAvailable(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
    }
    void deleteFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            FileUtil.DeleteFileOrDirectory(folderPath);
            AssetDatabase.Refresh();
        }
    }
    #endregion

    #region Recording
    void startGameAndRecording()
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

        EditorApplication.isPlaying = true;
        EditorApplication.update += frameUpdateTask;
        recordingSuccessful = false;
        isRecording = true;
        currentFrameCounter = 0;
        startFrame = Mathf.RoundToInt(characterAnimationClip.length * characterAnimationClip.frameRate);
        stopFrame = startFrame * 2;
        Application.targetFrameRate = (int)characterAnimationClip.frameRate;

        string savePath = outputFolderPath + ("/" + characterObject.name) + "/images";
        deleteFolder(savePath);
        makeFolderAvailable(savePath);

        savePath = outputFolderPath + ("/" + characterObject.name) + "/animation";
        deleteFolder(savePath);
        makeFolderAvailable(savePath);

        createRenderTexture();
        prepareCamera();
    }
    void frameUpdateTask()
    {
        recordFrames();
    }
    void recordFrames()
    {
        if (Time.frameCount >= startFrame && Time.frameCount < stopFrame)
            takeImageFromTextureAndSave();

        if (Time.frameCount >= stopFrame || !isRecording)
        {
            Debug.Log("Recording completed.");
            EditorApplication.update -= frameUpdateTask;
            EditorApplication.isPlaying = false;
            recordingSuccessful = true;
            finalStep();
            isRecording = false;
        }
        currentFrameCounter++;
    }
    void takeImageFromTextureAndSave()
    {
        string savePath = outputFolderPath + ("/" + characterObject.name) + "/images";

        RenderTexture.active = renderTexture;
        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();
        byte[] bytes = image.EncodeToPNG();
        string frameFileName = Path.Combine(savePath, $"frame_{Time.frameCount}.png");
        File.WriteAllBytes(frameFileName, bytes);

        RenderTexture.active = null;
        DestroyImmediate(image);

    }
    void finalStep()
    {
        if (recordingSuccessful)
        {
            AssetDatabase.Refresh();

            //* Get all saved images from output folder
            string imagesPath = outputFolderPath + ("/" + characterObject.name) + "/images";
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { imagesPath });

            if (guids.Length == 0)
            {
                Debug.LogError("No sprites found in " + imagesPath);
                return;
            }

            //* Create Sprite Atlas
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

            //* Get all images
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

            //* Create Animation clip and Controller
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

            AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(Path.Combine(animationPath, characterObject.name + ".controller"));
            animatorController.AddMotion(animationClip);
            //* Save animation
            AssetDatabase.CreateAsset(animationClip, Path.Combine(animationPath, characterAnimationClip.name + ".anim"));

            string prefabPath = Path.Combine(outputFolderPath + ("/" + characterObject.name), characterObject.name + "_.prefab");

            //* Create Prefab
            GameObject finalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (finalPrefab == null)
            {
                finalPrefab = new GameObject(characterObject.name + "_");
                finalPrefab.AddComponent<SpriteRenderer>();
            }

            SpriteRenderer spriteRenderer = finalPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = finalPrefab.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprites[0];

            Animator animator = finalPrefab.GetComponent<Animator>();
            if (animator == null)
                animator = finalPrefab.AddComponent<Animator>();

            animator.runtimeAnimatorController = animatorController;

            //* Save Prefab
            PrefabUtility.SaveAsPrefabAsset(finalPrefab, prefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Task successful. Prefab saved at:" + prefabPath);
        }
        else
        {
            Debug.Log("Task fail");
        }
    }
    #endregion
}

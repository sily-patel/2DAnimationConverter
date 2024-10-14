using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationInspectorTool : EditorWindow
{
    private GameObject selectedObject;
    private Animator animator;
    private AnimationClip defaultClip;
    private float animationTime;
    private int totalFrames;

    [MenuItem("Tools/Animation Inspector")]
    public static void ShowWindow()
    {
        GetWindow<AnimationInspectorTool>("Animation Inspector");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag and Drop Object with Animator", EditorStyles.boldLabel);

        // Drag and Drop area
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag Object Here");

        if (evt.type == EventType.DragUpdated && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        else if (evt.type == EventType.DragPerform && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.AcceptDrag();

            foreach (var draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject)
                {
                    selectedObject = (GameObject)draggedObject;
                    animator = selectedObject.GetComponent<Animator>();

                    if (animator != null)
                    {
                        if (animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips.Length > 0)
                        {
                            defaultClip = animator.runtimeAnimatorController.animationClips[0]; // Default to the first animation clip
                            animationTime = defaultClip.length;
                            totalFrames = Mathf.RoundToInt(defaultClip.length * defaultClip.frameRate);
                        }
                        else
                        {
                            defaultClip = null;
                            animationTime = 0;
                            totalFrames = 0;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("The object does not have an Animator component.");
                    }
                }
            }
        }

        // Display information if the Animator and defaultClip are set
        if (animator != null && defaultClip != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selected Object:", selectedObject.name);
            EditorGUILayout.LabelField("Animator Found:", animator.name);
            EditorGUILayout.LabelField("Default Animation Clip:", defaultClip.name);
            EditorGUILayout.LabelField("Animation Duration (seconds):", animationTime.ToString("F2"));
            EditorGUILayout.LabelField("Total Frames:", totalFrames.ToString());
            EditorGUILayout.LabelField("frameRate:", defaultClip.frameRate.ToString());
        }
        else if (selectedObject != null && animator == null)
        {
            EditorGUILayout.HelpBox("The selected object does not have an Animator component.", MessageType.Warning);
        }
    }
}

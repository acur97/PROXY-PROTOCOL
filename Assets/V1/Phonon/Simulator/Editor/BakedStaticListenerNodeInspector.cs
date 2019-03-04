//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Phonon
{
    //
    // BakedStaticListenerInspector
    // Custom inspector for BakedStaticListener.
    //

    [CustomEditor(typeof(BakedStaticListenerNode))]
    public class BakedStaticListenerNodeInspector : Editor
    {
        //
        // Draws the inspector GUI.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PhononGUI.SectionHeader("Baked Static Listener Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uniqueIdentifier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bakingRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));

            BakedStaticListenerNode bakedStaticListener = serializedObject.targetObject as BakedStaticListenerNode;
            bakedStaticListener.uniqueIdentifier = bakedStaticListener.uniqueIdentifier.Trim();
            if (bakedStaticListener.uniqueIdentifier.Length == 0)
                EditorGUILayout.HelpBox("You must specify a unique identifier name.", MessageType.Warning);

            if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

            GUI.enabled = !bakedStaticListener.phononBaker.IsBakeActive();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Bake Effect"))
            {
                if (bakedStaticListener.uniqueIdentifier.Length == 0)
                    Debug.LogError("You must specify a unique identifier name.");
                else
                {
                    Debug.Log("START: Baking effect for \"" + bakedStaticListener.uniqueIdentifier + "\".");
                    bakedStaticListener.BeginBake();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            DisplayProgressBarAndCancel();

            if (bakedStaticListener.phononBaker.GetBakeStatus() == BakeStatus.Complete)
            {
                bakedStaticListener.EndBake();
                Repaint();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("COMPLETED: Baking effect for \"" + bakedStaticListener.uniqueIdentifier + "\".");
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DisplayProgressBarAndCancel()
        {
            BakedStaticListenerNode bakedStaticListener = serializedObject.targetObject as BakedStaticListenerNode;
            bakedStaticListener.phononBaker.DrawProgressBar();
            Repaint();
        }

        static public bool oneBakeActive = false;
    }
}
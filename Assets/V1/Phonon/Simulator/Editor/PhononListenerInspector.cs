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
    // PhononMixerInspector
    // Custom inspector for the PhononMixer component.
    //
    [CustomEditor(typeof(PhononListener))]
    public class PhononListenerInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PhononGUI.SectionHeader("Mixer Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("acceleratedMixing"));

            if (!serializedObject.FindProperty("acceleratedMixing").boolValue)
            {
                PhononGUI.SectionHeader("Reverb Settings");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableReverb"));

                if (serializedObject.FindProperty("enableReverb").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reverbSimulationType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dryMixFraction"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reverbMixFraction"));

                    PhononListener phononListener = serializedObject.targetObject as PhononListener;
                    if (phononListener.reverbSimulationType == ReverbSimulationType.BakedReverb)
                    {
                        BakedReverbGUI();
                    }
                }
            }

            PhononGUI.SectionHeader("Rendering Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectBinauralEnabled"));

            if (serializedObject.FindProperty("acceleratedMixing").boolValue && serializedObject.FindProperty("indirectBinauralEnabled").boolValue)
                EditorGUILayout.HelpBox("The binaural settings on Phonon Source will be ignored.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        //
        // GUI for BakedReverb
        //
        public void BakedReverbGUI()
        {
            PhononGUI.SectionHeader("Baked Reverb Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));

            if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

            PhononListener bakedReverb = serializedObject.targetObject as PhononListener;
            GUI.enabled = !bakedReverb.phononBaker.IsBakeActive();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Bake Reverb"))
            {
                Debug.Log("START: Baking reverb effect.");
                bakedReverb.BeginBake();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            DisplayProgressBarAndCancel();

            if (bakedReverb.phononBaker.GetBakeStatus() == BakeStatus.Complete)
            {
                bakedReverb.EndBake();
                Repaint();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("COMPLETED: Baking reverb effect.");
            }
        }

        void DisplayProgressBarAndCancel()
        {
            PhononListener bakedReverb = serializedObject.targetObject as PhononListener;
            bakedReverb.phononBaker.DrawProgressBar();
            Repaint();
        }
    }
}
//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

using UnityEngine;

namespace Phonon
{
    //
    // SourceSimulationType
    // Various simulation options for a PhononSource.
    //
    public enum SourceSimulationType
    {
        Realtime,
        BakedStaticSource,
        BakedStaticListener
    }

    //
    // PhononSource
    // Enables physics-based modeling for any object with AudioSource component.
    //
    [AddComponentMenu("Phonon/Phonon Source")]
    public class PhononSource : MonoBehaviour
    {
        //
        // Initializes the effect.
        //
        void Awake()
        {
            audioEngine = AudioEngineComponent.GetAudioEngine();
            if (audioEngine == AudioEngine.Unity)
            {
                directSimulator.Init();
                indirectSimulator.Init();

                environmentalRenderer = FindObjectOfType<EnvironmentalRendererComponent>();
                environment = FindObjectOfType<EnvironmentComponent>();
                if ((environmentalRenderer == null || environment == null) && (enableReflections || directOcclusionOption != OcclusionOption.None))
                    Debug.LogError("Environment and Environmental Renderer component must be attached when reflections are enabled or direct occlusion is set to Raycast or Partial.");
            }
        }

        private void OnEnable()
        {
            StartCoroutine(EndOfFrameUpdate());
        }

        //
        // Destroys the effect.
        //
        void OnDestroy()
        {
            mutex.WaitOne();
            destroying = true;
            errorLogged = false;

            if (audioEngine == AudioEngine.Unity)
            {
                directSimulator.Destroy();
                indirectSimulator.Destroy();
            }

            mutex.ReleaseMutex();
        }

        //
        // Courutine to update source and listener position and orientation at frame end.
        // Done this way to ensure correct update in VR setup.
        //
        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                if (!initialized && environmentalRenderer != null && environmentalRenderer.GetEnvironmentalRenderer() != IntPtr.Zero && environment != null && environment.Environment().GetEnvironment() != IntPtr.Zero)
                {
                    indirectSimulator.FrameInit(environmentalRenderer.GetEnvironmentalRenderer(), true, 
                        sourceSimulationType, ReverbSimulationType.RealtimeReverb, uniqueIdentifier);
                    initialized = true;
                }

                if (!errorLogged && environment != null && environment.Scene().GetScene() == IntPtr.Zero
                    && environment.ProbeManager().GetProbeManager() != IntPtr.Zero
                    && ((directOcclusionOption != OcclusionOption.None) || enableReflections))
                {
                    Debug.LogError("Scene not found. Make sure to pre-export the scene.");
                    errorLogged = true;
                }

                if (!initialized && (directOcclusionOption == OcclusionOption.None) && !enableReflections)
                    initialized = true;

                UpdateRelativeDirection();
                IntPtr envRenderer = (environmentalRenderer != null) ? environmentalRenderer.GetEnvironmentalRenderer() : IntPtr.Zero;
                indirectSimulator.FrameUpdate(true, sourceSimulationType, ReverbSimulationType.RealtimeReverb);
                directSimulator.FrameUpdate(envRenderer, sourcePosition, listenerPosition, listenerAhead, listenerUp, 
                    partialOcclusionRadius, directOcclusionOption);

                yield return new WaitForEndOfFrame();   // Must yield after updating the relative direction.
            }
        }

        //
        // Updates the direction of the source relative to the listener.
        // Wait until the end of the frame to update the position to get latest information.
        //
        private void UpdateRelativeDirection()
        {
            if ((listener = listener ?? FindObjectOfType<AudioListener>()) == null) return;

            sourcePosition = Common.ConvertVector(transform.position);
            listenerPosition = Common.ConvertVector(listener.transform.position);
            listenerAhead = Common.ConvertVector(listener.transform.forward);
            listenerUp = Common.ConvertVector(listener.transform.up);
        }

        //
        // Applies propagtion effects to dry audio.
        //
        void OnAudioFilterRead(float[] data, int channels)
        {
            mutex.WaitOne();

            if (data == null)
            {
                mutex.ReleaseMutex();
                return;
            }

            if (!initialized || destroying)
            {
                mutex.ReleaseMutex();
                Array.Clear(data, 0, data.Length);
                return;
            }

            float[] wetData = indirectSimulator.AudioFrameUpdate(data, channels, sourcePosition, listenerPosition, 
                listenerAhead, listenerUp, enableReflections, indirectMixFraction, indirectBinauralEnabled); //data is copied, must be used before directSimulator which modifies the data.

            directSimulator.AudioFrameUpdate(data, channels, physicsBasedAttenuation, directMixFraction, 
                directBinauralEnabled, hrtfInterpolation);

            if (wetData != null && wetData.Length != 0)
                for (int i = 0; i < data.Length; ++i)
                    data[i] += wetData[i];

            mutex.ReleaseMutex();
        }

        public void BeginBake()
        {
            Sphere bakeSphere;
            Vector3 sphereCenter = Common.ConvertVector(gameObject.transform.position);
            bakeSphere.centerx = sphereCenter.x;
            bakeSphere.centery = sphereCenter.y;
            bakeSphere.centerz = sphereCenter.z;
            bakeSphere.radius = bakingRadius;

            if (useAllProbeBoxes)
                phononBaker.BeginBake(FindObjectsOfType<ProbeBox>() as ProbeBox[], BakingMode.StaticSource, 
                    uniqueIdentifier, bakeSphere);
            else
                phononBaker.BeginBake(probeBoxes, BakingMode.StaticSource, uniqueIdentifier, bakeSphere);
        }

        public void EndBake()
        {
            phononBaker.EndBake();
        }

        void OnDrawGizmosSelected()
        {
            if (sourceSimulationType == SourceSimulationType.BakedStaticSource)
            {
                Color oldColor = Gizmos.color;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(gameObject.transform.position, bakingRadius);

                Gizmos.color = Color.magenta;
                ProbeBox[] drawProbeBoxes = probeBoxes;
                if (useAllProbeBoxes)
                    drawProbeBoxes = FindObjectsOfType<ProbeBox>() as ProbeBox[];

                if (drawProbeBoxes != null)
                    foreach (ProbeBox probeBox in drawProbeBoxes)
                        if (probeBox != null)
                            Gizmos.DrawWireCube(probeBox.transform.position, probeBox.transform.localScale);

                Gizmos.color = oldColor;
            }
        }

        // Public fields - direct sound.
        public bool directBinauralEnabled = true;
        public HRTFInterpolation hrtfInterpolation;
        public OcclusionOption directOcclusionOption;
        [Range(.1f, 32f)]
        public float partialOcclusionRadius = 1.0f;
        public bool physicsBasedAttenuation = true;
        [Range(.0f, 1.0f)]
        public float directMixFraction = 1.0f;

        // Public fields - indirect sound.
        public bool enableReflections = false;
        public SourceSimulationType sourceSimulationType;
        [Range(.0f, 10.0f)]
        public float indirectMixFraction = 1.0f;
        public bool indirectBinauralEnabled = false;

        // Public fields - indirect baking.
        public string uniqueIdentifier = "";
        [Range(1f, 1024f)]
        public float bakingRadius = 16f;
        public bool useAllProbeBoxes = false;
        public ProbeBox[] probeBoxes = null;
        public PhononBaker phononBaker = new PhononBaker();

        // Private fields.
        AudioEngine audioEngine;
        AudioListener listener;
        EnvironmentalRendererComponent environmentalRenderer;
        EnvironmentComponent environment;

        AudioFormat inputFormat;
        AudioFormat outputFormat;
        AudioFormat ambisonicsFormat;

        Vector3 sourcePosition;
        Vector3 listenerPosition;
        Vector3 listenerAhead;
        Vector3 listenerUp;

        Mutex mutex = new Mutex();

        bool initialized = false;
        bool destroying = false;
        bool errorLogged = false;

        DirectSimulator directSimulator = new DirectSimulator();
        IndirectSimulator indirectSimulator = new IndirectSimulator();
    }
}

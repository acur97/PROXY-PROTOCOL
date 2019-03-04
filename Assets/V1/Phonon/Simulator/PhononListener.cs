//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using System.Collections;
using System.Threading;

using UnityEngine;

namespace Phonon
{
    public enum ReverbSimulationType
    {
        RealtimeReverb,
        BakedReverb,
    }

    //
    // PhononListener
    // Represents a Phonon Listener. Performs optimized mixing in fourier
    // domain or apply reverb.
    //

    [AddComponentMenu("Phonon/Phonon Listener")]
    public class PhononListener : MonoBehaviour
    {
        //
        // Initializes the Phonon Listener.
        //
        void Awake()
        {
            audioEngine = AudioEngineComponent.GetAudioEngine();
            if (audioEngine == AudioEngine.Unity)
            {
                indirectMixer.Init();
                indirectSimulator.Init();

                environmentalRenderer = FindObjectOfType<EnvironmentalRendererComponent>();
                environment = FindObjectOfType<EnvironmentComponent>();
                if ((environmentalRenderer == null || environment == null) && enableReverb)
                    Debug.LogError("Environment and Environmental Renderer component must be attached when modeling reverb.");
            }
        }

        private void OnEnable()
        {
            StartCoroutine(EndOfFrameUpdate());
        }

        //
        // Destroys the listener
        //
        void OnDestroy()
        {
            mutex.WaitOne();
            destroying = true;

            if (audioEngine == AudioEngine.Unity)
            {
                indirectMixer.Destroy();
                indirectSimulator.Destroy();
            }

            mutex.ReleaseMutex();
        }

        //
        // Courutine to update listener position and orientation at frame end.
        // Done this way to ensure correct update in VR setup.
        //
        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                if (!initialized && environmentalRenderer != null && environmentalRenderer.GetEnvironmentalRenderer() != IntPtr.Zero 
                    && environment != null && environment.Environment().GetEnvironment() != IntPtr.Zero)
                {
                    indirectMixer.environmentRenderer = environmentalRenderer.GetEnvironmentalRenderer();
                    environmentalRenderer.SetUsedByMixer();

                    indirectSimulator.FrameInit(environmentalRenderer.GetEnvironmentalRenderer(), false, 
                        SourceSimulationType.Realtime, reverbSimulationType, "__reverb__");
                    initialized = true;
                }

                listenerPosition = Common.ConvertVector(transform.position);
                listenerAhead = Common.ConvertVector(transform.forward);
                listenerUp = Common.ConvertVector(transform.up);
                indirectSimulator.FrameUpdate(false, SourceSimulationType.Realtime, reverbSimulationType);

                yield return new WaitForEndOfFrame();
            }
        }

        //
        // Applies the Phonon effect to audio.
        //
        void OnAudioFilterRead(float[] data, int channels)
        {
            mutex.WaitOne();

            if (!initialized || destroying)
            {
                mutex.ReleaseMutex();
                Array.Clear(data, 0, data.Length);
                return;
            }

            if ((data == null) || (acceleratedMixing && !processMixedAudio) || (indirectMixer.environmentRenderer == IntPtr.Zero))
            {
                mutex.ReleaseMutex();
                Array.Clear(data, 0, data.Length);
                return;
            }

            if (acceleratedMixing)
                indirectMixer.AudioFrameUpdate(data, channels, listenerPosition, listenerAhead, listenerUp, 
                    indirectBinauralEnabled);
            else
            {
                float[] wetData = indirectSimulator.AudioFrameUpdate(data, channels, listenerPosition, listenerPosition, 
                    listenerAhead, listenerUp, enableReverb, reverbMixFraction, indirectBinauralEnabled);
                if (wetData != null && wetData.Length != 0)
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = data[i] * dryMixFraction + wetData[i];
            }

            mutex.ReleaseMutex();
        }

        void OnDrawGizmosSelected()
        {
            Color oldColor = Gizmos.color;

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

        public void BeginBake()
        {
            if (useAllProbeBoxes)
                phononBaker.BeginBake(FindObjectsOfType<ProbeBox>() as ProbeBox[], BakingMode.Reverb, "__reverb__");
            else
                phononBaker.BeginBake(probeBoxes, BakingMode.Reverb, "__reverb__");
        }

        public void EndBake()
        {
            phononBaker.EndBake();
        }

        // Public members.
        public bool processMixedAudio;
        public bool acceleratedMixing = false;

        public bool enableReverb = false;
        public ReverbSimulationType reverbSimulationType;
        [Range(.0f, 1.0f)]
        public float dryMixFraction = 1.0f;
        [Range(.0f, 10.0f)]
        public float reverbMixFraction = 1.0f;

        public bool indirectBinauralEnabled = false;

        public bool useAllProbeBoxes = false;
        public ProbeBox[] probeBoxes = null;

        // Private members.
        AudioEngine audioEngine;
        EnvironmentalRendererComponent environmentalRenderer;
        EnvironmentComponent environment;

        IndirectMixer indirectMixer = new IndirectMixer();
        IndirectSimulator indirectSimulator = new IndirectSimulator();
        public PhononBaker phononBaker = new PhononBaker();

        Vector3 listenerPosition;
        Vector3 listenerAhead;
        Vector3 listenerUp;

        Mutex mutex = new Mutex();
        bool initialized = false;
        bool destroying = false;
    }
}
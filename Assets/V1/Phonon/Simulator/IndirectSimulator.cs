using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Phonon
{
    public class IndirectSimulator
    {
        public void Init()
        {
            GlobalContext globalContext = PhononSettings.GetGlobalContext();
            RenderingSettings renderingSettings = PhononSettings.GetRenderingSettings();

            inputFormat = PhononSettings.GetAudioConfiguration();
            outputFormat = PhononSettings.GetAudioConfiguration();

            if (PhononCore.iplCreateBinauralRenderer(globalContext, renderingSettings, null, ref binauralRenderer) != Error.None)
            {
                Debug.Log("Unable to create binaural renderer. Please check the log file for details.");
                return;
            }

            PropagationSettings simulationSettings = EnvironmentComponent.SimulationSettings();
            ambisonicsFormat.channelLayoutType = ChannelLayoutType.Ambisonics;
            ambisonicsFormat.ambisonicsOrder = simulationSettings.ambisonicsOrder;
            ambisonicsFormat.numSpeakers = (ambisonicsFormat.ambisonicsOrder + 1) * (ambisonicsFormat.ambisonicsOrder + 1);
            ambisonicsFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
            ambisonicsFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
            ambisonicsFormat.channelOrder = ChannelOrder.Deinterleaved;

            AudioFormat ambisonicsBinauralFormat = outputFormat;
            ambisonicsBinauralFormat.channelOrder = ChannelOrder.Deinterleaved;

#if !UNITY_ANDROID
            if (PhononCore.iplCreateAmbisonicsPanningEffect(binauralRenderer, ambisonicsFormat, ambisonicsBinauralFormat, ref propagationPanningEffect) != Error.None)
            {
                Debug.Log("Unable to create Ambisonics panning effect. Please check the log file for details.");
                return;
            }

            if (outputFormat.channelLayout == ChannelLayout.Stereo)
            {
                // Create ambisonics based binaural effect for indirect sound if the output format is stereo.
                if (PhononCore.iplCreateAmbisonicsBinauralEffect(binauralRenderer, ambisonicsFormat, ambisonicsBinauralFormat, ref propagationBinauralEffect) != Error.None)
                {
                    Debug.Log("Unable to create propagation binaural effect. Please check the log file for details.");
                    return;
                }
            }
#endif

            wetData = new float[renderingSettings.frameSize * outputFormat.numSpeakers];

            wetAmbisonicsDataMarshal = new IntPtr[ambisonicsFormat.numSpeakers];
            for (int i = 0; i < ambisonicsFormat.numSpeakers; ++i)
                wetAmbisonicsDataMarshal[i] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * renderingSettings.frameSize);

            wetDataMarshal = new IntPtr[outputFormat.numSpeakers];
            for (int i = 0; i < outputFormat.numSpeakers; ++i)
                wetDataMarshal[i] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * renderingSettings.frameSize);

            phononListener = GameObject.FindObjectOfType<PhononListener>();
            phononStaticListener = GameObject.FindObjectOfType<PhononStaticListener>();
        }

        public void Destroy()
        {
#if !UNITY_ANDROID
            PhononCore.iplDestroyConvolutionEffect(ref propagationAmbisonicsEffect);
            propagationAmbisonicsEffect = IntPtr.Zero;

            PhononCore.iplDestroyAmbisonicsBinauralEffect(ref propagationBinauralEffect);
            propagationBinauralEffect = IntPtr.Zero;

            PhononCore.iplDestroyAmbisonicsPanningEffect(ref propagationPanningEffect);
            propagationPanningEffect = IntPtr.Zero;
#endif

            wetData = null;

            if (wetDataMarshal != null)
                for (int i = 0; i < outputFormat.numSpeakers; ++i)
                    Marshal.FreeCoTaskMem(wetDataMarshal[i]);
            wetDataMarshal = null;

            if (wetAmbisonicsDataMarshal != null)
                for (int i = 0; i < ambisonicsFormat.numSpeakers; ++i)
                    Marshal.FreeCoTaskMem(wetAmbisonicsDataMarshal[i]);
            wetAmbisonicsDataMarshal = null;
        }

        public float[] AudioFrameUpdate(float[] data, int channels, Vector3 sourcePosition, Vector3 listenerPosition, Vector3 listenerAhead, Vector3 listenerUp, bool enableReflections, float indirectMixFraction, bool indirectBinauralEnabled)
        {
            AudioBuffer inputBuffer;
            inputBuffer.audioFormat = inputFormat;
            inputBuffer.numSamples = data.Length / channels;
            inputBuffer.deInterleavedBuffer = null;
            inputBuffer.interleavedBuffer = data;

            AudioBuffer outputBuffer;
            outputBuffer.audioFormat = outputFormat;
            outputBuffer.numSamples = data.Length / channels;
            outputBuffer.deInterleavedBuffer = null;
            outputBuffer.interleavedBuffer = data;

            // Input data is sent (where it is copied) for indirect propagation effect processing.
            // This data must be sent before applying any other effect to the input audio.
#if !UNITY_ANDROID
            if (enableReflections && (wetData != null) && (wetDataMarshal != null) && (wetAmbisonicsDataMarshal != null) && (propagationAmbisonicsEffect != IntPtr.Zero))
            {
                for (int i = 0; i < data.Length; ++i)
                    wetData[i] = data[i] * indirectMixFraction;

                AudioBuffer propagationInputBuffer;
                propagationInputBuffer.audioFormat = inputFormat;
                propagationInputBuffer.numSamples = wetData.Length / channels;
                propagationInputBuffer.deInterleavedBuffer = null;
                propagationInputBuffer.interleavedBuffer = wetData;

                PhononCore.iplSetDryAudioForConvolutionEffect(propagationAmbisonicsEffect, sourcePosition, propagationInputBuffer);

                if (fourierMixingEnabled)
                {
                    phononListener.processMixedAudio = true;
                    return null;
                }

                AudioBuffer wetAmbisonicsBuffer;
                wetAmbisonicsBuffer.audioFormat = ambisonicsFormat;
                wetAmbisonicsBuffer.numSamples = data.Length / channels;
                wetAmbisonicsBuffer.deInterleavedBuffer = wetAmbisonicsDataMarshal;
                wetAmbisonicsBuffer.interleavedBuffer = null;
                PhononCore.iplGetWetAudioForConvolutionEffect(propagationAmbisonicsEffect, listenerPosition, listenerAhead, listenerUp, wetAmbisonicsBuffer);

                AudioBuffer wetBufferMarshal;
                wetBufferMarshal.audioFormat = outputFormat;
                wetBufferMarshal.audioFormat.channelOrder = ChannelOrder.Deinterleaved;     // Set format to deinterleave.
                wetBufferMarshal.numSamples = data.Length / channels;
                wetBufferMarshal.deInterleavedBuffer = wetDataMarshal;
                wetBufferMarshal.interleavedBuffer = null;

                if ((outputFormat.channelLayout == ChannelLayout.Stereo) && indirectBinauralEnabled)
                    PhononCore.iplApplyAmbisonicsBinauralEffect(propagationBinauralEffect, wetAmbisonicsBuffer, wetBufferMarshal);
                else
                    PhononCore.iplApplyAmbisonicsPanningEffect(propagationPanningEffect, wetAmbisonicsBuffer, wetBufferMarshal);

                AudioBuffer wetBuffer;
                wetBuffer.audioFormat = outputFormat;
                wetBuffer.numSamples = data.Length / channels;
                wetBuffer.deInterleavedBuffer = null;
                wetBuffer.interleavedBuffer = wetData;
                PhononCore.iplInterleaveAudioBuffer(wetBufferMarshal, wetBuffer);

                return wetData;
            }
#endif

            return null;
        }

        public void FrameInit(IntPtr envRenderer, bool sourceUpdate, SourceSimulationType sourceSimulationType, ReverbSimulationType reverbSimualtionType, string uniqueIdentifier)
        {
            if (propagationAmbisonicsEffect == IntPtr.Zero)
            {
                // Check for Baked Source or Baked Reverb component.
                string effectName = "";

                if (sourceUpdate && sourceSimulationType == SourceSimulationType.BakedStaticSource)
                    effectName = uniqueIdentifier;
                else if (sourceUpdate && sourceSimulationType == SourceSimulationType.BakedStaticListener)
                {
                    if (phononStaticListener == null)
                        Debug.LogError("No Phonon Static Listener component found.");
                    else if (phononStaticListener.currentStaticListenerNode == null)
                        Debug.LogError("Current static listener node is not specified in Phonon Static Listener.");
                    else
                        effectName = phononStaticListener.currentStaticListenerNode.GetUniqueIdentifier();
                }
                else if (!sourceUpdate && reverbSimualtionType == ReverbSimulationType.BakedReverb)
                    effectName = "__reverb__";

#if !UNITY_ANDROID
                SimulationType simulationMode = (sourceSimulationType == SourceSimulationType.Realtime) ? SimulationType.Realtime : SimulationType.Baked;
                if (!sourceUpdate)
                    simulationMode = (reverbSimualtionType == ReverbSimulationType.RealtimeReverb) ? SimulationType.Realtime : SimulationType.Baked;

                if (PhononCore.iplCreateConvolutionEffect(envRenderer, effectName, simulationMode, inputFormat, ambisonicsFormat, ref propagationAmbisonicsEffect) != Error.None)
                {
                    Debug.LogError("Unable to create propagation effect for object");
                }
#endif
            }
        }

        public void FrameUpdate(bool sourceUpdate, SourceSimulationType sourceSimulationType, ReverbSimulationType reverbSimulationType)
        {
            if (sourceUpdate && sourceSimulationType == SourceSimulationType.BakedStaticListener && phononStaticListener != null && phononStaticListener.currentStaticListenerNode != null)
                UpdateEffectName(phononStaticListener.currentStaticListenerNode.GetUniqueIdentifier());

            if (phononListener && phononListener.acceleratedMixing)
                fourierMixingEnabled = true;
            else
                fourierMixingEnabled = false;
        }

        //
        // Helper function to change the name of the BakedSource or BakedStaticListener
        // used by the effect.
        //
        public void UpdateEffectName(string effectName)
        {
            if (propagationAmbisonicsEffect != IntPtr.Zero)
                PhononCore.iplSetConvolutionEffectName(propagationAmbisonicsEffect, effectName);
        }

        AudioFormat inputFormat;
        AudioFormat outputFormat;
        AudioFormat ambisonicsFormat;

        float[] wetData = null;
        IntPtr[] wetDataMarshal = null;
        IntPtr[] wetAmbisonicsDataMarshal = null;
        PhononListener phononListener;
        PhononStaticListener phononStaticListener;
        bool fourierMixingEnabled;

        // Phonon API related variables.
        IntPtr binauralRenderer = IntPtr.Zero;
        IntPtr propagationPanningEffect = IntPtr.Zero;
        IntPtr propagationBinauralEffect = IntPtr.Zero;
        IntPtr propagationAmbisonicsEffect = IntPtr.Zero;
    }
}
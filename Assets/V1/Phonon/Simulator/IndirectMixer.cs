using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Phonon
{
    public class IndirectMixer
    {
        public void Init()
        {
            GlobalContext globalContext = PhononSettings.GetGlobalContext();
            PropagationSettings simulationSettings = EnvironmentComponent.SimulationSettings();
            RenderingSettings renderingSettings = PhononSettings.GetRenderingSettings();

            ambisonicsFormat.channelLayoutType = ChannelLayoutType.Ambisonics;
            ambisonicsFormat.ambisonicsOrder = simulationSettings.ambisonicsOrder;
            ambisonicsFormat.numSpeakers = (ambisonicsFormat.ambisonicsOrder + 1) * (ambisonicsFormat.ambisonicsOrder + 1);
            ambisonicsFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
            ambisonicsFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
            ambisonicsFormat.channelOrder = ChannelOrder.Deinterleaved;

            if (PhononCore.iplCreateBinauralRenderer(globalContext, renderingSettings, null, ref binauralRenderer) != Error.None)
            {
                Debug.Log("Unable to create binaural renderer. Please check the log file for details.");
                return;
            }

            outputFormat = PhononSettings.GetAudioConfiguration();

            AudioFormat ambisonicsBinauralFormat = outputFormat;
            ambisonicsBinauralFormat.channelOrder = ChannelOrder.Deinterleaved;

#if !UNITY_ANDROID
            if (PhononCore.iplCreateAmbisonicsPanningEffect(binauralRenderer, ambisonicsFormat, ambisonicsBinauralFormat, 
                ref propagationPanningEffect) != Error.None)
            {
                Debug.Log("Unable to create Ambisonics panning effect. Please check the log file for details.");
                return;
            }

            if (outputFormat.channelLayout == ChannelLayout.Stereo)
            {
                // Create ambisonics based binaural effect for indirect sound if the output format is stereo.
                if (PhononCore.iplCreateAmbisonicsBinauralEffect(binauralRenderer, ambisonicsFormat, ambisonicsBinauralFormat, 
                    ref propagationBinauralEffect) != Error.None)
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

            environmentalRendererComponent = GameObject.FindObjectOfType<EnvironmentalRendererComponent>();
        }

        public void Destroy()
        {
            if (environmentRenderer != IntPtr.Zero)
                PhononCore.iplDestroyEnvironmentalRenderer(ref environmentRenderer);

#if !UNITY_ANDROID
            PhononCore.iplDestroyAmbisonicsBinauralEffect(ref propagationBinauralEffect);
            PhononCore.iplDestroyAmbisonicsPanningEffect(ref propagationPanningEffect);
#endif

            PhononCore.iplDestroyBinauralEffect(ref binauralRenderer);

            environmentRenderer = IntPtr.Zero;

            propagationBinauralEffect = IntPtr.Zero;
            binauralRenderer = IntPtr.Zero;

            wetData = null;

            for (int i = 0; i < outputFormat.numSpeakers; ++i)
                Marshal.FreeCoTaskMem(wetDataMarshal[i]);
            wetDataMarshal = null;

            for (int i = 0; i < ambisonicsFormat.numSpeakers; ++i)
                Marshal.FreeCoTaskMem(wetAmbisonicsDataMarshal[i]);
            wetAmbisonicsDataMarshal = null;
        }

        public void AudioFrameUpdate(float[] data, int channels, Vector3 listenerPosition, Vector3 listenerAhead, Vector3 listenerUp, 
            bool indirectBinauralEnabled)
        {

#if !UNITY_ANDROID
            AudioBuffer ambisonicsBuffer;
            ambisonicsBuffer.audioFormat = ambisonicsFormat;
            ambisonicsBuffer.numSamples = data.Length / channels;
            ambisonicsBuffer.deInterleavedBuffer = wetAmbisonicsDataMarshal;
            ambisonicsBuffer.interleavedBuffer = null;
            PhononCore.iplGetMixedEnvironmentalAudio(environmentRenderer, listenerPosition, listenerAhead, listenerUp, 
                ambisonicsBuffer);

            AudioBuffer spatializedBuffer;
            spatializedBuffer.audioFormat = outputFormat;
            spatializedBuffer.audioFormat.channelOrder = ChannelOrder.Deinterleaved;     // Set format to deinterleave.
            spatializedBuffer.numSamples = data.Length / channels;
            spatializedBuffer.deInterleavedBuffer = wetDataMarshal;
            spatializedBuffer.interleavedBuffer = null;

            if ((outputFormat.channelLayout == ChannelLayout.Stereo) && indirectBinauralEnabled)
                PhononCore.iplApplyAmbisonicsBinauralEffect(propagationBinauralEffect, ambisonicsBuffer, spatializedBuffer);
            else
                PhononCore.iplApplyAmbisonicsPanningEffect(propagationPanningEffect, ambisonicsBuffer, spatializedBuffer);

            AudioBuffer interleavedBuffer;
            interleavedBuffer.audioFormat = outputFormat;
            interleavedBuffer.numSamples = data.Length / channels;
            interleavedBuffer.deInterleavedBuffer = null;
            interleavedBuffer.interleavedBuffer = wetData;
            PhononCore.iplInterleaveAudioBuffer(spatializedBuffer, interleavedBuffer);
#endif

            for (int i = 0; i < data.Length; ++i)
                data[i] += wetData[i];
        }

        public IntPtr environmentRenderer = IntPtr.Zero;
        public EnvironmentalRendererComponent environmentalRendererComponent;

        AudioFormat ambisonicsFormat;
        AudioFormat outputFormat;

        IntPtr binauralRenderer = IntPtr.Zero;
        IntPtr propagationPanningEffect = IntPtr.Zero;
        IntPtr propagationBinauralEffect = IntPtr.Zero;

        float[] wetData = null;
        IntPtr[] wetDataMarshal = null;
        IntPtr[] wetAmbisonicsDataMarshal = null;
    }
}
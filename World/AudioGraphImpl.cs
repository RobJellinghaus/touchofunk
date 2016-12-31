/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

namespace Holofunk.World
{
    /// <summary>
    /// Verbatim from https://msdn.microsoft.com/en-us/windows/uwp/audio-video-camera/audio-graphs
    /// </summary>
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public class AudioGraphImpl : IHoloAudioGraph
    {
        /// <summary>
        /// The audio graph itself.
        /// </summary>
        AudioGraph _audioGraph;

        /// <summary>
        /// The default audio output device (the only one currently supported).
        /// </summary>
        AudioDeviceOutputNode _deviceOutputNode;

        /// <summary>
        /// The default audio input device (the only one currently supported).
        /// </summary>
        AudioDeviceInputNode _deviceInputNode;

        /// <summary>
        /// The submix node which listens to and copies out incoming audio data.
        /// </summary>
        AudioFrameOutputNode _inputCaptureNode;

        public AudioGraphImpl()
        {
        }

        internal AudioFrameOutputNode InputCaptureNode { get { return _inputCaptureNode; } }

        public async Task InitializeAsync()
        {
            DebugUtil.CheckAppThread();

            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            // settings.DesiredRenderDeviceAudioProcessing = AudioProcessing.Raw;
            settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency;

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            DebugUtil.Assert(result.Status == AudioGraphCreationStatus.Success, "Failed to create audio graph");

            _audioGraph = result.Graph;

            int latencyInSamples = _audioGraph.LatencyInSamples;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = _audioGraph.CreateDeviceOutputNodeAsync().GetResults();

            DebugUtil.Assert(deviceOutputNodeResult.Status == AudioDeviceNodeCreationStatus.Success,
                $"Audio Device Output unavailable because {deviceOutputNodeResult.Status}");

            _deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

            _inputCaptureNode = _audioGraph.CreateFrameOutputNode();                                                            // Create a device input node using the default audio input device

            CreateAudioDeviceInputNodeResult deviceInputNodeResult = await _audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Other);

            DebugUtil.Assert(deviceInputNodeResult.Status == AudioDeviceNodeCreationStatus.Success,
                $"Audio Device Input unavailable because {deviceInputNodeResult.Status}");

            _deviceInputNode = deviceInputNodeResult.DeviceInputNode;

            _deviceInputNode.AddOutgoingConnection(_inputCaptureNode);
            _deviceInputNode.AddOutgoingConnection(_deviceOutputNode);

            /*
            echoEffect = new EchoEffectDefinition(_graph);
            echoEffect.WetDryMix = 0.7f;
            echoEffect.Feedback = 0.5f;
            echoEffect.Delay = 500.0f;
            submixNode.EffectDefinitions.Add(echoEffect);

            // Disable the effect in the beginning. Enable in response to user action (UI toggle switch)
            submixNode.DisableEffectsByDefinition(echoEffect);
            */

            // All nodes can have an OutgoingGain property
            // Setting the gain on the Submix node attenuates the output of the node
            //_submixNode.OutgoingGain = 0.5;
        }
    }
}

using System;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.Render;

namespace Touchofunk
{
    internal class AudioGraphImpl : IAudioGraph
    {
        private AudioGraph _audioGraph;
        private AudioDeviceOutputNode _deviceOutputNode;
        private AudioSubmixNode _submixNode;

        public AudioGraphImpl()
        {
        }

        public async Task InitializeAsync()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
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

            _submixNode = _audioGraph.CreateSubmixNode();
            _submixNode.AddOutgoingConnection(_deviceOutputNode);

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
            _submixNode.OutgoingGain = 0.5;
        }
    }
}

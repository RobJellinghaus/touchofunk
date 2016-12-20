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
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Can't create the graph
                throw new System.Exception("Failed to create audio graph");
            }

            _audioGraph = result.Graph;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = _audioGraph.CreateDeviceOutputNodeAsync().GetResults();

            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                throw new System.Exception(String.Format("Audio Device Output unavailable because {0}", deviceOutputNodeResult.Status.ToString()));
            }

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

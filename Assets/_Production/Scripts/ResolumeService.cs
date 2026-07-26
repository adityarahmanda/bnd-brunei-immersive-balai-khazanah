using LZY.Resolume;
using UnityEngine;
using UnityEngine.Events;

namespace LZY.BND
{
    public class ResolumeService : SceneService
    {
        protected override string GetId() => nameof(ResolumeService);

        [SerializeField] private OSC osc;

        private bool _isEntranceConnected;
        private bool _isInteractiveConnected;
        
        public UnityEvent<bool> onInteractiveConnected;

        protected override void OnActivate()
        {
            osc.inPort = MainSceneCore.settings.oscInPort;
            osc.outPort = MainSceneCore.settings.oscOutPort;
            osc.Open();
            osc.SetAddressHandler(MainSceneCore.settings.entranceClip.GetVideoPath() + "/connected", OnEntranceClip);
            osc.SetAddressHandler(MainSceneCore.settings.interactiveClip.GetVideoPath() + "/connected", OnInteractiveClip);
        }

        private void OnEntranceClip(OscMessage oscm)
        {
            var isConnected = GetIsConnectedStatus(oscm);
            _isEntranceConnected = isConnected;
            if (isConnected != _isEntranceConnected)
            {
                _isEntranceConnected = isConnected;
                if (_isEntranceConnected)
                    ConnectClipColumn(MainSceneCore.settings.entranceClip.column);
            }
        }

        private void OnInteractiveClip(OscMessage oscm)
        {
            var isConnected = GetIsConnectedStatus(oscm);
            if (isConnected != _isInteractiveConnected)
            {
                _isInteractiveConnected = isConnected;
                if (_isInteractiveConnected)
                    ConnectClipColumn(MainSceneCore.settings.interactiveClip.column);
                onInteractiveConnected?.Invoke(isConnected);
            }
        }

        private static bool GetIsConnectedStatus(OscMessage oscm)
        {
            var status = (ConnectedStatus)oscm.GetInt(0);
            Debug.Log("[ResolumeService] OSC Input: " + oscm.address + " " + status);
            return status == ConnectedStatus.Connected || status == ConnectedStatus.PreviewedAndConnected;
        }

        public void ConnectClipColumn(int column)
        {
            var oscMessage = new OscMessage()
            {
                address = $"/composition/columns/{column}/connect"
            };

            Debug.Log("[ResolumeService] Sending OSC Message: " + oscMessage.address);
            osc.Send(oscMessage);
        }
    }
}
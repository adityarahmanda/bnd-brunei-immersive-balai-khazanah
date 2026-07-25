using LZY.Resolume;
using UnityEngine;

namespace LZY.BND
{
    public class ResolumeService : SceneService
    {
        protected override string GetId() => nameof(ResolumeService);

        [SerializeField] private OSC osc;

        protected override void OnActivate()
        {
            osc.inPort = MainSceneCore.settings.oscInPort;
            osc.outPort = MainSceneCore.settings.oscOutPort;
            osc.Open();
            osc.SetAddressHandler(MainSceneCore.settings.entranceClip.GetVideoPath(), OnEntranceClip);
            osc.SetAddressHandler(MainSceneCore.settings.interactiveClip.GetVideoPath(), OnInteractiveClip);
        }

        private void OnEntranceClip(OscMessage oscm)
        {
            var isConnected = GetIsConnectedStatus(oscm);
            if (isConnected)
                ConnectClipColumn(MainSceneCore.settings.entranceClip.column);
        }

        private void OnInteractiveClip(OscMessage oscm)
        {
            var isConnected = GetIsConnectedStatus(oscm);
            if (isConnected)
                ConnectClipColumn(MainSceneCore.settings.interactiveClip.column);
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
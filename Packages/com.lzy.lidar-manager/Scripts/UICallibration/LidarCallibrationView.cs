using TMPro;
using UnityEngine;

namespace LZY.Lidar
{
    public class LidarCallibrationView : MonoBehaviour
    {
        [SerializeField] private LidarDataVisualizer visualizer;
        [SerializeField] private LidarEthernetConnectionUIControls connectionControls;
        [SerializeField] private LidarSettingsUIControls settingsControls;
        [SerializeField] private TextMeshProUGUI loggerText;
        [SerializeField] private int loggerLength = 1000;
        [SerializeField] private TextMeshProUGUI versionText;

        private string _originalVersionText;
        private const string VersionIdentifier = "{VERSION}";

        // Run this after URGDeviceManager instance initialized
        public void Initialize(LidarDeviceController deviceController)
        {
            visualizer.Initialize(deviceController);
            connectionControls.Initialize(deviceController);
            settingsControls.Initialize(deviceController);
            RefreshVersionText();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            visualizer.Deinitialize();
            connectionControls.Deinitialize();
            settingsControls.Deinitialize();
        }

        private void RefreshVersionText()
        {
            if (string.IsNullOrEmpty(_originalVersionText))
                _originalVersionText = versionText.text;
            
            versionText.text = _originalVersionText.Replace(VersionIdentifier, LidarSettings.SETTINGS_VERSION);
        }

        private void HandleLog(string condition, string stacktrace, LogType type)
        {
            var log = condition;
            if (loggerLength >= 0)
                log = log.Substring(0, Mathf.Min(log.Length, loggerLength));
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    log = "<#FF0000>" + log + "</color>";
                    break;
                case LogType.Warning:
                    log = "<#FFFF00>" + log + "</color>";
                    break;
            }
            loggerText.text = log;
        }
    }
}

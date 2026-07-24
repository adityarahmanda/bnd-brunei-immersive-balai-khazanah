using System.Linq;
using LZY.Lidar.UIElements;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LZY.Lidar
{
    public class LidarEthernetConnectionUIControls : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private LabelDropdown connectionTypeDropdown;
        [SerializeField] private LabelInputField customIpAddressField;
        [SerializeField] private LabelInputField customPortNumberField;
        [SerializeField] private TextMeshProUGUI connectButtonText;
        [SerializeField] private Button connectButton;
        
        private LidarDeviceController _deviceController;

        public void Initialize(LidarDeviceController deviceController)
        {
            _deviceController = deviceController;
            RegisterCallbacks();
            LoadSettings();
        }

        public void Deinitialize()
        {
            UnregisterCallbacks();
        }
        
        private void OnDisable()
        {
            Deinitialize();
        }

        private void RegisterCallbacks()
        {
            if (_deviceController != null)
                _deviceController.onConnectionChanged.AddListener(OnConnectionChanged);
            
            if (connectionTypeDropdown != null)
                connectionTypeDropdown.Dropdown.onValueChanged.AddListener(OnConnectionTypeChanged);
            
            if (customIpAddressField != null) 
                customIpAddressField.InputField.onValueChanged.AddListener(OnCustomIPAddressChanged);
            
            if (customPortNumberField != null) 
                customPortNumberField.InputField.onValueChanged.AddListener(OnCustomPortNumberChanged);
            
            if (connectButton != null)
                connectButton.onClick.AddListener(OnConnectButtonClicked);
        }

        private void UnregisterCallbacks()
        {
            if (_deviceController != null)
                _deviceController.onConnectionChanged.RemoveListener(OnConnectionChanged);
            
            if (connectionTypeDropdown != null)
                connectionTypeDropdown.Dropdown.onValueChanged.RemoveListener(OnConnectionTypeChanged);
            
            if (customIpAddressField != null) 
                customIpAddressField.InputField.onValueChanged.RemoveListener(OnCustomIPAddressChanged);
            
            if (customPortNumberField != null) 
                customPortNumberField.InputField.onValueChanged.RemoveListener(OnCustomPortNumberChanged);
            
            if (connectButton != null)
                connectButton.onClick.RemoveListener(OnConnectButtonClicked);
        }

        private void OnConnectionChanged(bool isConnected)
        {
            if (connectButtonText == null) return;
            connectButtonText.text = isConnected ? "Disconnect" : "Connect";
        }

        private void OnConnectionTypeChanged(int newValue)
        {
            if (_deviceController == null) return;
            var newConnectionType = (LidarConnectionType)newValue;
            _deviceController.settings.ConnectionType = newConnectionType;
        }

        private void OnCustomIPAddressChanged(string newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.CustomIPAddress = newValue;
        }

        private void OnCustomPortNumberChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.CustomPortNumber = value;
        }
        
        private void OnConnectButtonClicked()
        {
            if (_deviceController == null) return;
            
            if (_deviceController.IsConnected)
                _deviceController.Disconnect();
            else
                _deviceController.Connect();
        }

        private void LoadSettings()
        {
            if (_deviceController == null) return;
            
            if (connectionTypeDropdown != null) 
            {
                var dropdownOptions = System.Enum.GetNames(typeof(LidarConnectionType)).ToList();
                connectionTypeDropdown.Dropdown.ClearOptions();
                connectionTypeDropdown.Dropdown.AddOptions(dropdownOptions);
                connectionTypeDropdown.Dropdown.value = (int)_deviceController.settings.ConnectionType;
                OnConnectionTypeChanged(connectionTypeDropdown.Dropdown.value);
            }
            
            if (customIpAddressField != null) 
                customIpAddressField.Text = _deviceController.settings.CustomIPAddress;
            
            if (customPortNumberField != null) 
                customPortNumberField.Text = _deviceController.settings.CustomPortNumber.ToString();
            
            OnConnectionChanged(_deviceController.IsConnected);
        }
    }
}

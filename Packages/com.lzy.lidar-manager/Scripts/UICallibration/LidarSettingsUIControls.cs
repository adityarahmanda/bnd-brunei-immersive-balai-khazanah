using System.Linq;
using LZY.Lidar.UIElements;
using UnityEngine;
using UnityEngine.UI;

namespace LZY.Lidar
{
    public class LidarSettingsUIControls : MonoBehaviour
    {
        [Header("Generic")] 
        [SerializeField] private Button saveSettingsButton;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Detection Area")] 
        [SerializeField] private LabelDropdown deviceTypeDropdown;
        [SerializeField] private LabelDropdown deviceAnchorDropdown;
        [SerializeField] private LabelVector2InputField offsetField;
        [SerializeField] private LabelSliderInputField rotationDegreesField;
        [SerializeField] private LabelVector2InputField resolutionField;
        [SerializeField] private LabelVector4InputField marginField;
        [SerializeField] private LabelVector2InputField screenSizeField;
        
        [Header("Object Tracking")] 
        [SerializeField] private LabelInputField cancelBriefTimeField;
        [SerializeField] private LabelInputField distanceThresholdForMergeField;
        [SerializeField] private LabelSliderInputField objectPositionSmoothTimeField;
        [SerializeField] private LabelSliderInputField noiseLimitField;
        [SerializeField] private LabelSliderInputField deltaLimitField;
        [SerializeField] private LabelToggle smoothDistanceCurveToggle;
        [SerializeField] private LabelSliderInputField smoothKernelSizeField;
        [SerializeField] private LabelToggle smoothDistanceByTimeToggle;
        [SerializeField] private LabelSliderInputField timeSmoothBreakingDistanceChangeField;
        [SerializeField] private LabelSliderInputField timeSmoothFactorField;

        private LidarDeviceController _deviceController;
        private ScrollToSelected[] _scrollToSelectedList;

        private void Awake()
        {
            _scrollToSelectedList = GetComponentsInChildren<ScrollToSelected>();
            foreach (var scrollToSelected in _scrollToSelectedList)
                scrollToSelected.scrollRect = scrollRect;
        }

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
            if (saveSettingsButton != null)
                saveSettingsButton.onClick.AddListener(OnSaveSettingsClicked);
            
            if (deviceTypeDropdown != null) 
                deviceTypeDropdown.Dropdown.onValueChanged.AddListener(OnDeviceTypeChanged);
            
            if (deviceAnchorDropdown != null) 
                deviceAnchorDropdown.Dropdown.onValueChanged.AddListener(OnDeviceAnchorChanged);
            
            if (rotationDegreesField != null)
                rotationDegreesField.onValueChanged.AddListener(OnRotationDegreesChanged);

            if (offsetField != null)
            {
                offsetField.XInputField.onValueChanged.AddListener(OnOffsetXChanged);
                offsetField.YInputField.onValueChanged.AddListener(OnOffsetYChanged);
            }

            if (resolutionField != null)
            {
                resolutionField.XInputField.onValueChanged.AddListener(OnResolutionWidthChanged);
                resolutionField.YInputField.onValueChanged.AddListener(OnResolutionHeightChanged);
            }
            
            if (marginField != null)
            {
                marginField.XInputField.onValueChanged.AddListener(OnMarginXChanged);
                marginField.YInputField.onValueChanged.AddListener(OnMarginYChanged);
                marginField.ZInputField.onValueChanged.AddListener(OnMarginZChanged);
                marginField.WInputField.onValueChanged.AddListener(OnMarginWChanged);
            }
            
            if (screenSizeField != null)
            {
                screenSizeField.XInputField.onValueChanged.AddListener(OnScreenSizeWidthChanged);
                screenSizeField.YInputField.onValueChanged.AddListener(OnScreenSizeHeightChanged);
            }
            
            if (cancelBriefTimeField != null)
                cancelBriefTimeField.InputField.onValueChanged.AddListener(OnCancelBriefTimeChanged);
            
            if (smoothDistanceCurveToggle != null) 
                smoothDistanceCurveToggle.Toggle.onValueChanged.AddListener(OnSmoothDistanceCurveToggled);
            
            if (smoothDistanceByTimeToggle != null) 
                smoothDistanceByTimeToggle.Toggle.onValueChanged.AddListener(OnSmoothDistanceByTimeToggled);
            
            if (smoothKernelSizeField != null) 
                smoothKernelSizeField.onValueChanged.AddListener(OnSmoothKernelSizeChanged);
            
            if (timeSmoothFactorField != null) 
                timeSmoothFactorField.onValueChanged.AddListener(OnTimeSmoothFactorChanged);
            
            if (timeSmoothBreakingDistanceChangeField != null) 
                timeSmoothBreakingDistanceChangeField.onValueChanged.AddListener(OnTimeSmoothBreakingDistanceChangeChanged);
            
            if (noiseLimitField != null) 
                noiseLimitField.onValueChanged.AddListener(OnNoiseLimitChanged);
            
            if (deltaLimitField != null) 
                deltaLimitField.onValueChanged.AddListener(OnDeltaLimitChanged);
            
            if (objectPositionSmoothTimeField != null) 
                objectPositionSmoothTimeField.onValueChanged.AddListener(OnObjectPositionSmoothTimeChanged);
            
            if (distanceThresholdForMergeField != null) 
                distanceThresholdForMergeField.InputField.onValueChanged.AddListener(OnDistanceThresholdForMergeChanged);
        }

        private void UnregisterCallbacks()
        {
            if (saveSettingsButton != null)
                saveSettingsButton.onClick.RemoveListener(OnSaveSettingsClicked);
         
            if (deviceTypeDropdown != null) 
                deviceTypeDropdown.Dropdown.onValueChanged.RemoveListener(OnDeviceTypeChanged);
            
            if (deviceAnchorDropdown != null) 
                deviceAnchorDropdown.Dropdown.onValueChanged.RemoveListener(OnDeviceAnchorChanged);
            
            if (rotationDegreesField != null)
                rotationDegreesField.onValueChanged.RemoveListener(OnRotationDegreesChanged);

            if (offsetField != null)
            {
                offsetField.XInputField.onValueChanged.RemoveListener(OnOffsetXChanged);
                offsetField.YInputField.onValueChanged.RemoveListener(OnOffsetYChanged);
            }

            if (resolutionField != null)
            {
                resolutionField.XInputField.onValueChanged.RemoveListener(OnResolutionWidthChanged);
                resolutionField.YInputField.onValueChanged.RemoveListener(OnResolutionHeightChanged);
            }
            
            if (marginField != null)
            {
                marginField.XInputField.onValueChanged.RemoveListener(OnMarginXChanged);
                marginField.YInputField.onValueChanged.RemoveListener(OnMarginYChanged);
                marginField.ZInputField.onValueChanged.RemoveListener(OnMarginZChanged);
                marginField.WInputField.onValueChanged.RemoveListener(OnMarginWChanged);
            }
            
            if (screenSizeField != null)
            {
                screenSizeField.XInputField.onValueChanged.RemoveListener(OnScreenSizeWidthChanged);
                screenSizeField.YInputField.onValueChanged.RemoveListener(OnScreenSizeHeightChanged);
            }
            
            if (cancelBriefTimeField != null)
                cancelBriefTimeField.InputField.onValueChanged.RemoveListener(OnCancelBriefTimeChanged);
            
            if (smoothDistanceCurveToggle != null) 
                smoothDistanceCurveToggle.Toggle.onValueChanged.RemoveListener(OnSmoothDistanceCurveToggled);
            
            if (smoothDistanceByTimeToggle != null) 
                smoothDistanceByTimeToggle.Toggle.onValueChanged.RemoveListener(OnSmoothDistanceByTimeToggled);
            
            if (smoothKernelSizeField != null) 
                smoothKernelSizeField.onValueChanged.RemoveListener(OnSmoothKernelSizeChanged);
            
            if (timeSmoothFactorField != null) 
                timeSmoothFactorField.onValueChanged.RemoveListener(OnTimeSmoothFactorChanged);
            
            if (timeSmoothBreakingDistanceChangeField != null) 
                timeSmoothBreakingDistanceChangeField.onValueChanged.RemoveListener(OnTimeSmoothBreakingDistanceChangeChanged);
            
            if (noiseLimitField != null) 
                noiseLimitField.onValueChanged.RemoveListener(OnNoiseLimitChanged);
            
            if (deltaLimitField != null) 
                deltaLimitField.onValueChanged.RemoveListener(OnDeltaLimitChanged);
            
            if (objectPositionSmoothTimeField != null) 
                objectPositionSmoothTimeField.onValueChanged.RemoveListener(OnObjectPositionSmoothTimeChanged);
            
            if (distanceThresholdForMergeField != null) 
                distanceThresholdForMergeField.InputField.onValueChanged.RemoveListener(OnDistanceThresholdForMergeChanged);
        }

        private void OnSaveSettingsClicked()
        {
            if (LidarDeviceManager.Instance == null) return;
            LidarDeviceManager.Instance.SaveSettings();
        }

        private void OnDeviceTypeChanged(int newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.DeviceType = (LidarDeviceType)newValue;
        }
        
        private void OnDeviceAnchorChanged(int newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.DeviceAnchor = (LidarDeviceAnchor)newValue;
        }

        private void OnRotationDegreesChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.RotationDegrees = newValue;
        }

        private void OnOffsetXChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!float.TryParse(newValue, out var value)) return;
            _deviceController.settings.DeviceOffset.x = value;
        }

        private void OnOffsetYChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!float.TryParse(newValue, out var value)) return;
            _deviceController.settings.DeviceOffset.y = value;
        }

        private void OnResolutionWidthChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.SetResolutionWidth(value);
        }

        private void OnResolutionHeightChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.SetResolutionHeight(value);
        }

        private void OnMarginXChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.Margin.x = value;
        }

        private void OnMarginYChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.Margin.y = value;
        }

        private void OnMarginZChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.Margin.z = value;
        }

        private void OnMarginWChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.Margin.w = value;
        }
        
        private void OnScreenSizeWidthChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.ScreenSize.x = value;
        }

        private void OnScreenSizeHeightChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!int.TryParse(newValue, out var value)) return;
            _deviceController.settings.ScreenSize.y = value;
        }
        
        private void OnCancelBriefTimeChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!float.TryParse(newValue, out var value)) return;
            _deviceController.settings.CancelBriefTime = value;
        }

        private void OnSmoothDistanceCurveToggled(bool isToggled)
        {
            if (_deviceController == null) return;
            _deviceController.settings.SmoothDistanceCurve = isToggled;
        }

        private void OnSmoothDistanceByTimeToggled(bool isToggled)
        {
            if (_deviceController == null) return;
            _deviceController.settings.SmoothDistanceByTime = isToggled;
        }

        private void OnTimeSmoothBreakingDistanceChangeChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.TimeSmoothBreakingDistanceChange = (int)newValue;
        }

        private void OnSmoothKernelSizeChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.SmoothKernelSize = (int)newValue;
        }

        private void OnTimeSmoothFactorChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.TimeSmoothFactor = newValue;
        }

        private void OnNoiseLimitChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.NoiseLimit = (int)newValue;
        }

        private void OnDeltaLimitChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.DeltaLimit = (int)newValue;
        }

        private void OnObjectPositionSmoothTimeChanged(float newValue)
        {
            if (_deviceController == null) return;
            _deviceController.settings.ObjectPositionSmoothTime = newValue;
        }

        private void OnDistanceThresholdForMergeChanged(string newValue)
        {
            if (_deviceController == null) return;
            if (!float.TryParse(newValue, out var value)) return;
            _deviceController.settings.DistanceThresholdForMerge = value;
        }
            
        private void LoadSettings()
        {
            if (deviceTypeDropdown != null) 
            {
                var dropdownOptions = System.Enum.GetNames(typeof(LidarDeviceType)).ToList();
                deviceTypeDropdown.Dropdown.ClearOptions();
                deviceTypeDropdown.Dropdown.AddOptions(dropdownOptions);
                deviceTypeDropdown.Dropdown.value = (int)_deviceController.settings.DeviceType;
                OnDeviceTypeChanged(deviceTypeDropdown.Dropdown.value);
            }

            if (deviceAnchorDropdown != null) 
            {
                var dropdownOptions = System.Enum.GetNames(typeof(LidarDeviceAnchor)).ToList();
                deviceAnchorDropdown.Dropdown.ClearOptions();
                deviceAnchorDropdown.Dropdown.AddOptions(dropdownOptions);
                deviceAnchorDropdown.Dropdown.value = (int)_deviceController.settings.DeviceAnchor;
                OnDeviceAnchorChanged(deviceAnchorDropdown.Dropdown.value);
            }
            
            if (rotationDegreesField != null)
                rotationDegreesField.value = _deviceController.settings.RotationDegrees;
            
            if (offsetField != null) 
                offsetField.Value = _deviceController.settings.DeviceOffset;
            
            if (resolutionField != null) 
                resolutionField.Value = _deviceController.settings.Resolution;
            
            if (marginField != null) 
                marginField.Value = _deviceController.settings.Margin;
            
            if (screenSizeField != null) 
                screenSizeField.Value = _deviceController.settings.ScreenSize;
            
            if (cancelBriefTimeField != null)
                cancelBriefTimeField.Text = _deviceController.settings.CancelBriefTime.ToString();

            if (smoothDistanceCurveToggle != null)
            {
                smoothDistanceCurveToggle.SetIsOnWithoutNotify(_deviceController.settings.SmoothDistanceCurve);
                OnSmoothDistanceCurveToggled(smoothDistanceCurveToggle.IsOn);
            }
            
            if (smoothKernelSizeField != null) 
                smoothKernelSizeField.value = _deviceController.settings.SmoothKernelSize;

            if (smoothDistanceByTimeToggle != null)
            {
                smoothDistanceByTimeToggle.SetIsOnWithoutNotify(_deviceController.settings.SmoothDistanceByTime);
                OnSmoothDistanceByTimeToggled(smoothDistanceByTimeToggle.IsOn);
            }
            
            if (timeSmoothFactorField != null) 
                timeSmoothFactorField.value = _deviceController.settings.TimeSmoothFactor;
            
            if (timeSmoothBreakingDistanceChangeField != null) 
                timeSmoothBreakingDistanceChangeField.value = _deviceController.settings.TimeSmoothBreakingDistanceChange;
            
            if (distanceThresholdForMergeField != null) 
                distanceThresholdForMergeField.Text = _deviceController.settings.DistanceThresholdForMerge.ToString();
            
            if (objectPositionSmoothTimeField != null) 
                objectPositionSmoothTimeField.value = _deviceController.settings.ObjectPositionSmoothTime;
            
            if (noiseLimitField != null) 
                noiseLimitField.value = _deviceController.settings.NoiseLimit;
            
            if (deltaLimitField != null) 
                deltaLimitField.value = _deviceController.settings.DeltaLimit;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LZY.Lidar.UIElements
{
    public class LabelSliderInputField : MonoBehaviour
    {
        public float value
        {
            get => _value;
            set
            {
                if (Application.isPlaying)
                {
                    if (slider != null)
                        slider.SetValueWithoutNotify(value);
                    if (inputField != null)
                        inputField.SetTextWithoutNotify(value.ToString());
                }
                _value = value;
            }
        }

        [SerializeField] private float _value;

        [Header("UI Dependencies")]
        public TMP_InputField inputField;
        public Slider slider;
        
        public UnityEvent<float> onValueChanged;

        private void OnEnable()
        {
            if (inputField != null)
                inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            if (slider != null)
                slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDisable()
        {
            if (inputField != null)
                inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
        
        private void OnInputFieldValueChanged(string stringValue)
        {
            if (!float.TryParse(stringValue, out float floatValue)) return;
            if (slider != null)
                slider.SetValueWithoutNotify(floatValue);
            _value = floatValue;
            onValueChanged?.Invoke(floatValue);
        }

        private void OnSliderValueChanged(float newValue)
        {
            if (inputField != null)
            {
                inputField.text = (inputField.contentType == TMP_InputField.ContentType.IntegerNumber)
                    ? Mathf.RoundToInt(newValue).ToString()
                    : newValue.ToString();
                _value = newValue;
                onValueChanged?.Invoke(newValue);
            }
            else
            {
                _value = newValue;
                onValueChanged?.Invoke(newValue);
            }
        }
    }
}

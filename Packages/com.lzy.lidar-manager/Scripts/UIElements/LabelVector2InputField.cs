using System;
using TMPro;
using UnityEngine;

namespace LZY.Lidar.UIElements
{
    public class LabelVector2InputField : MonoBehaviour
    {
        public Type Type => typeof(string);

        public TextMeshProUGUI Label => label;
        public TextMeshProUGUI XLabel => xLabel;
        public TMP_InputField XInputField => xInputField;
        public TextMeshProUGUI YLabel => yLabel;
        public TMP_InputField YInputField => yInputField;
        
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI xLabel;
        [SerializeField] private TMP_InputField xInputField;
        [SerializeField] private TextMeshProUGUI yLabel;
        [SerializeField] private TMP_InputField yInputField;

        public void SetValueWithoutNotify(Vector2 value)
        {
            xInputField.SetTextWithoutNotify(value.x.ToString("N0"));
            yInputField.SetTextWithoutNotify(value.y.ToString("N0"));
        }

        public Vector2 Value
        {
            get
            {
                float.TryParse(xInputField.text, out var xValue);
                float.TryParse(yInputField.text, out var yValue);
                return new(xValue, yValue);
            } 
            set
            {
                xInputField.text = value.x.ToString("N0");
                yInputField.text = value.y.ToString("N0");
            }
        }
    }
}

using System;
using TMPro;
using UnityEngine;

namespace LZY.Lidar.UIElements
{
    public class LabelVector4InputField : MonoBehaviour
    {
        public Type Type => typeof(string);

        public TextMeshProUGUI Label => label;
        public TextMeshProUGUI XLabel => xLabel;
        public TMP_InputField XInputField => xInputField;
        public TextMeshProUGUI YLabel => yLabel;
        public TMP_InputField YInputField => yInputField;
        public TextMeshProUGUI ZLabel => zLabel;
        public TMP_InputField ZInputField => zInputField;
        public TextMeshProUGUI WLabel => wLabel;
        public TMP_InputField WInputField => wInputField;
        
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI xLabel;
        [SerializeField] private TMP_InputField xInputField;
        [SerializeField] private TextMeshProUGUI yLabel;
        [SerializeField] private TMP_InputField yInputField;
        [SerializeField] private TextMeshProUGUI zLabel;
        [SerializeField] private TMP_InputField zInputField;
        [SerializeField] private TextMeshProUGUI wLabel;
        [SerializeField] private TMP_InputField wInputField;

        public void SetValueWithoutNotify(Vector4 value)
        {
            xInputField.SetTextWithoutNotify(value.x.ToString("N0"));
            yInputField.SetTextWithoutNotify(value.y.ToString("N0"));
            xInputField.SetTextWithoutNotify(value.z.ToString("N0"));
            yInputField.SetTextWithoutNotify(value.w.ToString("N0"));
        }

        public Vector4 Value
        {
            get
            {
                float.TryParse(xInputField.text, out var xValue);
                float.TryParse(yInputField.text, out var yValue);
                float.TryParse(zInputField.text, out var zValue);
                float.TryParse(wInputField.text, out var wValue);
                return new(xValue, yValue, zValue, wValue);
            } 
            set
            {
                xInputField.text = value.x.ToString("N0");
                yInputField.text = value.y.ToString("N0"); 
                zInputField.text = value.z.ToString("N0");
                wInputField.text = value.w.ToString("N0");
            }
        }
    }
}

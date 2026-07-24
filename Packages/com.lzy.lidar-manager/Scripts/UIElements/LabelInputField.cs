using TMPro;
using UnityEngine;

namespace LZY.Lidar.UIElements
{
    public class LabelInputField : MonoBehaviour
    {
        public TMP_InputField InputField => inputField;
        public TextMeshProUGUI Label => label;
        
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI label;

        public string Text
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        public void SetTextWithoutNotify(string text)
        {
            inputField.SetTextWithoutNotify(text);
        }
    }
}

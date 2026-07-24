using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LZY.Lidar.UIElements
{
    public class LabelToggle : MonoBehaviour
    {
        public Toggle Toggle => toggle;
        public TextMeshProUGUI Label => label;
        
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI label;
        
        public bool IsOn
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }
    }
}

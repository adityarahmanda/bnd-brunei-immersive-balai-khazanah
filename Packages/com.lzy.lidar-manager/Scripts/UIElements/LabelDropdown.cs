using TMPro;
using UnityEngine;

namespace LZY.Lidar.UIElements
{
    public class LabelDropdown : MonoBehaviour
    {
        public TMP_Dropdown Dropdown => dropdown;
        public TextMeshProUGUI Label => label;

        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TextMeshProUGUI label;
        
        public int Value
        {
            get => dropdown.value;
            set => dropdown.value = value;
        }

        public void SetTextWithoutNotify(int input)
        {
            dropdown.SetValueWithoutNotify(input);
        }
    }
}

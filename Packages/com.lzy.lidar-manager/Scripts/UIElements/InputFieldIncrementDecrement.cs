using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LZY.Lidar.UIElements
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldIncrementDecrement : MonoBehaviour
    {
        [SerializeField] private float incrementValue = 1f;
        [SerializeField] private int currentIntNumber;
        [SerializeField] private float currentFloatNumber;

        private TMP_InputField _inputField;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
        }

        private void Update()
        {
            if (_inputField.contentType != TMP_InputField.ContentType.IntegerNumber && _inputField.contentType != TMP_InputField.ContentType.DecimalNumber) return;
            if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) return;
            if (EventSystem.current == null) return;
            if (EventSystem.current.currentSelectedGameObject != gameObject) return;

            if (_inputField.contentType == TMP_InputField.ContentType.IntegerNumber)
            {
                var roundedValue = Mathf.RoundToInt(incrementValue);
                ChangeIntValue(Input.GetKey(KeyCode.LeftArrow) ? -1 * roundedValue : roundedValue);
            }
            else
            {
                ChangeFloatValue(Input.GetKey(KeyCode.LeftArrow) ? -1 * incrementValue : incrementValue);
            }
        }

        private void ChangeIntValue(int amount)
        {
            if (int.TryParse(_inputField.text, out int result))
                currentIntNumber = result + amount;
            else
                currentIntNumber += amount;
            _inputField.text = currentIntNumber.ToString();
        }
        
        private void ChangeFloatValue(float amount)
        {
            if (float.TryParse(_inputField.text, out float result))
                currentFloatNumber = result + amount;
            else
                currentFloatNumber += amount;
            _inputField.text = currentFloatNumber.ToString();
        }
    }
}

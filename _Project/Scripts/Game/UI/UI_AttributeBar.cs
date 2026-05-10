using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheUnique.Core.Attributes;
using CharacterAttribute = TheUnique.Core.Attributes.Attribute;

namespace TheUnique.UI
{
    public class UI_AttributeBar : MonoBehaviour
    {
        [SerializeField] private AttributeType _type;
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _valueText;

        private CharacterAttribute _targetAttribute;
        private int _lastDisplayedValue = -1;

        public void Initialize(AttributeSystem stats)
        {
            _targetAttribute = _type switch
            {
                AttributeType.Health => stats.Health,
                AttributeType.Hunger => stats.Hunger,
                AttributeType.Stamina => stats.Stamina,
                _ => null
            };

            if (_targetAttribute != null)
            {
                _targetAttribute.OnValueChanged += UpdateUI;
                UpdateUI(_targetAttribute);
            }
        }

        private void UpdateUI(CharacterAttribute attr)
        {
            float ratio = attr.CurrentValue / attr.Value;
            _fillImage.fillAmount = ratio;
            
            if (_valueText != null)
            {
                int currentIntVal = Mathf.CeilToInt(attr.CurrentValue);
                
                if (currentIntVal != _lastDisplayedValue)
                {
                    _valueText.text = $"{currentIntVal}/{attr.Value}";
                    _lastDisplayedValue = currentIntVal;
                }
            }
        }

        private void OnDestroy()
        {
            if (_targetAttribute != null) _targetAttribute.OnValueChanged -= UpdateUI;
        }
    }
}
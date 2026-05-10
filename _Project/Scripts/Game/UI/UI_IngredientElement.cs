using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheUnique.Core.Items;

namespace TheUnique.UI
{
    public class UI_IngredientElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amountText;

        public void Setup(Ingredient ingredient)
        {
            _icon.sprite = ingredient.item.icon;
            _amountText.text = ingredient.amount.ToString();
        }
    }
}
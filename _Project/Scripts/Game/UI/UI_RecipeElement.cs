using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TheUnique.Core.Items;
using TheUnique.Core.Crafting;

namespace TheUnique.UI
{
    public class UI_RecipeElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] private Image _resultIcon;

        private RecipeSO _recipe;
        private CraftingManager _manager;
        private UI_CraftingWindow _window;

        public void Setup(RecipeSO recipe, CraftingManager manager, UI_CraftingWindow window)
        {
            _recipe = recipe;
            _manager = manager;
            _window = window;
            
            _resultIcon.sprite = recipe.resultItem.icon;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _manager.TryCraft(_recipe);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _window.ShowIngredients(_recipe);
        }
    }
}
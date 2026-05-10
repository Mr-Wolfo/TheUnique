using UnityEngine;
using System.Collections.Generic;
using TheUnique.Core.Crafting;
using TheUnique.Core.Items;
using TMPro;

namespace TheUnique.UI
{
    public class UI_CraftingWindow : MonoBehaviour
    {
        [Header("Список крафта")]
        [SerializeField] private GameObject _recipePrefab;
        [SerializeField] private Transform _scrollContainer;

        [Header("Отображение ингредиентов")]
        [SerializeField] private GameObject _ingredientsPanel;
        [SerializeField] private GameObject _ingredientPrefab;
        [SerializeField] private Transform _ingredientsContainer;
        [SerializeField] private TextMeshProUGUI _recipeNameText;

        private CraftingManager _craftingManager;

        public void Initialize(CraftingManager manager)
        {
            _craftingManager = manager;
            
            _craftingManager.OnCraftingListChanged += RefreshList;
            
            var player = manager.GetComponent<TheUnique.Core.Player.PlayerEntity>();
            player.Inventory.OnInventoryUpdated += RefreshList;

            _ingredientsPanel.SetActive(false);
            RefreshList();
        }

        public void RefreshList()
        {
            foreach (Transform child in _scrollContainer) Destroy(child.gameObject);

            var availableRecipes = _craftingManager.GetAvailableRecipes();

            foreach (var recipe in availableRecipes)
            {
                var itemObj = Instantiate(_recipePrefab, _scrollContainer);
                itemObj.GetComponent<UI_RecipeElement>().Setup(recipe, _craftingManager, this);
            }

            if (availableRecipes.Count == 0)
            {
                _ingredientsPanel.SetActive(false);
            }
        }

        public void ShowIngredients(RecipeSO recipe)
        {
            _ingredientsPanel.SetActive(true);
            
            if (_recipeNameText != null)
                _recipeNameText.text = recipe.resultItem.displayName;

            foreach (Transform child in _ingredientsContainer) Destroy(child.gameObject);

            foreach (var ingredient in recipe.ingredients)
            {
                var ingObj = Instantiate(_ingredientPrefab, _ingredientsContainer);
                ingObj.GetComponent<UI_IngredientElement>().Setup(ingredient);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using TheUnique.Core.Player;

namespace TheUnique.UI
{
    public class UI_Manager : MonoBehaviour
    {
        [Header("Windows")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _craftingPanel;

        [Header("Displays")]
        [SerializeField] private UI_AttributeBar[] _attributeBars;
        [SerializeField] private UI_InventoryDisplay _inventoryDisplay;
        [SerializeField] private UI_Hotbar _hotbar;
        [SerializeField] private UI_CraftingWindow _craftingWindow;

        private PlayerInput _playerInput;

        private void Start()
        {
            PlayerEntity player = FindFirstObjectByType<PlayerEntity>();
            if (player == null) return;

            _playerInput = player.GetComponent<PlayerInput>();

            foreach (var bar in _attributeBars) bar.Initialize(player.Stats);
            
            _inventoryDisplay.Initialize(player.Inventory);
            _hotbar.Initialize(player.Inventory, _inventoryDisplay);
            _craftingWindow.Initialize(player.Crafting);

            _inventoryPanel.SetActive(false);
            _craftingPanel.SetActive(false);
            
            UpdateCursorState();
        }

        private void Update()
        {
            if (_playerInput.actions["Inventory"].triggered)
                ToggleInventory();
        }

        public void ToggleInventory()
        {
            _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
            UpdateCursorState();
        }

        public void ToggleCrafting()
        {
            bool willBeActive = !_craftingPanel.activeSelf;
            _craftingPanel.SetActive(willBeActive);
            UpdateCursorState();

            if (!willBeActive)
            {
                FindFirstObjectByType<PlayerEntity>()?.Crafting.SetNearbyStation("None");
            }
        }

        public void OpenCraftingFromStation()
        {
            if (!_craftingPanel.activeSelf)
            {
                _craftingPanel.SetActive(true);
                UpdateCursorState();
            }
        }

        private void UpdateCursorState()
        {
            bool isAnyPanelOpen = true;
            
            Cursor.visible = isAnyPanelOpen;
            Cursor.lockState = isAnyPanelOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
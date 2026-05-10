using UnityEngine;
using TheUnique.Core.Interaction;
using TheUnique.Core.Player;
using TheUnique.Core.Crafting;

namespace TheUnique.Core.World
{
    public class CraftingStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _stationTag = "Workbench";
        [SerializeField] private string _stationName = "Верстак";

        public string GetInteractionPrompt() => $"Открыть {_stationName}";
        public float GetInteractionDistance() => 2.0f;

        public void Interact(PlayerEntity player)
        {
            var crafting = player.GetComponent<CraftingManager>();
            if (crafting != null)
            {
                crafting.SetNearbyStation(_stationTag, transform);
                
                var uiManager = FindFirstObjectByType<TheUnique.UI.UI_Manager>();
                if (uiManager != null)
                {
                    uiManager.OpenCraftingFromStation();
                }
                
                Debug.Log($"Игрок подошел к станции: {_stationName}");
            }
        }
    }
}
using UnityEngine;
using TheUnique.Core.Player;

namespace TheUnique.Core.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "The Unique/Items/Consumable")]
    public class ConsumableItemSO : ItemSO
    {
        [Header("Эффекты восстановления")]
        public float healthRestore = 0f;
        public float hungerRestore = 0f;
        public float staminaRestore = 0f;

        public override void Use(PlayerEntity player)
        {
            if (healthRestore != 0) player.Stats.Health.ApplyChange(healthRestore);
            if (hungerRestore != 0) player.Stats.Hunger.ApplyChange(hungerRestore);
            if (staminaRestore != 0) player.Stats.Stamina.ApplyChange(staminaRestore);

            Debug.Log($"{player.name} использовал {displayName}");
            
        }
    }
}
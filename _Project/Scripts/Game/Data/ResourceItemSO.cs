using UnityEngine;
using TheUnique.Core.Player;

namespace TheUnique.Core.Items
{
    [CreateAssetMenu(fileName = "New Resource", menuName = "The Unique/Items/Resource")]
    public class ResourceItemSO : ItemSO
    {
        private void Awake() => category = ItemCategory.Resource;

        public override void Use(PlayerEntity player) 
        {
            // Ресурсы нельзя "использовать"
            Debug.Log("Это ресурс, его можно только крафтить.");
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TheUnique.Core.Items
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "The Unique/Core/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemSO> _allItems;
        [SerializeField] private List<RecipeSO> _allRecipes;

        private Dictionary<string, ItemSO> _itemCache;

        public IReadOnlyList<RecipeSO> AllRecipes => _allRecipes;

        public void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            if (_allItems == null) return;
            _itemCache = _allItems.ToDictionary(x => x.id, x => x);
        }

        public ItemSO GetItemById(string id)
        {
            if (_itemCache == null) BuildCache();
            return _itemCache.GetValueOrDefault(id);
        }

#if UNITY_EDITOR
        [ContextMenu("Обновить базу (Собрать всё в проекте)")]
        public void RefreshDatabaseEditor()
        {
            _allItems = new List<ItemSO>();
            _allRecipes = new List<RecipeSO>();
            
            string[] itemGuids = AssetDatabase.FindAssets("t:ItemSO");
            foreach (var guid in itemGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _allItems.Add(AssetDatabase.LoadAssetAtPath<ItemSO>(path));
            }
            
            string[] recipeGuids = AssetDatabase.FindAssets("t:RecipeSO");
            foreach (var guid in recipeGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _allRecipes.Add(AssetDatabase.LoadAssetAtPath<RecipeSO>(path));
            }

            EditorUtility.SetDirty(this);
            Debug.Log($"[Database] База обновлена: {_allItems.Count} предметов, {_allRecipes.Count} рецептов.");
        }
#endif
    }
}
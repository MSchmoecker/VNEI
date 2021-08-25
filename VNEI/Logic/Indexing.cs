using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace VNEI.Logic {
    public static class Indexing {
        public static Dictionary<int, Item> Items { get; private set; } = new Dictionary<int, Item>();
        public static event Action IndexFinished;

        public static void IndexAll() {
            PrefabManager.OnPrefabsRegistered -= IndexAll;

            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Piece piece)) {
                    Item item = new Item() {
                        internalName = prefab.name,
                        localizedName = Localization.instance.Localize(piece.m_name),
                        description = piece.m_description,
                        icons = new[] { piece.m_icon },
                        gameObject = prefab
                    };

                    Items.Add(item.internalName.GetStableHashCode(), item);
                }

                if (prefab.TryGetComponent(out ItemDrop itemDrop)) {
                    Item item = new Item() {
                        internalName = prefab.name,
                        localizedName = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name),
                        description = itemDrop.m_itemData.m_shared.m_description,
                        icons = itemDrop.m_itemData.m_shared.m_icons,
                        gameObject = prefab
                    };

                    Items.Add(item.internalName.GetStableHashCode(), item);
                }
            }

            Log.LogInfo("Recipes: " + ObjectDB.instance.m_recipes.Count);
            foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
                if (recipe.m_item && Items.ContainsKey(recipe.m_item.name.GetStableHashCode())) {
                    Items[recipe.m_item.name.GetStableHashCode()].result.Add(recipe);
                }

                foreach (Piece.Requirement resource in recipe.m_resources) {
                    if (resource.m_resItem && Items.ContainsKey(resource.m_resItem.name.GetStableHashCode())) {
                        Items[resource.m_resItem.name.GetStableHashCode()].ingredient.Add(recipe);
                    }
                }
            }

            IndexFinished?.Invoke();
        }
    }
}

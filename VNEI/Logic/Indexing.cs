using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace VNEI.Logic {
    public static class Indexing {
        public static List<Item> Items { get; private set; } = new List<Item>();
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

                    Items.Add(item);
                }

                if (prefab.TryGetComponent(out ItemDrop itemDrop)) {
                    Item item = new Item() {
                        internalName = prefab.name,
                        localizedName = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name),
                        description = itemDrop.m_itemData.m_shared.m_description,
                        icons = itemDrop.m_itemData.m_shared.m_icons,
                        gameObject = prefab
                    };

                    Items.Add(item);
                }
            }

            IndexFinished?.Invoke();
        }
    }
}

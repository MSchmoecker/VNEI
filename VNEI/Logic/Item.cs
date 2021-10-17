using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using VNEI.UI;

namespace VNEI.Logic {
    public class Item {
        public readonly string internalName;
        public readonly string localizedName;
        public readonly string description;
        public readonly GameObject gameObject;
        public readonly bool isOnBlacklist;
        public readonly ItemType itemType;
        public readonly BepInPlugin mod;
        public bool isActive = true;

        public readonly List<RecipeInfo> result = new List<RecipeInfo>();
        public readonly List<RecipeInfo> ingredient = new List<RecipeInfo>();

        private Sprite icon;

        public Item(string name, string localizeName, string description, Sprite icon, ItemType itemType, GameObject prefab) {
            internalName = name;
            localizedName = Localization.instance.Localize(localizeName);
            this.description = description;
            gameObject = prefab;
            isOnBlacklist = Plugin.ItemBlacklist.Contains(name) || Plugin.ItemBlacklist.Contains(Indexing.CleanupName(name));
            this.itemType = itemType;

            if (prefab != null) {
                mod = Indexing.GetModByPrefabName(prefab.name);
            }

            if (icon != null) {
                SetIcon(icon);
            } else {
                Indexing.ToRenderSprite.Enqueue(name);
            }
        }

        public string GetName() {
            return localizedName;
        }

        public string GetNameContext() {
            string modName = mod != null ? mod.Name : string.Empty;
            return $"{internalName}{Environment.NewLine}{modName}";
        }

        public string GetDescription() {
            return description;
        }

        public string GetTooltip(int quality) {
            if ((bool)gameObject && gameObject.TryGetComponent(out ItemDrop itemDrop)) {
                return ItemDrop.ItemData.GetTooltip(itemDrop.m_itemData, quality, true);
            }

            return description;
        }

        public void SetIcon(Sprite sprite) {
            if (icon == null) {
                icon = sprite;
            } else {
                Log.LogDebug($"cannot set sprite for '{internalName}', icon already exists");
            }
        }

        public Sprite GetIcon() {
            return icon ? icon : Plugin.Instance.noIconSprite;
        }

        public string GetPrimaryName() {
            return localizedName.Length > 0 ? localizedName : internalName;
        }
    }
}

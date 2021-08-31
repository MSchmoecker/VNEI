using System;
using System.Collections.Generic;
using UnityEngine;
using VNEI.UI;

namespace VNEI.Logic {
    public class Item {
        public string internalName = string.Empty;
        public string localizedName = string.Empty;
        public string description = string.Empty;
        public GameObject gameObject;
        public bool isOnBlacklist;

        public List<RecipeInfo> result = new List<RecipeInfo>();
        public List<RecipeInfo> ingredient = new List<RecipeInfo>();

        private Sprite icon;

        public string GetName() {
            return localizedName + Environment.NewLine + $"({internalName})";
        }

        public string GetDescription() {
            return description;
        }

        public string GetTooltip() {
            if ((bool)gameObject && gameObject.TryGetComponent(out ItemDrop itemDrop)) {
                return itemDrop.m_itemData.GetTooltip();
            }

            return description;
        }

        public void SetIcon(Sprite sprite) {
            if (icon == null) {
                icon = sprite;
            } else {
                Log.LogInfo($"cannot set sprite for '{internalName}', icon already exists");
            }
        }

        public Sprite GetIcon() {
            return icon != null ? icon : RecipeUI.Instance.noSprite;
        }
    }
}

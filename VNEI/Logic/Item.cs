using System;
using System.Collections.Generic;
using UnityEngine;

namespace VNEI.Logic {
    public class Item {
        public string internalName = string.Empty;
        public string localizedName = string.Empty;
        public string description = string.Empty;
        public Sprite[] icons = Array.Empty<Sprite>();
        public GameObject gameObject;

        public List<RecipeInfo> result = new List<RecipeInfo>();
        public List<RecipeInfo> ingredient = new List<RecipeInfo>();

        public string GetTooltip() {
            if ((bool)gameObject && gameObject.TryGetComponent(out ItemDrop itemDrop)) {
                return itemDrop.m_itemData.GetTooltip();
            }

            return description;
        }
    }
}

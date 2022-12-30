using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Managers;
using UnityEngine;

namespace VNEI.Logic {
    public class Item {
        public readonly string internalName;
        public readonly string preLocalizedName;
        public readonly string localizedName;
        public readonly string description;
        public readonly GameObject gameObject;
        public readonly bool isOnBlacklist;
        public readonly ItemType itemType;
        public readonly BepInPlugin mod;
        public bool isActive = true;
        public int maxQuality;
        public bool isFavorite;

        private bool isKnown;
        public bool IsKnown => isKnown || !Plugin.showOnlyKnown.Value;
        public bool IsSelfKnown { get; private set; }
        private readonly Dictionary<int, string> tooltipsCache = new Dictionary<int, string>();

        public readonly HashSet<RecipeInfo> result = new HashSet<RecipeInfo>();
        public readonly HashSet<RecipeInfo> ingredient = new HashSet<RecipeInfo>();

        public readonly ComponentEvent onFavoriteChanged = new ComponentEvent();
        public readonly ComponentEvent onKnownChanged = new ComponentEvent();
        public static event Action OnAnyFavoriteChanged;
        private Sprite icon;

        public Item(string name, string localizeName, string description, Sprite icon, ItemType itemType, GameObject prefab, int maxQuality = 1) {
            internalName = name;
            preLocalizedName = localizeName;
            localizedName = Localization.instance.Localize(localizeName);
            this.description = description;
            gameObject = prefab;
            isOnBlacklist = Plugin.IsItemBlacklisted(this);
            this.itemType = itemType;
            this.maxQuality = maxQuality;

            if (prefab) {
                mod = ModNames.GetModByPrefabName(prefab.name);
            }

            if (icon) {
                SetIcon(icon);
            } else if (prefab) {
                RenderManager.RenderRequest renderRequest = new RenderManager.RenderRequest(prefab) {
                    Rotation = RenderManager.IsometricRotation,
                    TargetPlugin = mod,
                    UseCache = true,
                };

                SetIcon(RenderManager.Instance.Render(renderRequest));
            }
        }

        public string GetName() {
            return localizedName;
        }

        public string GetNameContext() {
            return $"{internalName}{Environment.NewLine}{GetModName()}";
        }

        public string GetDescription() {
            return description;
        }

        public string GetTooltip(int quality) {
            if (tooltipsCache.TryGetValue(quality, out string tooltip)) {
                return tooltip;
            }

            tooltip = GenerateTooltip(quality);
            tooltipsCache.Add(quality, tooltip);
            return tooltip;
        }

        private string GenerateTooltip(int quality) {
            if ((bool)gameObject && gameObject.TryGetComponent(out ItemDrop itemDrop)) {
                if (!itemDrop.m_itemData.m_dropPrefab) {
                    itemDrop.m_itemData.m_dropPrefab = gameObject;
                }

                return ItemDrop.ItemData.GetTooltip(itemDrop.m_itemData, quality, true);
            }

            return description;
        }

        public void SetIcon(Sprite sprite) {
            if (!icon) {
                icon = sprite;
            }
        }

        public Sprite GetIcon() {
            return icon ? icon : Plugin.Instance.noIconSprite;
        }

        public string GetPrimaryName() {
            return localizedName.Length > 0 ? localizedName : internalName;
        }

        public string GetModName() {
            return mod != null ? mod.Name : string.Empty;
        }

        public string PrintItem() {
            if (internalName == null) return " -- invalid item -- ";
            string descriptionOneLine = GetDescription().Replace('\n', ' ');
            string mappedItemType = itemType.ToString();
            string text = $"Internal Name: {internalName}, Localized Name: {GetPrimaryName()}, Item Type: {mappedItemType}";

            if (!string.IsNullOrEmpty(descriptionOneLine)) {
                text += $", Description: {descriptionOneLine}";
            }

            if (!string.IsNullOrEmpty(GetModName())) {
                text += $", Source Mod: {GetModName()}";
            }

            return text;
        }

        public string PrintItemCSV(string separator) {
            if (internalName == null) return " -- invalid item -- ";
            string descriptionOneLine = GetDescription().Replace('\n', ' ');
            return $"{internalName}{separator}" +
                   $"{GetPrimaryName()}{separator}" +
                   $"{itemType}{separator}" +
                   $"{descriptionOneLine}{separator}" +
                   $"{GetModName()}";
        }

        public static string PrintCSVHeader(string separator) {
            return $"Internal Name{separator}Localized Name{separator}Item Type{separator}Description{separator}Source Mod";
        }

        public void UpdateFavorite(bool favorite) {
            isFavorite = favorite;
            onFavoriteChanged.Invoke();
            OnAnyFavoriteChanged?.Invoke();
        }

        public void UpdateSelfKnown(Player player) {
            isKnown = false;

            if (!Plugin.showOnlyKnown.Value) {
                IsSelfKnown = true;
                return;
            }

            IsSelfKnown = player.m_knownMaterial.Contains(preLocalizedName) || player.m_knownStations.ContainsKey(preLocalizedName);
        }

        public void UpdateKnown() {
            if (IsSelfKnown) {
                isKnown = true;
                onKnownChanged.Invoke();
                return;
            }

            if (result.Any(recipe => recipe.IsSelfKnown) || ingredient.Any(recipe => recipe.IsSelfKnown)) {
                isKnown = true;
                onKnownChanged.Invoke();
                return;
            }

            isKnown = false;
            onKnownChanged.Invoke();
        }
    }
}

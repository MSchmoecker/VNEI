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
        public bool IsKnown { get; private set; }
        public bool IsSelfKnown { get; private set; }
        private readonly Dictionary<int, string> tooltipsCache = new Dictionary<int, string>();

        public readonly List<RecipeInfo> result = new List<RecipeInfo>();
        public readonly List<RecipeInfo> ingredient = new List<RecipeInfo>();

        public readonly ComponentEvent onFavoriteChanged = new ComponentEvent();
        public readonly ComponentEvent onKnownChanged = new ComponentEvent();
        private Sprite icon;

        public Item(string name, string localizeName, string description, Sprite icon, ItemType itemType, GameObject prefab, int maxQuality = 1) {
            internalName = name;
            preLocalizedName = localizeName;
            localizedName = Localization.instance.Localize(localizeName);
            this.description = description;
            gameObject = prefab;
            isOnBlacklist = Plugin.ItemBlacklist.Contains(name) ||
                            Plugin.ItemBlacklist.Contains(Indexing.CleanupName(name));
            this.itemType = itemType;
            this.maxQuality = maxQuality;

            if (prefab != null) {
                mod = ModNames.GetModByPrefabName(prefab.name);
            }

            if (icon != null) {
                SetIcon(icon);
            } else {
                if (prefab) {
                    RenderManager.RenderRequest renderRequest = new RenderManager.RenderRequest(prefab) {
                        Rotation = RenderManager.IsometricRotation,
                    };

                    if (Chainloader.PluginInfos["com.jotunn.jotunn"].Metadata.Version.CompareTo(new System.Version(2, 4, 10)) >= 0) {
                        RenderWithCache(renderRequest);
                    }

                    SetIcon(RenderManager.Instance.Render(renderRequest));
                }
            }
        }

        private void RenderWithCache(RenderManager.RenderRequest renderRequest) {
            renderRequest.TargetPlugin = mod;
            renderRequest.UseCache = true;
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

        public string GetModName() {
            return mod != null ? mod.Name : string.Empty;
        }

        public string PrintItem() {
            if (internalName == null) return " -- invalid item -- ";
            string descriptionOneLine = GetDescription().Replace('\n', ' ');
            string mappedItemType = itemType.ToString();
            return $"InternalName='{internalName}'; " +
                   $"PrimaryName='{GetPrimaryName()}'; " +
                   $"MappedItemType='{mappedItemType}'; " +
                   $"Description='{descriptionOneLine}'; " +
                   $"SourceMod='{GetModName()}'";
        }

        public string PrintItemCSV() {
            if (internalName == null) return " -- invalid item -- ";
            string descriptionOneLine = GetDescription().Replace('\n', ' ');
            string mappedItemType = itemType.ToString();
            return $"{internalName};" +
                   $"{GetPrimaryName()};" +
                   $"{mappedItemType};" +
                   $"{descriptionOneLine};" +
                   $"{GetModName()}";
        }

        public static string PrintCSVHeader() {
            return $"internalName;PrimaryName;MappedItemType;Description;SourceModName";
        }

        public void UpdateFavorite(bool favorite) {
            isFavorite = favorite;
            onFavoriteChanged.Invoke();
        }

        public void UpdateSelfKnown() {
            IsKnown = false;

            if (Plugin.showUnknown.Value) {
                IsSelfKnown = true;
                return;
            }

            IsSelfKnown = Player.m_localPlayer.m_knownMaterial.Contains(preLocalizedName) || Player.m_localPlayer.m_knownStations.ContainsKey(preLocalizedName);
        }

        public void UpdateKnown() {
            if (IsSelfKnown || Plugin.showUnknown.Value) {
                IsKnown = true;
                onKnownChanged.Invoke();
                return;
            }

            if (result.Any(recipe => recipe.IsSelfKnown) || ingredient.Any(recipe => recipe.IsSelfKnown)) {
                IsKnown = true;
                onKnownChanged.Invoke();
                return;
            }

            IsKnown = false;
            onKnownChanged.Invoke();
        }
    }
}

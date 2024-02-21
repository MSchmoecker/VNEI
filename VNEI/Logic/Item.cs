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
        public readonly string preLocalizeName;
        public string localizedName;
        public readonly string description;
        public readonly GameObject prefab;
        public readonly bool isOnBlacklist;
        public readonly ItemType itemType;
        public readonly BepInPlugin mod;
        public bool isActive = true;
        public int maxQuality;
        public bool isFavorite;

        private bool isKnown;
        public bool IsKnown => isKnown || !Plugin.ShowOnlyKnown;
        public bool IsSelfKnown { get; private set; }
        private readonly Dictionary<int, string> tooltipsCache = new Dictionary<int, string>();

        public readonly HashSet<RecipeInfo> result = new HashSet<RecipeInfo>();
        public readonly HashSet<RecipeInfo> ingredient = new HashSet<RecipeInfo>();

        public readonly ComponentEvent onFavoriteChanged = new ComponentEvent();
        public readonly ComponentEvent onKnownChanged = new ComponentEvent();
        public static event Action OnAnyFavoriteChanged;
        private Sprite icon;

        public Item(string name, string preLocalizeName, string description, Sprite icon, ItemType itemType, GameObject prefab, int maxQuality = 1) {
            this.internalName = name;
            this.preLocalizeName = preLocalizeName ?? "";
            this.description = description;
            this.prefab = prefab;
            this.itemType = itemType;
            this.maxQuality = maxQuality;

            isOnBlacklist = Plugin.IsItemBlacklisted(this);
            Plugin.showModTooltip.SettingChanged += ClearTooltipCache;
            Localization.OnLanguageChange += UpdateLocalizedName;
            UpdateLocalizedName();

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

        ~Item() {
            Plugin.showModTooltip.SettingChanged -= ClearTooltipCache;
            Localization.OnLanguageChange -= UpdateLocalizedName;
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

        private void UpdateLocalizedName() {
            localizedName = Localization.instance.Localize(preLocalizeName);
        }

        private void ClearTooltipCache(object sender, EventArgs e) {
            tooltipsCache.Clear();
        }

        private string GenerateTooltip(int quality) {
            if ((bool)prefab && prefab.TryGetComponent(out ItemDrop itemDrop)) {
                if (!itemDrop.m_itemData.m_dropPrefab) {
                    itemDrop.m_itemData.m_dropPrefab = prefab;
                }

                return ItemDrop.ItemData.GetTooltip(itemDrop.m_itemData, quality, true, Game.m_worldLevel);
            }

            return description.TrimEnd() + GetTooltipModName();
        }

        public string GetTooltipModName() {
            if (!Plugin.showModTooltip.Value) {
                return string.Empty;
            }

            string color = "orange";

            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot")) {
                color = "#ADD8E6FF";
            }

            return $"\n\n<color={color}>{GetModName()}</color>";
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
            return mod != null ? mod.Name : "Valheim";
        }

        public string PrintItem() {
            if (string.IsNullOrEmpty(internalName)) {
                return " -- invalid item -- ";
            }

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
            if (string.IsNullOrEmpty(internalName)) {
                return " -- invalid item -- ";
            }

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

        public void UpdateSelfKnown() {
            isKnown = false;

            if (!Plugin.ShowOnlyKnown) {
                IsSelfKnown = true;
                return;
            }

            IsSelfKnown = Player.m_localPlayer.m_knownMaterial.Contains(preLocalizeName) || Player.m_localPlayer.m_knownStations.ContainsKey(preLocalizeName);
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

        public IEnumerable<Item> GetStations() {
            List<Item> stations = new List<Item>();
            stations.AddRange(ingredient.SelectMany(r => r.GetStationItems()));
            stations.AddRange(result.SelectMany(r => r.GetStationItems()));
            return stations.Distinct().ToList();
        }
    }
}

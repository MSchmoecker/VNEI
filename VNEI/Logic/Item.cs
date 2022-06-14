using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Managers;
using UnityEngine;

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
        public int maxQuality;
        public bool isFavorite;

        public readonly List<RecipeInfo> result = new List<RecipeInfo>();
        public readonly List<RecipeInfo> ingredient = new List<RecipeInfo>();

        private readonly List<Tuple<Component, Action>> onFavoriteChangedListeners = new List<Tuple<Component, Action>>();
        private Sprite icon;

        public Item(string name, string localizeName, string description, Sprite icon, ItemType itemType,
            GameObject prefab, int maxQuality = 1) {
            internalName = name;
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

        public void UpdateFavorite() {
            onFavoriteChangedListeners.RemoveAll(i => i?.Item1 == null);
            foreach (Tuple<Component, Action> listener in onFavoriteChangedListeners) {
                listener.Item2.Invoke();
            }
        }

        public void SubscribeOnFavoriteChanged(Component target, Action updateFavorite) {
            onFavoriteChangedListeners.Add(new Tuple<Component, Action>(target, updateFavorite));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace VNEI.Logic.Compatibility {
    public static class WackysDatabaseCompat {
        public static bool WackyMoleDatabaseLoaded { get; private set; }
        public static Dictionary<Recipe, bool> RequiredUpgradeItemsString = new Dictionary<Recipe, bool>();
        public static Dictionary<Recipe, bool> RequiredCraftItemsString = new Dictionary<Recipe, bool>();

        public static void Init() {
            WackyMoleDatabaseLoaded = Chainloader.PluginInfos.ContainsKey("WackyMole.WackysDatabase");

            if (!WackyMoleDatabaseLoaded) {
                return;
            }

            Type WMRecipeCust = AccessTools.TypeByName("wackydatabase.WMRecipeCust, WackysDatabase");
            FieldInfo RequiredUpgradeItemsStringField = AccessTools.Field(WMRecipeCust, "RequiredUpgradeItemsString");
            RequiredUpgradeItemsString = (Dictionary<Recipe, bool>)RequiredUpgradeItemsStringField.GetValue(null);
            FieldInfo RequiredCraftItemsStringField = AccessTools.Field(WMRecipeCust, "RequiredCraftItemsString");
            RequiredCraftItemsString = (Dictionary<Recipe, bool>)RequiredCraftItemsStringField.GetValue(null);
        }

        public static bool HasRecipeEnableOverride(Recipe recipe) {
            if (!WackyMoleDatabaseLoaded) {
                return false;
            }

            if (RequiredUpgradeItemsString.TryGetValue(recipe, out bool value) && value) {
                return true;
            }

            if (RequiredCraftItemsString.TryGetValue(recipe, out value) && value) {
                return true;
            }

            return false;
        }

        public static bool CanBeCrafted(Recipe recipe) {
            if (!WackyMoleDatabaseLoaded) {
                return true;
            }

            if (RequiredUpgradeItemsString.TryGetValue(recipe, out bool value) && value) {
                // this recipe is required for an upgrade, thus not craftable
                return false;
            }

            return true;
        }

        public static bool CanBeUpgraded(Recipe recipe) {
            if (!WackyMoleDatabaseLoaded) {
                return true;
            }

            if (RequiredCraftItemsString.TryGetValue(recipe, out bool value) && value) {
                // this recipe is required for crafting, thus not upgradable
                return false;
            }

            return true;
        }
    }
}

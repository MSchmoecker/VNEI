using System;
using HarmonyLib;
using VNEI.Logic;

namespace VNEI.Patches {
    [HarmonyPatch]
    public static class TooltipPatches {
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), new Type[] { typeof(ItemDrop.ItemData), typeof(int), typeof(bool) })]
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void AppendModName(ref string __result, ItemDrop.ItemData item) {
            if (!Plugin.showModTooltip.Value) {
                return;
            }

            string prefabName = PrefabName(item);
            Item indexedItem = Indexing.GetItem(prefabName);

            if (indexedItem != null) {
                __result = __result.TrimEnd() + indexedItem.GetTooltipModName();
            }
        }

        private static string PrefabName(ItemDrop.ItemData item) {
            if (item == null) {
                return string.Empty;
            }

            if (item.m_dropPrefab != null) {
                return item.m_dropPrefab.name;
            }

            return item.m_shared.m_name;
        }
    }
}

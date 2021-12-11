using System.Collections;
using HarmonyLib;
using UnityEngine;
using VNEI.UI;

namespace VNEI.Patches {
    [HarmonyPatch]
    public class InventoryGuiPatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake)), HarmonyPostfix]
        public static void AwakePatch(InventoryGui __instance) {
            MainVneiHandler.Instance.GetOrCreateVneiTabButton();
            if(!Plugin.Instance.IsAugaPresent()) {
                __instance.StartCoroutine(UpdateOtherTabs());
            }
        }

        private static IEnumerator UpdateOtherTabs() {
            WaitForSeconds wait = new WaitForSeconds(1);
            while (true) {
                yield return wait;
                MainVneiHandler.Instance.UpdateOtherTabs();
            }
            // ReSharper disable once IteratorNeverReturns
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateCraftingPanel)), HarmonyPostfix]
        public static void UpdateCraftingPanelPatch() {
            // vanilla behaviour has set the vanilla tabs active
            if (MainVneiHandler.Instance.VneiTabActive) {
                MainVneiHandler.Instance.SetVneiTabActive();
            }
        }
    }
}

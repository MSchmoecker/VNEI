using System.Collections;
using HarmonyLib;
using UnityEngine;
using VNEI.UI;

namespace VNEI.Patches {
    [HarmonyPatch]
    public class InventoryGuiPatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake)), HarmonyPostfix, HarmonyPriority(Priority.Low)]
        public static void AwakePatch(InventoryGui __instance) {
            if (!Auga.API.IsLoaded()) {
                MainVneiHandler.Instance.GetTabButton();
                __instance.StartCoroutine(UpdateOtherTabs());
            } else {
                MainVneiHandlerAuga.Instance.CreateBaseUI();
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
        [HarmonyAfter("org.bepinex.plugins.jewelcrafting")]
        public static void UpdateCraftingPanelPatch() {
            if (!Auga.API.IsLoaded()) {
                MainVneiHandler.Instance.UpdateTabPosition();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupCrafting)), HarmonyPostfix]
        [HarmonyAfter("org.bepinex.plugins.jewelcrafting")]
        public static void SetupCraftingPatch() {
            if (!Auga.API.IsLoaded()) {
                // vanilla behaviour has set the vanilla tabs active
                if (MainVneiHandler.Instance.VneiTabActive) {
                    MainVneiHandler.Instance.SetTabActive();
                }
            }
        }
    }
}

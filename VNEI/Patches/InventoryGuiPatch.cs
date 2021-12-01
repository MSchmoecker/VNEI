using HarmonyLib;
using VNEI.UI;

namespace VNEI.Patches {
    [HarmonyPatch]
    public class InventoryGuiPatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateCraftingPanel)), HarmonyPostfix]
        public static void UpdateCraftingPanelPatch() {
            MainVneiHandler.Instance.GetOrCreateVneiTabButton();
            MainVneiHandler.Instance.UpdateInventoryTab();
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTabCraftPressed)), HarmonyPrefix]
        public static void OnTabCraftPressedPatch() {
            MainVneiHandler.Instance.SetVneiTabNotActive();
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTabUpgradePressed)), HarmonyPrefix]
        public static void OnTabUpgradePressedPatch() {
            MainVneiHandler.Instance.SetVneiTabNotActive();
        }
    }
}

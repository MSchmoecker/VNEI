using System;
using HarmonyLib;

namespace VNEI.Patches {
    [HarmonyPatch]
    public class KnownRecipesPatchs {
        public static event Action<Player> OnUpdateKnownRecipes;

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateKnownRecipesList)), HarmonyPostfix]
        public static void UpdateKnownRecipesListPatch(Player __instance) {
            OnUpdateKnownRecipes?.Invoke(__instance);
        }
    }
}

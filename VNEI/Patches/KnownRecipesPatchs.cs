using System;
using HarmonyLib;

namespace VNEI.Patches {
    [HarmonyPatch]
    public class KnownRecipesPatchs {
        public static event Action OnUpdateKnownRecipes;

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateKnownRecipesList)), HarmonyPostfix]
        public static void UpdateKnownRecipesListPatch() {
            OnUpdateKnownRecipes?.Invoke();
        }
    }
}

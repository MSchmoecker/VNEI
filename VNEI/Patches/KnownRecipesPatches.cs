using System;
using HarmonyLib;

namespace VNEI.Patches {
    [HarmonyPatch]
    public class KnownRecipesPatches {
        public static event Action OnUpdateKnownRecipes;

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateKnownRecipesList))]
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        [HarmonyPatch(typeof(Player), nameof(Player.ResetCharacterKnownItems))]
        [HarmonyPostfix]
        public static void UpdateKnownRecipesListPatch() {
            OnUpdateKnownRecipes?.Invoke();
        }
    }
}

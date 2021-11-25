using HarmonyLib;
using VNEI.Logic;

namespace VNEI.Patches {
    [HarmonyPatch]
    public static class DungeonGeneratorPatch {
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load)), HarmonyPostfix]
        public static void Start() {
            Indexing.IndexAll();
        }
    }
}

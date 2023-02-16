using System.Collections.Generic;
using BepInEx.Bootstrap;

namespace VNEI.Logic.Compatibility {
    public class PlanBuild {
        private static List<string> disableToolPieces = new List<string>() {
            "piece_bpcapture",
            "piece_bpselectadd",
            "piece_bpselectremove",
            "piece_bpselectedit",
            "piece_bpsnappoint",
            "piece_bpcenterpoint",
            "piece_bpterrainmod",
            "piece_bpdelete",
            "piece_bpterrain",
            "piece_bpobjects",
            "piece_bppaint",
            "piece_plan_delete",
        };

        public static void Init() {
            if (!Chainloader.PluginInfos.ContainsKey("marcopogo.PlanBuild")) {
                return;
            }

            Indexing.OnDisableItems += (prefab) => {
                if (prefab.GetComponent("PlanBuild.Plans.PlanPiece")) {
                    Indexing.DisableItem(prefab.name, "is PlanBuild PlanPiece");
                }
            };

            Indexing.AfterDisableItems += () => {
                foreach (string piece in disableToolPieces) {
                    Indexing.DisableItem(piece, "is PlanBuild tool piece");
                }
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace VNEI.Patches {
    public class ModCompat {
        private class ValheimRecycleFix {
            [UsedImplicitly]
            private static IEnumerable<MethodBase> TargetMethods() {
                Type valheimRecycle =
                    Type.GetType("ValheimRecycle.ValheimRecycle, ValheimRecycle, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                yield return valheimRecycle.GetMethod("GetOrCreateRecycleTab", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            [UsedImplicitly]
            private static void Postfix(GameObject __result) {
                __result.transform.SetSiblingIndex(2);
            }
        }

        public static void Init(Harmony harmony) {
            if (Chainloader.PluginInfos.ContainsKey("org.lafchi.plugins.valheim_recycle")) {
                harmony.PatchAll(typeof(ValheimRecycleFix));
            }
        }
    }
}

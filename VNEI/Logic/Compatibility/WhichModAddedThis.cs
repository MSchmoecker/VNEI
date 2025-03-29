using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace VNEI.Logic.Compatibility {
    public class WhichModAddedThis {
        public static bool IsLoaded => Chainloader.PluginInfos.ContainsKey("com.maxsch.valheim.WhichModAddedThis");

        private static MethodInfo GetTooltipModNameMethod;

        static WhichModAddedThis() {
            if (IsLoaded) {
                Type type = AccessTools.TypeByName("WhichModAddedThis.Patches, WhichModAddedThis");
                GetTooltipModNameMethod = AccessTools.Method(type, "GetTooltipModName");
            }
        }

        public static string GetTooltipModName(string modName) {
            if (IsLoaded) {
                return (string)GetTooltipModNameMethod.Invoke(null, new object[] { modName });
            }

            return string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Jotunn;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;
using VNEI.UI;
using Paths = BepInEx.Paths;

namespace VNEI {
    public class EditorSetup : MonoBehaviour {
        public BaseUI baseUI;

        public void Awake() {
            Harmony harmony = new Harmony("id");

            harmony.PatchAll(typeof(PathsPatches));
            harmony.PatchAll(typeof(SteamManagerPatches));
            harmony.PatchAll(typeof(LocalizationPatches));

            ZInput.Initialize();
            gameObject.AddComponent<Main>();
            gameObject.AddComponent<Plugin>();
        }

        private void Start() {
            Plugin.attachToCrafting.Value = false;
            Plugin.OpenUI();

            baseUI.SetSize(false, 8, 10);
            baseUI.SetVisibility(true);
            baseUI.dragHandler.GetComponent<Image>().color = new Color(0.3882f, 0.2274f, 0.1372f);
            baseUI.ShowRecipe(new Item("Test", "Test", "Test", null, ItemType.Item, null), true);

            baseUI.recipeUi.craftingStationList.SetStations(new List<Item>() {
                new Item("Test1", "Test1", "Test1", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test2", "Test2", "Test2", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test3", "Test3", "Test3", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test4", "Test4", "Test4", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test5", "Test5", "Test5", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test6", "Test6", "Test6", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test7", "Test7", "Test7", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test8", "Test8", "Test8", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test9", "Test9", "Test9", Plugin.Instance.noIconSprite, ItemType.Item, null),
                new Item("Test10", "Test10", "Test10", Plugin.Instance.noIconSprite, ItemType.Item, null),
            });
        }

        private static class PathsPatches {
            private static string BaseFolder { get; } = CreateTempFolder();

            private static string PluginFolder { get; } = CreateFolder(BaseFolder, "plugins");

            private static string ConfigFolder { get; } = CreateFolder(BaseFolder, "config");

            private static string CreateTempFolder() {
                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDirectory);
                return tempDirectory;
            }

            private static string CreateFolder(string basePath, string path) {
                string fullPath = Path.Combine(basePath, path);
                Directory.CreateDirectory(fullPath);
                return fullPath;
            }

            [HarmonyPatch(typeof(Paths), nameof(Paths.BepInExConfigPath), MethodType.Getter), HarmonyPostfix]
            public static void PatchPluginInfos(ref string __result) => __result = Path.Combine(ConfigFolder, "BepInEx.cfg");

            [HarmonyPatch(typeof(Paths), nameof(Paths.ConfigPath), MethodType.Getter), HarmonyPostfix]
            public static void PatchConfigPath(ref string __result) => __result = ConfigFolder;

            [HarmonyPatch(typeof(Paths), nameof(Paths.PluginPath), MethodType.Getter), HarmonyPostfix]
            public static void PatchPluginPath(ref string __result) => __result = PluginFolder;
        }

        public static class SteamManagerPatches {
            [HarmonyPatch(typeof(SteamManager), "Awake"), HarmonyPrefix]
            public static bool NoSteamManagerAwake() {
                return false;
            }

            [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.InitializeSteam)), HarmonyPrefix]
            public static bool NoFejdStartupInitializeSteam() {
                return false;
            }
        }

        public static class LocalizationPatches {
            [HarmonyPatch(typeof(Localization), "LoadLanguages"), HarmonyPrefix]
            public static bool NoLocalizationLoadLanguages(ref List<string> __result) {
                __result = new List<string>();
                return false;
            }

            [HarmonyPatch(typeof(Localization), nameof(Localization.SetupLanguage)), HarmonyPrefix]
            public static bool NoLocalizationSetupLanguage() {
                return false;
            }
        }
    }
}

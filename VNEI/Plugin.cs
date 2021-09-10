using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using Jotunn.Managers;
using UnityEngine;
using VNEI.Logic;
using VNEI.UI;

namespace VNEI {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "VNEI";
        public const string ModGuid = "com.maxsch.valheim.vnei";
        public const string ModVersion = "0.1.0";
        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }
        public static HashSet<string> ItemBlacklist { get; private set; } = new HashSet<string>();

        public static ConfigEntry<bool> fixPlants;
        public static ConfigEntry<bool> useBlacklist;

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            const string fixPlantsDescription = "This combines plants which are stored as two separate objects to one, " +
                                                "as one is used for seeds and the other for the real plant. " +
                                                "Turn this off if some other mod has problems with this fix and provide a bug report, please";
            fixPlants = Config.Bind("General", "Fix Cultivate Plants", true, new ConfigDescription(fixPlantsDescription));

            const string useBlacklistDescription = "Disables items that are not used currently in the game. This doesn't include " +
                                                   "placeholder items but testing objects for the devs or not obtainable items/effects. " +
                                                   "This list is manual, so please contact me if an item is missing/not on the list";
            useBlacklist = Config.Bind("General", "Use Item Blacklist", true, new ConfigDescription(useBlacklistDescription));
            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            LocalizationManager.Instance.AddJson("English", LoadTextFromResources("Localization.English.json"));

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("VNEI_AssetBundle", Assembly.GetExecutingAssembly());
            ItemBlacklist = SimpleJson.SimpleJson.DeserializeObject<List<string>>(LoadTextFromResources("ItemBlacklist.json")).ToHashSet();

            GUIManager.OnCustomGUIAvailable += BaseUI.Create;
        }

        private void OnDestroy() {
            harmony?.UnpatchAll(ModGuid);
        }

        public static string LoadTextFromResources(string fileName) {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using (Stream stream = execAssembly.GetManifestResourceStream(resourceName)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}

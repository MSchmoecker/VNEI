using System;
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
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "VNEI";
        public const string ModGuid = "com.maxsch.valheim.vnei";
        public const string ModVersion = "0.0.0";
        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }

        public static ConfigEntry<bool> fixPlants;

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            const string fixPlantsDescription = "This combines plants which are stored as two separate objects to one, " +
                                                "as one is used for seeds and the other for the real plant. " +
                                                "Turn this off if some other mod has problems with this fix and provide a bug report, please";
            fixPlants = Config.Bind("General", "fix_cultivate_plants", true, new ConfigDescription(fixPlantsDescription));

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("VNEI_AssetBundle", Assembly.GetExecutingAssembly());

            PrefabManager.OnPrefabsRegistered += Indexing.IndexAll;
            GUIManager.OnCustomGUIAvailable += BaseUI.Create;
        }

        private void OnDestroy() {
            harmony?.UnpatchAll(ModGuid);
        }
    }
}

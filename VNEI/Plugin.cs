using System;
using System.Reflection;
using BepInEx;
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

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

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

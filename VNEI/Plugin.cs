using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Utils;
using Jotunn.Managers;
using UnityEngine;
using VNEI.Logic;
using VNEI.Patches;
using VNEI.UI;

namespace VNEI {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "VNEI";
        public const string ModGuid = "com.maxsch.valheim.vnei";
        public const string ModVersion = "0.5.3";
        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }
        public static HashSet<string> ItemBlacklist { get; private set; } = new HashSet<string>();

        public static ConfigEntry<bool> fixPlants;
        public static ConfigEntry<bool> useBlacklist;
        public static ConfigEntry<bool> invertScroll;
        public static ConfigEntry<bool> attachToCrafting;
        public static ConfigEntry<bool> hideUIAtStartup;
        public static ConfigEntry<int> rowCount;
        public static ConfigEntry<int> columnCount;
        public static ConfigEntry<int> transparency;
        public static ConfigEntry<KeyboardShortcut> openHotkey;

        public static bool isUiOpen = true;
        public static event Action OnOpenHotkey;
        public Sprite noIconSprite;

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);
            AcceptableValueRange<int> rowRange = new AcceptableValueRange<int>(1, 25);

            AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);

            string configText = AssetUtils.LoadTextFromResources("Localization.Config.json", Assembly.GetExecutingAssembly());
            Dictionary<string, string> config = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, string>>(configText);

            // General
            fixPlants = Config.Bind("General", "Fix Cultivate Plants", true, new ConfigDescription(config["FixCultivatePlants"]));
            useBlacklist = Config.Bind("General", "Use Item Blacklist", true, new ConfigDescription(config["UseItemBlacklist"]));
            invertScroll = Config.Bind("General", "Invert Scroll", false, new ConfigDescription(config["InvertScroll"]));

            // Hotkeys
            openHotkey = Config.Bind("Hotkeys", "Open UI Hotkey", new KeyboardShortcut(KeyCode.H, KeyCode.LeftAlt), config["OpenUIHotkey"]);

            // UI
            columnCount = Config.Bind("UI", "Items Horizontal", 12, new ConfigDescription(config["ItemsHorizontal"], rowRange));
            rowCount = Config.Bind("UI", "Items Vertical", 6, new ConfigDescription(config["ItemsVertical"], rowRange));
            attachToCrafting = Config.Bind("UI", "Attach To Crafting", true, new ConfigDescription(config["AttachToCrafting"]));
            hideUIAtStartup = Config.Bind("UI", "Hide GUI At Start", false, new ConfigDescription(config["HideGUIAtStart"]));

            // Visual
            transparency = Config.Bind("Visual", "Background Transparency", 0, new ConfigDescription(config["Transparency"], percentRange));

            isUiOpen = !hideUIAtStartup.Value;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            // load embedded asset bundle
            AssetBundle = AssetUtils.LoadAssetBundleFromResources("VNEI_AssetBundle", Assembly.GetExecutingAssembly());

            CustomLocalization localization = new CustomLocalization();

            // load embedded localisation
            string englishJson = AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly());
            localization.AddJsonFile("English", englishJson);

            LocalizationManager.Instance.AddLocalization(localization);

            // load embedded blacklist
            string blacklistJson = AssetUtils.LoadTextFromResources("ItemBlacklist.json", Assembly.GetExecutingAssembly());
            ItemBlacklist = SimpleJson.SimpleJson.DeserializeObject<List<string>>(blacklistJson).ToHashSet();

            noIconSprite = AssetBundle.LoadAsset<Sprite>("NoSprite.png");
            GUIManager.OnCustomGUIAvailable += () => MainVneiHandler.Instance.GetOrCreateBaseUI();
            CommandManager.Instance.AddConsoleCommand(new SelectUITest.ToggleUIConsoleCommand());
            CommandManager.Instance.AddConsoleCommand(new FileWriterController());
        }

        private void Start() {
            ModCompat.Init(harmony);
        }

        private void Update() {
            if (openHotkey.Value.IsDown()) {
                isUiOpen = !isUiOpen;
                OnOpenHotkey?.Invoke();
            }

            if (!Indexing.HasIndexed() && Player.m_localPlayer) {
                Indexing.IndexAll();
            }
        }

        public bool IsAugaPresent() {
            return Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga");
        }

        public bool AttachToCrafting() {
            return attachToCrafting.Value && !IsAugaPresent();
        }
    }
}

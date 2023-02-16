using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Utils;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using VNEI.Logic;
using VNEI.Logic.Compatibility;
using VNEI.Patches;
using VNEI.UI;

namespace VNEI {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "VNEI";
        public const string ModGuid = "com.maxsch.valheim.vnei";
        public const string ModVersion = "0.8.2";

        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }
        private static HashSet<string> ItemBlacklist { get; set; } = new HashSet<string>();

        public static ConfigEntry<bool> useBlacklist;
        public static ConfigEntry<bool> invertScroll;
        public static ConfigEntry<bool> attachToCrafting;
        public static ConfigEntry<bool> hideUIAtStartup;
        public static ConfigEntry<int> rowCount;
        public static ConfigEntry<int> columnCount;
        public static ConfigEntry<int> transparency;
        public static ConfigEntry<KeyboardShortcut> openHotkey;
        public static ConfigEntry<KeyboardShortcut> viewRecipeHotkey;
        public static ConfigEntry<InputButtonWrapper> itemCheatHotkey;
        public static ConfigEntry<InputButtonWrapper> removeRecentHotkey;
        public static ConfigEntry<KeyboardShortcut> goForwardHotkey;
        public static ConfigEntry<KeyboardShortcut> goBackHotkey;
        public static ConfigEntry<bool> showOnlyKnown;
        public static ConfigEntry<bool> showRecentItems;
        public static ConfigEntry<string> tabName;

        public static bool isUiOpen = true;
        public static event Action OnOpenHotkey;
        public Sprite noIconSprite;
        public Sprite starSprite;
        public Sprite noStarSprite;
        public GameObject vneiUI;
        public GameObject displayItemTemplate;

        private static readonly Regex RichTagRegex = new Regex(@"<[^>]*>");
        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            AcceptableValueRange<int> rowRange = new AcceptableValueRange<int>(1, 25);
            AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);

            string configText = AssetUtils.LoadTextFromResources("Localization.Config.json", Assembly.GetExecutingAssembly());
            Dictionary<string, string> config = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, string>>(configText);

            // General
            useBlacklist = Config.Bind("General", "Use Item Blacklist", true, new ConfigDescription(config["UseItemBlacklist"]));
            invertScroll = Config.Bind("General", "Invert Scroll", false, new ConfigDescription(config["InvertScroll"]));
            showOnlyKnown = Config.Bind("General", "Show Only Known", false, new ConfigDescription(config["ShowOnlyKnown"]));
            showRecentItems = Config.Bind("General", "Show Recent Items", true, new ConfigDescription(config["ShowRecentItems"]));

            // Hotkeys
            openHotkey = Config.Bind("Hotkeys", "Open UI Hotkey", new KeyboardShortcut(KeyCode.H, KeyCode.LeftAlt), config["OpenUIHotkey"]);
            viewRecipeHotkey = Config.Bind("Hotkeys", "View Recipe Hotkey", new KeyboardShortcut(KeyCode.R), config["ViewRecipeHotkey"]);
            itemCheatHotkey = Config.Bind("Hotkeys", "Item Cheat Mouse Button", InputButtonWrapper.Right, config["ItemCheatMouseButton"]);
            removeRecentHotkey = Config.Bind("Hotkeys", "Remove Recent Item Mouse Button", InputButtonWrapper.Middle, config["RemoveRecentMouseButton"]);
            goForwardHotkey = Config.Bind("Hotkeys", "Go Forward Hotkey", new KeyboardShortcut(KeyCode.RightArrow), config["GoForwardHotkey"]);
            goBackHotkey = Config.Bind("Hotkeys", "Go Back Hotkey", new KeyboardShortcut(KeyCode.LeftArrow), config["GoBackHotkey"]);

            // UI
            columnCount = Config.Bind("UI", "Items Horizontal", 12, new ConfigDescription(config["ItemsHorizontal"], rowRange));
            rowCount = Config.Bind("UI", "Items Vertical", 6, new ConfigDescription(config["ItemsVertical"], rowRange));
            attachToCrafting = Config.Bind("UI", "Attach To Crafting", true, new ConfigDescription(config["AttachToCrafting"]));
            hideUIAtStartup = Config.Bind("UI", "Hide GUI At Start", false, new ConfigDescription(config["HideGUIAtStart"]));

            // Visual
            transparency = Config.Bind("Visual", "Background Transparency", 0, new ConfigDescription(config["Transparency"], percentRange));
            tabName = Config.Bind("Visual", "Tab Name", "VNEI", new ConfigDescription(config["TabName"]));

            isUiOpen = !hideUIAtStartup.Value;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            // load embedded asset bundle
            AssetBundle = AssetUtils.LoadAssetBundleFromResources("VNEI_AssetBundle");

            CustomLocalization localization = new CustomLocalization();

            // load embedded localisation
            string englishJson = AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly());
            string germanJson = AssetUtils.LoadTextFromResources("Localization.German.json", Assembly.GetExecutingAssembly());
            localization.AddJsonFile("English", englishJson);
            localization.AddJsonFile("German", germanJson);

            LocalizationManager.Instance.AddLocalization(localization);

            LoadBlacklist();

            noIconSprite = AssetBundle.LoadAsset<Sprite>("NoSprite.png");
            starSprite = AssetBundle.LoadAsset<Sprite>("Star.png");
            noStarSprite = AssetBundle.LoadAsset<Sprite>("NoStar.png");
            vneiUI = AssetBundle.LoadAsset<GameObject>("VNEI");
            displayItemTemplate = AssetBundle.LoadAsset<GameObject>("_Template");

            GUIManager.OnCustomGUIAvailable += () => {
                GetMainUI().CreateBaseUI();
            };

            CommandManager.Instance.AddConsoleCommand(new SelectUITest.ToggleUIConsoleCommand());
            CommandManager.Instance.AddConsoleCommand(new FileWriterControllerCSV());
            CommandManager.Instance.AddConsoleCommand(new FileWriterControllerYAML());
            CommandManager.Instance.AddConsoleCommand(new FileWriterControllerText());

            PrefabManager.OnPrefabsRegistered += ApplyMocks;

            ModQuery.Enable();
        }

        private static void LoadBlacklist() {
            // load embedded blacklist
            string blacklistJson = AssetUtils.LoadTextFromResources("ItemBlacklist.json", Assembly.GetExecutingAssembly());
            ItemBlacklist = SimpleJson.SimpleJson.DeserializeObject<List<string>>(blacklistJson).ToHashSet();

            // load user blacklist
            string externalBlacklist = Path.Combine(BepInEx.Paths.ConfigPath, $"{ModGuid}.blacklist.txt");

            if (!File.Exists(externalBlacklist)) {
                File.WriteAllText(externalBlacklist, "");
                return;
            }

            foreach (string line in File.ReadAllLines(externalBlacklist)) {
                if (string.IsNullOrEmpty(line) || ItemBlacklist.Contains(line)) {
                    continue;
                }

                ItemBlacklist.Add(line);
            }
        }

        private void Start() {
            ModCompat.Init(harmony);
            PlanBuild.Init();
        }

        private void Update() {
            if (openHotkey.Value.IsKeyDown()) {
                isUiOpen = !isUiOpen;
                OnOpenHotkey?.Invoke();
            }

            if (!Indexing.HasIndexed() && Player.m_localPlayer) {
                Indexing.IndexAll();
                Indexing.UpdateKnown(Player.m_localPlayer);
                showOnlyKnown.SettingChanged += (sender, args) => Indexing.UpdateKnown(Player.m_localPlayer);
                KnownRecipesPatchs.OnUpdateKnownRecipes += Indexing.UpdateKnown;
            }

            if (viewRecipeHotkey.Value.IsKeyDown() && UITooltip.m_current) {
                string topic = UITooltip.m_current.m_topic.Trim();
                string text = UITooltip.m_current.m_text.Trim();
                Item item = null;

                if (topic.Length > 0) {
                    item = Indexing.GetItem(topic);
                }

                if (item == null && text.Length > 0) {
                    item = Indexing.GetItem(text);
                }

                // checks if the topic contains xml tags and removes them
                if (item == null && RichTagRegex.IsMatch(topic)) {
                    string cleanedTopic = RichTagRegex.Replace(topic, string.Empty).Trim();
                    item = Indexing.GetItem(cleanedTopic);
                }

                if (item != null) {
                    if (attachToCrafting.Value) {
                        GetMainUI().SetTabActive();
                    }

                    BaseUI baseUI = GetMainUI().GetBaseUI();
                    baseUI.ShowRecipe(item, true);
                }
            }
        }

        public static bool AttachToCrafting() {
            return attachToCrafting.Value;
        }

        private void ApplyMocks() {
            vneiUI.FixReferences(true);
            displayItemTemplate.FixReferences(true);

            PrefabManager.OnPrefabsRegistered -= ApplyMocks;
        }

        public static bool IsItemBlacklisted(Item item) {
            return ItemBlacklist.Contains(item.internalName) || ItemBlacklist.Contains(Indexing.CleanupName(item.internalName));
        }

        public enum InputButtonWrapper {
            Left = PointerEventData.InputButton.Left,
            Right = PointerEventData.InputButton.Right,
            Middle = PointerEventData.InputButton.Middle,
            None = -1
        }

        public static VneiHandler GetMainUI() {
            if (IsAugaLoaded()) {
                return MainVneiHandlerAuga.Instance;
            }

            return MainVneiHandler.Instance;
        }

        public static bool IsAugaLoaded() {
            return Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga");
        }
    }
}

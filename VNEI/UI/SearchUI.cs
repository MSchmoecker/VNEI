using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class SearchUI : MonoBehaviour {
        public Transform spawnRect;
        public InputField searchField;
        public Text pageText;

        public BaseUI baseUI;

        private readonly List<ListItem> listItems = new List<ListItem>();
        private readonly List<DisplayItem> displayItems = new List<DisplayItem>();
        private bool hasInit;
        private readonly Vector2 itemSpacing = new Vector2(50f, 50f);
        private Action typeToggleOnChange;
        private int currentPage;
        private int maxPages;

        public void Awake() {
            hasInit = false;
            typeToggleOnChange = () => UpdateSearch(true);
            TypeToggle.OnChange += typeToggleOnChange;
            baseUI.RebuildedSize += RebuildCells;

            for (int i = 0; i < spawnRect.childCount; i++) {
                Destroy(spawnRect.GetChild(i).gameObject);
            }
        }

        private void RebuildCells() {
            foreach (DisplayItem displayItem in displayItems) {
                Destroy(displayItem.gameObject);
            }

            displayItems.Clear();

            for (int i = 0; i < baseUI.ItemSizeX; i++) {
                for (int j = 0; j < baseUI.ItemSizeY; j++) {
                    GameObject sprite = Instantiate(baseUI.itemPrefab, spawnRect);
                    sprite.SetActive(false);
                    DisplayItem displayItem = sprite.GetComponent<DisplayItem>();
                    displayItem.Init(baseUI);
                    displayItems.Add(displayItem);
                }
            }

            UpdateSearch(false);
        }

        private void Update() {
            if (!hasInit && Player.m_localPlayer != null) {
                Log.LogInfo("Init Search UI");
                Init();
                hasInit = true;
            }
        }

        public void Init() {
            foreach (KeyValuePair<int, Item> item in Indexing.GetActiveItems()) {
                listItems.Add(new ListItem(item.Value));
            }

            // sort alphabetically
            // listItems.Sort(ListItem.Comparer);

            RebuildCells();

            searchField.onValueChanged.AddListener((_) => UpdateSearch(true));

            foreach (TypeToggle typeToggle in baseUI.typeToggles) {
                switch (typeToggle.itemType) {
                    case ItemType.Undefined:
                        typeToggle.image.sprite = Plugin.Instance.noIconSprite;
                        break;
                    case ItemType.Creature:
                        typeToggle.image.sprite = GUIManager.Instance.GetSprite("texts_button");
                        break;
                    case ItemType.Piece:
                        typeToggle.image.sprite = GUIManager.Instance.GetSprite("mapicon_bed");
                        break;
                    case ItemType.Item:
                        typeToggle.image.sprite = GUIManager.Instance.GetSprite("mapicon_trader");
                        break;
                    case ItemType.Food:
                        typeToggle.image.sprite = GUIManager.Instance.GetSprite("food_icon");
                        break;
                    case ItemType.Weapon:
                        typeToggle.image.sprite = GUIManager.Instance.GetSprite("pvp_on");
                        break;
                    case ItemType.Armor:
                        typeToggle.image.sprite = GUIManager.Instance.GetSprite("ac_bkg");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            UpdateSearch(true);
        }

        public void UpdateSearch(bool recalculateActive) {
            baseUI.ShowSearch();
            bool useBlacklist = Plugin.useBlacklist.Value;

            string[] searchKeys = searchField.text.Split();

            if (recalculateActive) {
                Parallel.ForEach(listItems, i => { i.isActive = CalculateActive(i.item, useBlacklist, searchKeys); });
            }

            int totalActive = listItems.Count(i => i.isActive);
            maxPages = Mathf.Max(Mathf.CeilToInt((float) totalActive / displayItems.Count) - 1, 0);
            int displayPage = Mathf.Min(currentPage, maxPages);
            List<ListItem> activeDisplayItems = listItems.Where(i => i.isActive)
                                                         .Skip(displayPage * displayItems.Count)
                                                         .Take(displayItems.Count).ToList();
            pageText.text = $"{displayPage + 1}/{maxPages + 1}";

            for (int i = 0; i < displayItems.Count; i++) {
                displayItems[i].gameObject.SetActive(true);

                if (i < activeDisplayItems.Count) {
                    displayItems[i].SetItem(activeDisplayItems[i].item, 1);
                } else {
                    displayItems[i].SetItem(null, 1);
                }

                RectTransform rectTransform = (RectTransform) displayItems[i].transform;

                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                int row = i % baseUI.ItemSizeX;
                int column = i / baseUI.ItemSizeX;
                rectTransform.anchoredPosition = new Vector2(row + 0.5f, -column - 0.5f) * itemSpacing;
            }
        }

        public void SwitchPage(int skip = 1) {
            currentPage = Mathf.Clamp(currentPage, 0, maxPages);
            currentPage += skip;
            currentPage = Mathf.Clamp(currentPage, 0, maxPages);

            UpdateSearch(false);
        }

        public void FirstPage() {
            currentPage = 0;
            UpdateSearch(false);
        }

        public void LastPage() {
            currentPage = maxPages;
            UpdateSearch(false);
        }

        private bool CalculateActive(Item item, bool useBlacklist, string[] searchKeys) {
            bool onBlackList = useBlacklist && item.isOnBlacklist;

            if (onBlackList) {
                return false;
            }

            if (baseUI.typeToggles.Any(typeToggle => item.itemType == typeToggle.itemType && !typeToggle.isOn)) {
                return false;
            }

            foreach (string searchKey in searchKeys) {
                bool isSearched;

                if (searchKey.StartsWith("@")) {
                    if (item.mod == null) {
                        return false;
                    }

                    isSearched = item.mod.Name.IndexOf(searchKey.Substring(1), StringComparison.OrdinalIgnoreCase) >= 0;
                } else {
                    isSearched = item.localizedName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 item.internalName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (!isSearched) {
                    return false;
                }
            }

            return true;
        }

        private void OnDestroy() {
            TypeToggle.OnChange -= typeToggleOnChange;
            baseUI.RebuildedSize -= RebuildCells;
        }

        private class ListItem {
            public readonly Item item;
            public bool isActive;

            public ListItem(Item item) {
                this.item = item;
                isActive = false;
            }

            public static int Comparer(ListItem a, ListItem b) {
                return string.Compare(a.item.GetPrimaryName(), b.item.GetPrimaryName(), StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}

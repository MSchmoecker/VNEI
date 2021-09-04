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
        public static SearchUI Instance { get; private set; }
        [SerializeField] private Transform spawnRect;
        [SerializeField] public InputField searchField;
        [SerializeField] public Toggle cheat;
        [SerializeField] public Text pageText;

        [SerializeField] public TypeToggle showUndefined;
        [SerializeField] public TypeToggle showCreatures;
        [SerializeField] public TypeToggle showPieces;
        [SerializeField] public TypeToggle showItems;

        private readonly List<ListItem> listItems = new List<ListItem>();
        private readonly List<DisplayItem> displayItems = new List<DisplayItem>();
        private bool hasInit;
        private const int RowCount = 6;
        private const int ItemsInRow = 11;
        private readonly Vector2 itemSpacing = new Vector2(50f, 50f);
        private Action typeToggleOnChange;
        private int currentPage;
        private int maxPages;

        public void Awake() {
            Instance = this;
            hasInit = false;
            typeToggleOnChange = () => UpdateSearch(true);
            TypeToggle.OnChange += typeToggleOnChange;
        }

        private void Update() {
            if (!hasInit && Player.m_localPlayer != null) {
                Log.LogInfo("Init Search UI");
                Init();
                hasInit = true;
            }

            cheat.gameObject.SetActive(Player.m_localPlayer != null && Console.instance.IsCheatsEnabled());
        }

        public void Init() {
            foreach (KeyValuePair<int, Item> item in Indexing.Items) {
                listItems.Add(new ListItem(item.Value));
            }

            listItems.Sort(ListItem.Comparer);

            for (int i = 0; i < RowCount; i++) {
                for (int j = 0; j < ItemsInRow; j++) {
                    GameObject sprite = Instantiate(BaseUI.Instance.itemPrefab, spawnRect);
                    sprite.SetActive(false);
                    displayItems.Add(sprite.GetComponent<DisplayItem>());
                }
            }

            searchField.onValueChanged.AddListener((_) => UpdateSearch(true));

            showUndefined.image.sprite = RecipeUI.Instance.noSprite;
            showCreatures.image.sprite = GUIManager.Instance.GetSprite("texts_button");
            showPieces.image.sprite = GUIManager.Instance.GetSprite("mapicon_bed");
            showItems.image.sprite = GUIManager.Instance.GetSprite("mapicon_trader");

            UpdateSearch(true);
        }

        public void UpdateSearch(bool recalculateLayout) {
            BaseUI.Instance.ShowSearch();
            bool useBlacklist = Plugin.useBlacklist.Value;

            string[] searchKeys = searchField.text.Split();

            if (recalculateLayout) {
                Parallel.ForEach(listItems, i => { i.isActive = CalculateActive(i.item, useBlacklist, searchKeys); });
            }

            int totalActive = listItems.Count(i => i.isActive);
            maxPages = Mathf.Max(Mathf.CeilToInt((float)totalActive / (RowCount * ItemsInRow)) - 1, 0);
            int displayPage = Mathf.Min(currentPage, maxPages);
            List<ListItem> activeDisplayItems = listItems.Where(i => i.isActive)
                                                            .Skip(displayPage * RowCount * ItemsInRow)
                                                            .Take(RowCount * ItemsInRow).ToList();
            pageText.text = $"{displayPage + 1}/{maxPages + 1}";

            for (int i = 0; i < RowCount * ItemsInRow; i++) {
                if (i < activeDisplayItems.Count) {
                    displayItems[i].gameObject.SetActive(true);
                    displayItems[i].SetItem(activeDisplayItems[i].item);
                    RectTransform rectTransform = (RectTransform)displayItems[i].transform;

                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    int row = i % ItemsInRow;
                    int column = i / ItemsInRow;
                    rectTransform.anchoredPosition = new Vector2(row + 0.5f, -column - 0.5f) * itemSpacing;
                } else {
                    displayItems[i].gameObject.SetActive(false);
                }
            }
        }

        public void NextPage() {
            if (currentPage > maxPages) {
                currentPage = maxPages;
            }

            currentPage++;

            if (currentPage > maxPages) {
                currentPage = maxPages;
            }

            UpdateSearch(false);
        }

        public void PreviousPage() {
            if (currentPage > maxPages) {
                currentPage = maxPages;
            }

            currentPage--;

            if (currentPage < 0) {
                currentPage = 0;
            }

            UpdateSearch(false);
        }

        private bool CalculateActive(Item item, bool useBlacklist, string[] searchKeys) {
            bool onBlackList = useBlacklist && item.isOnBlacklist;

            if (onBlackList) {
                return false;
            }

            if (item.itemType == ItemType.Undefined && !showUndefined.isOn) {
                return false;
            }

            if (item.itemType == ItemType.Creature && !showCreatures.isOn) {
                return false;
            }

            if (item.itemType == ItemType.Piece && !showPieces.isOn) {
                return false;
            }

            if (item.itemType == ItemType.Item && !showItems.isOn) {
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

        public bool IsCheating() {
            return cheat.gameObject.activeSelf && cheat.isOn;
        }

        private void OnDestroy() {
            TypeToggle.OnChange -= typeToggleOnChange;
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

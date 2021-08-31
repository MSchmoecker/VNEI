using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class SearchUI : MonoBehaviour {
        public static SearchUI Instance { get; private set; }
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] public InputField searchField;
        private List<MouseHover> sprites = new List<MouseHover>();
        private bool hasInit;
        private const int ItemsInRow = 11;
        private Vector2 itemSpacing = new Vector2(50f, 50f);

        public void Awake() {
            Instance = this;
            hasInit = false;
        }

        private void Update() {
            if (!hasInit && Player.m_localPlayer != null) {
                Log.LogInfo("Init Search UI");
                Init();
                hasInit = true;
            }
        }

        public void Init() {
            foreach (KeyValuePair<int, Item> item in Indexing.Items) {
                GameObject sprite = Instantiate(BaseUI.Instance.itemPrefab, scrollRect.content);
                sprite.GetComponentInChildren<MouseHover>().SetItem(item.Value);
                sprites.Add(sprite.GetComponent<MouseHover>());
                sprite.GetComponent<Image>().sprite = item.Value.GetIcon();
            }

            scrollRect.onValueChanged.AddListener(UpdateInvisible);

            UpdateSearch();
        }

        public void UpdateSearch() {
            BaseUI.Instance.ShowSearch();
            bool useBlacklist = Plugin.useBlacklist.Value;
            int activeItemCount = 0;

            foreach (MouseHover mouseHover in sprites) {
                Item item = mouseHover.item;
                bool active = !(useBlacklist && item.isOnBlacklist);
                bool isSearched = true;

                if (active) {
                    isSearched = item.localizedName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                    isSearched = isSearched || item.internalName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                mouseHover.gameObject.SetActive(active && isSearched);

                RectTransform rectTransform = ((RectTransform)mouseHover.transform);
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                int row = activeItemCount % ItemsInRow;
                int column = activeItemCount / ItemsInRow;
                rectTransform.anchoredPosition = new Vector2Int(row, -column) * itemSpacing + itemSpacing / 2f;

                if (active && isSearched) {
                    activeItemCount++;
                }
            }

            int rowCount = activeItemCount / ItemsInRow;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, rowCount * itemSpacing.y);
            UpdateInvisible(Vector2.zero);
        }

        public void UpdateInvisible(Vector2 slider) {
            Rect rect = ((RectTransform)scrollRect.transform).rect;
            Vector2 scrollPos = scrollRect.content.anchoredPosition;

            foreach (MouseHover sprite in sprites) {
                float posY = ((RectTransform)sprite.transform).anchoredPosition.y;
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;
                sprite.gameObject.SetActive(!invisible);
            }
        }
    }
}

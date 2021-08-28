using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class SearchUI : MonoBehaviour {
        public static SearchUI Instance { get; private set; }
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] public InputField searchField;
        private List<MouseHover> sprites = new List<MouseHover>();
        private bool hasInit;

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

                if (item.Value.icons.Length > 0) {
                    sprite.GetComponent<Image>().sprite = item.Value.icons.First();
                }
            }

            scrollRect.onValueChanged.AddListener(UpdateInvisible);

            UpdateSearch();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            UpdateInvisible(Vector2.zero);
        }

        public void UpdateSearch() {
            BaseUI.Instance.ShowSearch();
            bool useBlacklist = Plugin.useBlacklist.Value;

            foreach (MouseHover mouseHover in sprites) {
                Item item = mouseHover.item;
                bool active = !(useBlacklist && item.isOnBlacklist);
                bool isSearched = true;

                if (active) {
                    isSearched = item.localizedName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                    isSearched = isSearched || item.internalName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                mouseHover.gameObject.SetActive(active && isSearched);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            UpdateInvisible(Vector2.zero);
        }

        public void UpdateInvisible(Vector2 slider) {
            Rect rect = ((RectTransform)scrollRect.transform).rect;
            Vector2 scrollPos = scrollRect.content.anchoredPosition;

            foreach (MouseHover sprite in sprites) {
                float posY = ((RectTransform)sprite.transform).anchoredPosition.y;
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;
                sprite.image.enabled = !invisible;
            }
        }
    }
}

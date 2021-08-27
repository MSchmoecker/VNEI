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

        public void Awake() {
            Instance = this;
            Indexing.IndexFinished += Init;
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

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            UpdateInvisible(Vector2.zero);
        }

        public void UpdateSearch() {
            BaseUI.Instance.ShowSearch();

            foreach (MouseHover mouseHover in sprites) {
                bool isSearched = mouseHover.item.localizedName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                isSearched = isSearched || mouseHover.item.internalName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                mouseHover.gameObject.SetActive(isSearched);
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

        private void OnDestroy() {
            Indexing.IndexFinished -= Init;
        }
    }
}

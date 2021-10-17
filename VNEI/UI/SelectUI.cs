using System;
using System.Linq;
using UnityEngine;
using VNEI.Logic;

namespace VNEI.UI {
    public class SelectUI : MonoBehaviour {
        private Action<string> onSelectCallback;

        /// <summary>
        ///     Creates a Popup window to select an item. Only works after VNEI has indexed all items.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="onSelect">
        ///     Callback of selection, returns a string with the internal name of the prefab.
        ///     If no items are indexed or section is aborted, it returns an empty string.
        /// </param>
        /// <param name="anchoredPosition"></param>
        /// <param name="activeTypes">Default ItemTypes. If null all are shown</param>
        /// <param name="itemsX">Visible columns</param>
        /// <param name="itemsY">Visible rows</param>
        /// <returns></returns>
        public static SelectUI CreateSelection(Transform parent, Action<string> onSelect, Vector2 anchoredPosition,
            ItemType[] activeTypes = null, int itemsX = 6, int itemsY = 4) {
            BaseUI baseUI = BaseUI.CreateBaseUI();
            SelectUI selectUI = baseUI.gameObject.AddComponent<SelectUI>();

            selectUI.transform.SetParent(parent);
            ((RectTransform) selectUI.transform).anchoredPosition = anchoredPosition;

            baseUI.SetSize(false, itemsX, itemsY);

            if (activeTypes != null) {
                foreach (TypeToggle typeToggle in baseUI.typeToggles) {
                    typeToggle.SetOn(activeTypes.Contains(typeToggle.itemType));
                }
            }

            baseUI.searchUi.UpdateSearch(true);

            if (!Indexing.HasIndexed()) {
                Log.LogWarning("Items not ready, returning empty");
                selectUI.SelectItem(null);
            }

            selectUI.onSelectCallback = onSelect;
            selectUI.onSelectCallback += (_) => Destroy(selectUI.gameObject);

            return selectUI;
        }

        public void SelectItem(Item item) {
            onSelectCallback?.Invoke(item?.internalName ?? string.Empty);
        }
    }
}

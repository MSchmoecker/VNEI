using UnityEngine;
using UnityEngine.EventSystems;

namespace VNEI.UI {
    public class ScrollHandler : MonoBehaviour, IScrollHandler {
        public SearchUI searchUi;

        public void OnScroll(PointerEventData eventData) {
            if (!searchUi.gameObject.activeSelf) {
                return;
            }

            float scrollY;

            if (Plugin.normalizeScroll.Value) {
                scrollY = eventData.scrollDelta.normalized.y;
            } else {
                scrollY = eventData.scrollDelta.y;
            }

            int scroll = -Mathf.RoundToInt(scrollY);
            scroll *= Plugin.invertScroll.Value ? -1 : 1;

            if (scroll != 0) {
                searchUi.SwitchPage(scroll);
            }
        }
    }
}

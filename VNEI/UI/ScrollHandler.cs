using UnityEngine;
using UnityEngine.EventSystems;

namespace VNEI.UI {
    public class ScrollHandler : MonoBehaviour, IScrollHandler {
        public SearchUI searchUi;

        public void OnScroll(PointerEventData eventData) {
            if (!searchUi.gameObject.activeSelf) {
                return;
            }

            int scroll = -Mathf.RoundToInt(eventData.scrollDelta.y);
            scroll *= Plugin.invertScroll.Value ? -1 : 1;

            if (scroll != 0) {
                searchUi.SwitchPage(scroll);
            }
        }
    }
}

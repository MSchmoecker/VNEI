using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace VNEI.UI {
    public class Styling {
        public static void ApplyAllComponents(Transform root) {
            foreach (Text text in root.GetComponentsInChildren<Text>(true)) {
                ApplyText(text, GUIManager.Instance.AveriaSerif, new Color(219f / 255f, 219f / 255f, 219f / 255f));
            }

            foreach (InputField inputField in root.GetComponentsInChildren<InputField>(true)) {
                GUIManager.Instance.ApplyInputFieldStyle(inputField);
            }

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(true)) {
                GUIManager.Instance.ApplyToogleStyle(toggle);
            }

            foreach (Button button in root.GetComponentsInChildren<Button>(true)) {
                GUIManager.Instance.ApplyButtonStyle(button);
            }

            foreach (ScrollRect scrollRect in root.GetComponentsInChildren<ScrollRect>(true)) {
                ApplyScrollRect(scrollRect);
            }

            ApplyAllDarken(root);
            ApplyAllSunken(root);
        }

        public static void ApplyWoodpanel(Image image) {
            image.sprite = GUIManager.Instance.GetSprite("woodpanel_trophys");
            image.color = new Color(.6f, .6f, .6f);
        }

        public static void ApplyText(Text text, Font font, Color color) {
            text.font = font;
            text.color = color;
        }

        public static void ApplyLocalization(Transform root) {
            foreach (Text text in root.GetComponentsInChildren<Text>()) {
                text.text = Localization.instance.Localize(text.text);
            }
        }

        public static void ApplyAllDarken(Transform root) {
            foreach (Image image in root.GetComponentsInChildren<Image>(true)) {
                if (image.gameObject.name == "Darken") {
                    image.sprite = GUIManager.Instance.GetSprite("darken_blob");
                    image.color = Color.white;
                }
            }
        }

        public static void ApplyAllSunken(Transform root) {
            foreach (Image image in root.GetComponentsInChildren<Image>(true)) {
                if (image.gameObject.name == "Sunken") {
                    image.sprite = GUIManager.Instance.GetSprite("sunken");
                    image.color = Color.white;
                    image.type = Image.Type.Sliced;
                    image.pixelsPerUnitMultiplier = 1;
                }
            }
        }

        public static void ApplyScrollRect(ScrollRect scrollRect) {
            scrollRect.GetComponent<Image>().sprite = GUIManager.Instance.GetSprite("item_background_sunken");

            if ((bool)scrollRect.horizontalScrollbar) {
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).sprite = GUIManager.Instance.GetSprite("text_field");
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).color = Color.grey;
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).type = Image.Type.Sliced;
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).pixelsPerUnitMultiplier = 2f;
                scrollRect.horizontalScrollbar.GetComponent<Image>().sprite = GUIManager.Instance.GetSprite("text_field");
                scrollRect.horizontalScrollbar.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
            }

            if ((bool)scrollRect.verticalScrollbar) {
                ((Image)scrollRect.verticalScrollbar.targetGraphic).sprite = GUIManager.Instance.GetSprite("text_field");
                ((Image)scrollRect.verticalScrollbar.targetGraphic).color = Color.grey;
                ((Image)scrollRect.verticalScrollbar.targetGraphic).type = Image.Type.Sliced;
                ((Image)scrollRect.verticalScrollbar.targetGraphic).pixelsPerUnitMultiplier = 2f;
                scrollRect.verticalScrollbar.GetComponent<Image>().sprite = GUIManager.Instance.GetSprite("text_field");
                scrollRect.verticalScrollbar.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
            }
        }
    }
}

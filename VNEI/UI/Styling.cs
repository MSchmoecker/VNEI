using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace VNEI.UI {
    public class Styling {
        public static void ApplyAllComponents(Transform root) {
            foreach (Text text in root.GetComponentsInChildren<Text>(true)) {
                if (text.name == "__Name_Text_Field__") {
                    ApplyText(text, GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange, 14);
                } else {
                    ApplyText(text, GUIManager.Instance.AveriaSerif, Color.white, 14);
                }
            }

            foreach (InputField inputField in root.GetComponentsInChildren<InputField>(true)) {
                GUIManager.Instance.ApplyInputFieldStyle(inputField);
            }

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(true)) {
                GUIManager.Instance.ApplyToogleStyle(toggle);
            }

            foreach (Button button in root.GetComponentsInChildren<Button>(true)) {
                if (!button.name.StartsWith("__Copy__")) {
                    GUIManager.Instance.ApplyButtonStyle(button);
                }
            }

            foreach (ScrollRect scrollRect in root.GetComponentsInChildren<ScrollRect>(true)) {
                GUIManager.Instance.ApplyScrollRectStyle(scrollRect);
            }

            ApplyAllDarken(root);
            ApplyAllSunken(root);
        }

        public static void ApplyText(Text text, Font font, Color color, int fontSize) {
            text.font = font;
            text.color = color;
            text.fontSize = fontSize;
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
    }
}

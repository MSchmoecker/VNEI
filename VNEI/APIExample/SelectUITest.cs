using System;
using Jotunn.Entities;
using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class SelectUITest : MonoBehaviour {
        private static SelectUITest instance;

        public RectTransform root;
        public Image background;
        public Text text;
        public Button select;
        private SelectUI selection;

        public static void Create() {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("SelectUITest");
            GameObject spawn = Instantiate(prefab, GUIManager.CustomGUIFront.transform);
            instance = spawn.GetComponent<SelectUITest>();

            instance.background.gameObject.AddComponent<DragWindowCntrl>();

            GUIManager.Instance.ApplyWoodpanelStyle(instance.background.transform);
            GUIManager.Instance.ApplyTextStyle(instance.text);
            GUIManager.Instance.ApplyButtonStyle(instance.select);
        }

        private void Start() {
            select.onClick.AddListener(() => {
                if (!selection) {
                    Action<string> onSelect = prefabName => text.text = prefabName;
                    selection = SelectUI.CreateSelection(GUIManager.CustomGUIFront.transform, onSelect, Vector2.zero,
                        new[] { ItemType.Piece, ItemType.Item });
                }
            });
        }

        private void Update() {
            root.position = background.transform.position;
        }

        public class ToggleUIConsoleCommand : ConsoleCommand {
            public override void Run(string[] args) {
                if (!instance) {
                    Create();
                } else {
                    bool active = instance.background.IsActive();
                    instance.background.gameObject.SetActive(!active);
                    instance.root.gameObject.SetActive(!active);
                }
            }

            public override string Name => "vnei_toggle_select_test";
            public override string Help => "Shows the selection test";
        }
    }
}

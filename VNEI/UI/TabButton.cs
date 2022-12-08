using System;
using UnityEngine;
using UnityEngine.UI;

namespace VNEI.UI {
    public class TabButton : MonoBehaviour {
        private Button button;
        private Text nameText;

        public void Awake() {
            button = GetComponent<Button>();
            nameText = transform.Find("Text").GetComponent<Text>();

            Plugin.tabName.SettingChanged += UpdateTabName;
            UpdateTabName(null, EventArgs.Empty);
        }

        private void OnDestroy() {
            Plugin.tabName.SettingChanged -= UpdateTabName;
        }

        private void UpdateTabName(object sender, EventArgs e) {
            nameText.text = Plugin.tabName.Value;
        }
    }
}

using System;
using System.Linq;
using UnityEngine;
using Jotunn.Managers;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class BaseUI : MonoBehaviour {
        [SerializeField] private RectTransform panel;
        [SerializeField] private GameObject prefab;

        public static void Create() {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("VNEI");
            GameObject root = Instantiate(prefab, GUIManager.CustomGUIFront.transform);
        }

        private void Awake() {
            BuildUI();
        }

        private void BuildUI() {
            foreach (Item item in Indexing.Items) {
                GameObject sprite = Instantiate(prefab, panel);
                sprite.GetComponent<Image>().sprite = item.icons.FirstOrDefault();
                sprite.GetComponentInChildren<MouseHover>().SetText(item.localizedName);
            }
        }
    }
}

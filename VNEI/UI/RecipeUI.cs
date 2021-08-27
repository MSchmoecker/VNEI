using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class RecipeUI : MonoBehaviour {
        public static RecipeUI Instance { get; private set; }

        [SerializeField] public RectTransform results;
        [SerializeField] public RectTransform ingredients;
        [SerializeField] public Image infoIcon;
        [SerializeField] public Text infoName;
        [SerializeField] public Text infoDescription;

        private Item currentItem;

        private void Awake() {
            Instance = this;
        }

        public void SetItem(Item item) {
            currentItem = item;

            for (int i = 0; i < results.childCount; i++) {
                Destroy(results.GetChild(i).gameObject);
            }

            for (int i = 0; i < ingredients.childCount; i++) {
                Destroy(ingredients.GetChild(i).gameObject);
            }

            foreach (RecipeInfo recipe in item.result) {
                SpawnRecipe(recipe, results);
            }

            foreach (RecipeInfo recipe in item.ingredient) {
                SpawnRecipe(recipe, ingredients);
            }

            infoName.text = currentItem.GetName();
            infoDescription.text = Localization.instance.Localize(currentItem.GetDescription());

            if (currentItem.icons.Length > 0) {
                infoIcon.sprite = currentItem.icons.First();
            }
        }

        public void SpawnRecipe(RecipeInfo recipe, RectTransform root) {
            GameObject row = Instantiate(BaseUI.Instance.rowPrefab, root);

            foreach (KeyValuePair<Item, RecipeInfo.Amount> ingredient in recipe.ingredient) {
                SpawnItem(ingredient, row.GetComponent<RectTransform>());
            }

            Instantiate(BaseUI.Instance.arrowPrefab, row.GetComponent<RectTransform>());

            foreach (KeyValuePair<Item, RecipeInfo.Amount> result in recipe.result) {
                SpawnItem(result, row.GetComponent<RectTransform>());
            }
        }

        void SpawnItem(KeyValuePair<Item, RecipeInfo.Amount> item, RectTransform root) {
            GameObject spawnedItem = Instantiate(BaseUI.Instance.itemPrefab, root);

            spawnedItem.GetComponent<MouseHover>().SetItem(item.Key);

            if (item.Key.icons.Length > 0) {
                spawnedItem.GetComponent<Image>().sprite = item.Key.icons.First();
            }

            Text count = spawnedItem.transform.Find("Count").GetComponent<Text>();
            count.text = item.Value.ToString();
            count.gameObject.SetActive(true);
            Styling.ApplyText(count, GUIManager.Instance.AveriaSerif, Color.white);
        }
    }
}

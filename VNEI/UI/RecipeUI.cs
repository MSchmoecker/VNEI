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
        [SerializeField] public Sprite noSprite;

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

            bool useBlacklist = Plugin.useBlacklist.Value;

            foreach (RecipeInfo recipe in item.result) {
                if (useBlacklist && recipe.isOnBlacklist) continue;

                SpawnRecipe(recipe, results);
            }

            foreach (RecipeInfo recipe in item.ingredient) {
                if (useBlacklist && recipe.isOnBlacklist) continue;

                SpawnRecipe(recipe, ingredients);
            }

            infoName.text = currentItem.GetName();
            infoDescription.text = Localization.instance.Localize(currentItem.GetDescription());
            infoIcon.sprite = currentItem.GetIcon();
        }

        public void SpawnRecipe(RecipeInfo recipe, RectTransform root) {
            GameObject row = Instantiate(BaseUI.Instance.rowPrefab, root);

            foreach (Tuple<Item, RecipeInfo.Amount> ingredient in recipe.ingredient) {
                SpawnItem(ingredient, row.GetComponent<RectTransform>());
            }

            Instantiate(BaseUI.Instance.arrowPrefab, row.GetComponent<RectTransform>());

            if (recipe.droppedCount.min != 1 || recipe.droppedCount.max != 1 || Math.Abs(recipe.droppedCount.chance - 1f) > 0.01f) {
                Text recipeDroppedText = Instantiate(BaseUI.Instance.recipeDroppedTextPrefab, row.transform).GetComponent<Text>();
                recipeDroppedText.text = recipe.droppedCount.ToString();
                Styling.ApplyText(recipeDroppedText, GUIManager.Instance.AveriaSerif, Color.white);
            }

            foreach (Tuple<Item, RecipeInfo.Amount> result in recipe.result) {
                SpawnItem(result, row.GetComponent<RectTransform>());
            }
        }

        void SpawnItem(Tuple<Item, RecipeInfo.Amount> item, RectTransform root) {
            GameObject spawnedItem = Instantiate(BaseUI.Instance.itemPrefab, root);

            spawnedItem.GetComponent<MouseHover>().SetItem(item.Item1);
            spawnedItem.GetComponent<Image>().sprite = item.Item1.GetIcon();

            Text count = spawnedItem.transform.Find("Count").GetComponent<Text>();
            count.text = item.Item2.ToString();
            count.gameObject.SetActive(true);
            Styling.ApplyText(count, GUIManager.Instance.AveriaSerif, Color.white);
        }
    }
}

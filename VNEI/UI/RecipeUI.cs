using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class RecipeUI : MonoBehaviour {
        public static RecipeUI Instance { get; private set; }

        [SerializeField] public RectTransform results;
        [SerializeField] public RectTransform ingredients;

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
        }

        public void SpawnRecipe(RecipeInfo recipe, RectTransform root) {
            GameObject row = Instantiate(BaseUI.Instance.rowPrefab, root);

            foreach (Item ingredient in recipe.ingredient) {
                SpawnItem(ingredient, row.GetComponent<RectTransform>());
            }

            Instantiate(BaseUI.Instance.arrowPrefab, row.GetComponent<RectTransform>());

            foreach (Item result in recipe.result) {
                SpawnItem(result, row.GetComponent<RectTransform>());
            }
        }

        void SpawnItem(Item item, RectTransform root) {
            GameObject spawnedItem = Instantiate(BaseUI.Instance.itemPrefab, root);

            spawnedItem.GetComponent<MouseHover>().SetItem(item);
            spawnedItem.GetComponent<Image>().sprite = item.icons.FirstOrDefault();
        }
    }
}

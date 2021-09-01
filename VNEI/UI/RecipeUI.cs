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

        [SerializeField] public ScrollRect obtainingScroll;
        [SerializeField] public ScrollRect usingScroll;
        [SerializeField] public Image infoIcon;
        [SerializeField] public Text infoName;
        [SerializeField] public Text infoDescription;
        [SerializeField] public Sprite noSprite;

        private Item currentItem;
        private List<RectTransform> obtainingItems = new List<RectTransform>();
        private List<RectTransform> usingItems = new List<RectTransform>();
        private const float rowHeight = 70f;

        private void Awake() {
            Instance = this;
        }

        public void SetItem(Item item) {
            currentItem = item;

            foreach (RectTransform resultObject in obtainingItems) {
                Destroy(resultObject.gameObject);
            }

            foreach (RectTransform ingredientObject in usingItems) {
                Destroy(ingredientObject.gameObject);
            }

            obtainingItems.Clear();
            usingItems.Clear();

            bool useBlacklist = Plugin.useBlacklist.Value;

            float maxSizeX = 0;
            float posY = -rowHeight / 2f;

            foreach (RecipeInfo recipe in item.result) {
                if (useBlacklist && recipe.isOnBlacklist) continue;

                float sizeX = SpawnRecipe(recipe, obtainingScroll.content, obtainingItems, posY);

                maxSizeX = Mathf.Max(sizeX, maxSizeX);
                posY -= rowHeight;
            }

            obtainingScroll.content.sizeDelta = new Vector2(maxSizeX, -(posY + rowHeight / 2f - 10f));
            maxSizeX = 0;
            posY = -rowHeight / 2f;

            foreach (RecipeInfo recipe in item.ingredient) {
                if (useBlacklist && recipe.isOnBlacklist) continue;

                float sizeX = SpawnRecipe(recipe, usingScroll.content, usingItems, posY);

                maxSizeX = Mathf.Max(sizeX, maxSizeX);
                posY -= rowHeight;
            }

            usingScroll.content.sizeDelta = new Vector2(maxSizeX, -(posY + rowHeight / 2f - 10f));

            infoName.text = currentItem.GetName();
            infoDescription.text = Localization.instance.Localize(currentItem.GetDescription());
            infoIcon.sprite = currentItem.GetIcon();
        }

        public float SpawnRecipe(RecipeInfo recipe, RectTransform root, List<RectTransform> objects, float posY) {
            GameObject row = Instantiate(BaseUI.Instance.rowPrefab, root);
            RectTransform rowRect = (RectTransform)row.transform;
            objects.Add(rowRect);
            rowRect.anchoredPosition = new Vector2(25f, posY);

            float sizeX = 25;

            foreach (Tuple<Item, Amount> ingredient in recipe.ingredient) {
                sizeX += SpawnItem(ingredient, rowRect, sizeX);
            }

            GameObject arrow = Instantiate(BaseUI.Instance.arrowPrefab, rowRect);
            ((RectTransform)arrow.transform).anchoredPosition = new Vector2(sizeX - 15f, 10f);
            sizeX += ((RectTransform)BaseUI.Instance.arrowPrefab.transform).sizeDelta.x;

            if (recipe.droppedCount.min != 1 || recipe.droppedCount.max != 1 || Math.Abs(recipe.droppedCount.chance - 1f) > 0.01f) {
                Text recipeDroppedText = Instantiate(BaseUI.Instance.recipeDroppedTextPrefab, rowRect).GetComponent<Text>();
                ((RectTransform)recipeDroppedText.transform).anchoredPosition = new Vector2(sizeX, -25f);

                recipeDroppedText.text = recipe.droppedCount.ToString();
                Styling.ApplyText(recipeDroppedText, GUIManager.Instance.AveriaSerif, Color.white);

                sizeX += ((RectTransform)BaseUI.Instance.recipeDroppedTextPrefab.transform).sizeDelta.x;
            }

            foreach (Tuple<Item, Amount> result in recipe.result) {
                sizeX += SpawnItem(result, rowRect, sizeX);
            }

            return sizeX - 15f;
        }

        private float SpawnItem(Tuple<Item, Amount> item, RectTransform root, float posX) {
            GameObject spawnedItem = Instantiate(BaseUI.Instance.itemPrefab, root);

            ((RectTransform)spawnedItem.transform).anchoredPosition = new Vector2(posX, -25f);
            spawnedItem.GetComponent<MouseHover>().SetItem(item.Item1);
            spawnedItem.GetComponent<Image>().sprite = item.Item1.GetIcon();

            Text count = spawnedItem.transform.Find("Count").GetComponent<Text>();
            count.text = item.Item2.ToString();
            count.gameObject.SetActive(true);
            Styling.ApplyText(count, GUIManager.Instance.AveriaSerif, Color.white);

            return ((RectTransform)BaseUI.Instance.itemPrefab.transform).sizeDelta.x;
        }
    }
}

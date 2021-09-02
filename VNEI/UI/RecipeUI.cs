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
        private List<RectTransform> obtainItems = new List<RectTransform>();
        private List<RectTransform> usingItems = new List<RectTransform>();
        private const float rowHeight = 70f;

        private void Awake() {
            Instance = this;
            obtainingScroll.onValueChanged.AddListener((_) => UpdateObtainingHidden());
            usingScroll.onValueChanged.AddListener((_) => UpdateUseHidden());
        }

        public void SetItem(Item item) {
            currentItem = item;

            foreach (RectTransform resultObject in obtainItems) {
                Destroy(resultObject.gameObject);
            }

            foreach (RectTransform ingredientObject in usingItems) {
                Destroy(ingredientObject.gameObject);
            }

            obtainItems.Clear();
            usingItems.Clear();

            bool useBlacklist = Plugin.useBlacklist.Value;

            float maxSizeX = 0;
            float posY = -rowHeight / 2f;

            foreach (RecipeInfo recipe in item.result) {
                if (useBlacklist && recipe.isOnBlacklist) continue;

                float sizeX = SpawnRecipe(recipe, obtainingScroll.content, obtainItems, posY);

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

            UpdateObtainingHidden();
            UpdateUseHidden();
        }

        private void UpdateObtainingHidden() {
            Rect rect = ((RectTransform)obtainingScroll.transform).rect;
            Vector2 scrollPos = obtainingScroll.content.anchoredPosition;

            foreach (RectTransform obtainingItem in obtainItems) {
                RectTransform rectTransform = (RectTransform)obtainingItem.transform;

                float posY = rectTransform.anchoredPosition.y;
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;

                obtainingItem.gameObject.SetActive(!invisible);
            }
        }

        private void UpdateUseHidden() {
            Rect rect = ((RectTransform)usingScroll.transform).rect;
            Vector2 scrollPos = usingScroll.content.anchoredPosition;

            foreach (RectTransform usingItem in usingItems) {
                RectTransform rectTransform = (RectTransform)usingItem.transform;

                float posY = rectTransform.anchoredPosition.y;
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;

                usingItem.gameObject.SetActive(!invisible);
            }
        }

        public float SpawnRecipe(RecipeInfo recipe, RectTransform root, List<RectTransform> objects, float posY) {
            GameObject row = Instantiate(BaseUI.Instance.rowPrefab, root);
            RectTransform rowRect = (RectTransform)row.transform;
            rowRect.anchoredPosition = new Vector2(25f, posY);
            objects.Add(rowRect);

            float sizeX = 25;
            float deltaX;

            foreach (Tuple<Item, Amount> ingredient in recipe.ingredient) {
                SpawnItem(ingredient, rowRect, new Vector2(0, -25f), ref sizeX, out deltaX);
            }

            SpawnRowElement(BaseUI.Instance.arrowPrefab, rowRect, new Vector2(-15f, 10f), ref sizeX, out deltaX);

            if (recipe.droppedCount.min != 1 || recipe.droppedCount.max != 1 || Math.Abs(recipe.droppedCount.chance - 1f) > 0.01f) {
                RectTransform recipeDropped = SpawnRowElement(BaseUI.Instance.recipeDroppedTextPrefab, rowRect, new Vector2(0, -25f),
                                                              ref sizeX, out deltaX);
                Text recipeDroppedText = recipeDropped.GetComponent<Text>();
                recipeDroppedText.text = recipe.droppedCount.ToString();
                Styling.ApplyText(recipeDroppedText, GUIManager.Instance.AveriaSerif, Color.white);
            }

            foreach (Tuple<Item, Amount> result in recipe.result) {
                SpawnItem(result, rowRect, new Vector2(0, -25f), ref sizeX, out deltaX);
            }

            return sizeX - deltaX / 2f;
        }

        private static void SpawnItem(Tuple<Item, Amount> item, Transform root, Vector2 relPos, ref float posX, out float deltaX) {
            RectTransform spawned = SpawnRowElement(BaseUI.Instance.itemPrefab, root, relPos, ref posX, out deltaX);

            MouseHover mouseHover = spawned.GetComponent<MouseHover>();
            mouseHover.SetItem(item.Item1);
            mouseHover.SetCount(item.Item2.ToString());
        }

        private static RectTransform SpawnRowElement(GameObject prefab, Transform parent, Vector2 relPos, ref float posX,
            out float deltaX) {
            GameObject spawned = Instantiate(prefab, parent);
            RectTransform rectTransform = (RectTransform)spawned.transform;

            rectTransform.anchoredPosition = relPos + new Vector2(posX, 0);
            deltaX = rectTransform.sizeDelta.x;
            posX += deltaX;

            return rectTransform;
        }
    }
}

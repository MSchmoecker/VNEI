using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;
using Object = UnityEngine.Object;

namespace VNEI.UI {
    public class RecipeScroll : MonoBehaviour {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Text title;
        private List<RectTransform> rows = new List<RectTransform>();
        private const float RowHeight = 70f;
        private BaseUI baseUI;

        public void Init(BaseUI baseUI) {
            this.baseUI = baseUI;
            scrollRect.onValueChanged.AddListener((_) => UpdateHidden());
        }

        public void Clear() {
            foreach (RectTransform row in rows) {
                Destroy(row.gameObject);
            }

            rows.Clear();
        }

        public void SpawnRows(IEnumerable<RecipeInfo> recipes) {
            bool useBlacklist = Plugin.useBlacklist.Value;
            float maxSizeX = 0;
            float posY = -RowHeight / 2f;

            foreach (RecipeInfo recipe in recipes) {
                if (useBlacklist && recipe.IsOnBlacklist) {
                    continue;
                }

                float sizeX = SpawnRecipe(recipe, scrollRect.content, rows, posY);

                maxSizeX = Mathf.Max(sizeX, maxSizeX);
                posY -= RowHeight;
            }

            scrollRect.content.sizeDelta = new Vector2(maxSizeX, -(posY + RowHeight / 2f - 10f));
            UpdateHidden();
        }

        public void UpdateHidden() {
            Rect rect = ((RectTransform)scrollRect.transform).rect;
            Vector2 scrollPos = scrollRect.content.anchoredPosition;

            foreach (RectTransform obtainingItem in rows) {
                RectTransform rectTransform = (RectTransform)obtainingItem.transform;

                float posY = rectTransform.anchoredPosition.y;
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;

                obtainingItem.gameObject.SetActive(!invisible);
            }
        }

        public float SpawnRecipe(RecipeInfo recipe, RectTransform root, List<RectTransform> objects, float posY) {
            RectTransform row = (RectTransform)Instantiate(baseUI.rowPrefab, root).transform;
            row.anchoredPosition = new Vector2(25f, posY);
            objects.Add(row);

            float sizeX = 25;
            float deltaX;

            foreach (KeyValuePair<Amount, List<Part>> pair in recipe.Ingredients) {
                if (recipe.Ingredients.Count > 1 || pair.Key.min != 1 || pair.Key.max != 1 || Math.Abs(pair.Key.chance - 1f) > 0.01f) {
                    RectTransform recipeDropped = SpawnRowElement(baseUI.recipeDroppedTextPrefab, row, new Vector2(0, -25f),
                                                                  ref sizeX, out deltaX);
                    Text recipeDroppedText = recipeDropped.GetComponent<Text>();
                    recipeDroppedText.text = pair.Key.ToString();
                    Styling.ApplyText(recipeDroppedText, GUIManager.Instance.AveriaSerif, Color.white, 14);
                }

                foreach (Part ingredient in pair.Value) {
                    SpawnItem(ingredient, row, new Vector2(0, -25f), ref sizeX, out deltaX);
                }
            }

            if (recipe.Station == null) {
                SpawnRowElement(baseUI.arrowPrefab, row, new Vector2(-15f, -25f), ref sizeX, out deltaX);
            } else {
                float tmp = sizeX;
                SpawnRowElement(baseUI.arrowPrefab, row, new Vector2(-5f, -15f), ref tmp, out _);
                tmp = sizeX;
                RectTransform spawned = SpawnRowElement(baseUI.itemPrefab, row, new Vector2(-5f, -40f), ref tmp, out _);
                spawned.sizeDelta = new Vector2(30f, 30f);
                deltaX = 40f;
                sizeX += deltaX;
                DisplayItem displayItem = spawned.GetComponent<DisplayItem>();
                displayItem.Init(baseUI);
                displayItem.SetItem(recipe.Station.item, recipe.Station.quality);
            }

            foreach (KeyValuePair<Amount, List<Part>> pair in recipe.Results) {
                if (recipe.Results.Count > 1 || pair.Key.min != 1 || pair.Key.max != 1 || Math.Abs(pair.Key.chance - 1f) > 0.01f) {
                    RectTransform recipeDropped = SpawnRowElement(baseUI.recipeDroppedTextPrefab, row, new Vector2(0, -25f),
                                                                  ref sizeX, out deltaX);
                    Text recipeDroppedText = recipeDropped.GetComponent<Text>();
                    recipeDroppedText.text = pair.Key.ToString();
                    Styling.ApplyText(recipeDroppedText, GUIManager.Instance.AveriaSerif, Color.white, 14);
                }

                foreach (Part ingredient in pair.Value) {
                    SpawnItem(ingredient, row, new Vector2(0, -25f), ref sizeX, out deltaX);
                }
            }

            return sizeX - deltaX / 2f;
        }

        private void SpawnItem(Part part, Transform root, Vector2 relPos, ref float posX, out float deltaX) {
            RectTransform spawned = SpawnRowElement(baseUI.itemPrefab, root, relPos, ref posX, out deltaX);

            DisplayItem displayItem = spawned.GetComponent<DisplayItem>();
            displayItem.Init(baseUI);
            displayItem.SetItem(part.item, part.quality);
            displayItem.SetCount(part.amount.ToString());
        }

        private static RectTransform SpawnRowElement(GameObject prefab, Transform parent, Vector2 relPos, ref float posX, out float deltaX) {
            GameObject spawned = Instantiate(prefab, parent);
            RectTransform rectTransform = (RectTransform)spawned.transform;

            rectTransform.anchoredPosition = relPos + new Vector2(posX, 0);
            deltaX = rectTransform.sizeDelta.x;
            posX += deltaX;

            return rectTransform;
        }

        public void SetActive(bool active) {
            gameObject.SetActive(active);
        }

        public void SetTitle(string titleText) {
            title.text = Localization.instance.Localize(titleText);
        }
    }
}

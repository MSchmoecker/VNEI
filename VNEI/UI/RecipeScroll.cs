using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class RecipeScroll : MonoBehaviour {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Text title;
        private HashSet<RecipeInfo> recipes = new HashSet<RecipeInfo>();
        [NonSerialized] private readonly Dictionary<RecipeInfo, RectTransform> rows = new Dictionary<RecipeInfo, RectTransform>();
        private const float RowHeight = 70f;
        private BaseUI baseUI;

        public void Init(BaseUI baseUI) {
            this.baseUI = baseUI;
            scrollRect.onValueChanged.AddListener((_) => UpdateHidden());
        }

        public void Clear() {
            foreach (RectTransform row in rows.Values) {
                Destroy(row.gameObject);
            }

            rows.Clear();
            recipes.Clear();
        }

        public void SetRecipes(Item targetItem, IEnumerable<RecipeInfo> newRecipes) {
            recipes = new HashSet<RecipeInfo>(newRecipes.Where(r => !r.IsUpgrade(out Item tool) || tool == targetItem));

            float width = 0;
            float height = 0;

            foreach (RecipeInfo recipe in GetActiveRecipes()) {
                width = Mathf.Max(width, recipe.Width);
                height += RowHeight;
            }

            scrollRect.content.sizeDelta = new Vector2(width, height + 5f);
            UpdateHidden();
        }

        public void UpdateHidden() {
            Rect rect = ((RectTransform)scrollRect.transform).rect;
            Vector2 scrollPos = scrollRect.content.anchoredPosition;
            float posY = -RowHeight / 2f;

            foreach (RecipeInfo recipe in GetActiveRecipes()) {
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;

                if (invisible && rows.ContainsKey(recipe)) {
                    Destroy(rows[recipe].gameObject);
                    rows.Remove(recipe);
                } else if (!invisible && !rows.ContainsKey(recipe)) {
                    RectTransform row = SpawnRecipe(recipe, scrollRect.content, posY);
                    row.gameObject.SetActive(true);
                    rows.Add(recipe, row);
                }

                posY -= RowHeight;
            }
        }

        public RectTransform SpawnRecipe(RecipeInfo recipe, RectTransform root, float posY) {
            RectTransform row = (RectTransform)Instantiate(baseUI.rowPrefab, root).transform;
            row.anchoredPosition = new Vector2(25f, posY);

            float sizeX = 25;
            float deltaX = 0;

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

            if (recipe.Stations.Count == 0) {
                SpawnRowElement(baseUI.arrowPrefab, row, new Vector2(-15f, -25f), ref sizeX, out deltaX);
            } else {
                float tmp = sizeX;
                SpawnRowElement(baseUI.arrowPrefab, row, new Vector2(-5f, -15f), ref tmp, out _);

                foreach (Part station in recipe.Stations) {
                    RectTransform spawned = SpawnRowElement(baseUI.itemPrefab, row, new Vector2(-5f, -40f), ref sizeX, out _);
                    sizeX -= 15f;
                    spawned.sizeDelta = new Vector2(30f, 30f);
                    DisplayItem displayItem = spawned.GetComponent<DisplayItem>();
                    displayItem.Init(baseUI);
                    displayItem.SetItem(station.item, station.quality);
                }

                sizeX += 5f;
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

            return row;
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

        private HashSet<RecipeInfo> GetActiveRecipes() {
            if (Plugin.useBlacklist.Value) {
                return recipes.Where(recipe => !recipe.IsOnBlacklist).ToHashSet();
            }

            return recipes;
        }
    }
}

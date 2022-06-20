using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class RecipeUI : MonoBehaviour {
        public ScrollRect obtainingScroll;
        public ScrollRect usingScroll;
        public DisplayItem infoIcon;
        public Text infoName;
        public Text infoInternalName;
        public Text infoModName;
        public Text infoDescription;
        public Image favorite;

        public BaseUI baseUI;
        public event Action<Item> OnSetItem;

        private Item currentItem;
        private List<RectTransform> obtainItems = new List<RectTransform>();
        private List<RectTransform> usingItems = new List<RectTransform>();
        private const float rowHeight = 70f;

        private void Awake() {
            obtainingScroll.onValueChanged.AddListener((_) => UpdateObtainingHidden());
            usingScroll.onValueChanged.AddListener((_) => UpdateUseHidden());
            infoIcon.Init(baseUI);
        }

        public void SetItem(Item item) {
            currentItem = item;
            OnSetItem?.Invoke(currentItem);

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
                if (useBlacklist && recipe.IsOnBlacklist) continue;

                float sizeX = SpawnRecipe(recipe, obtainingScroll.content, obtainItems, posY);

                maxSizeX = Mathf.Max(sizeX, maxSizeX);
                posY -= rowHeight;
            }

            obtainingScroll.content.sizeDelta = new Vector2(maxSizeX, -(posY + rowHeight / 2f - 10f));
            maxSizeX = 0;
            posY = -rowHeight / 2f;

            foreach (RecipeInfo recipe in item.ingredient) {
                if (useBlacklist && recipe.IsOnBlacklist) continue;

                float sizeX = SpawnRecipe(recipe, usingScroll.content, usingItems, posY);

                maxSizeX = Mathf.Max(sizeX, maxSizeX);
                posY -= rowHeight;
            }

            usingScroll.content.sizeDelta = new Vector2(maxSizeX, -(posY + rowHeight / 2f - 10f));

            infoName.text = currentItem.GetName();
            infoInternalName.text = currentItem.internalName;
            infoModName.text = currentItem.GetModName();
            infoDescription.text = Localization.instance.Localize(currentItem.GetDescription());
            infoIcon.SetItem(currentItem, 1);

            UpdateFavoriteButton();
            UpdateObtainingHidden();
            UpdateUseHidden();
        }

        public void ToggleFavorite() {
            currentItem.UpdateFavorite(!currentItem.isFavorite);
            UpdateFavoriteButton();
            FavouritesSave.Save();
        }

        private void UpdateFavoriteButton() {
            favorite.color = currentItem.isFavorite ? GUIManager.Instance.ValheimOrange : Color.grey;
        }

        public void CopyTextToClipboard(Text text) {
            GUIUtility.systemCopyBuffer = text.text;
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
    }
}

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
        [SerializeField] public DisplayItem infoIcon;
        [SerializeField] public Text infoName;
        [SerializeField] public Text infoNameContext;
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
            infoNameContext.text = currentItem.GetNameContext();
            infoDescription.text = Localization.instance.Localize(currentItem.GetDescription());
            infoIcon.SetItem(currentItem, 1);

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
            RectTransform row = (RectTransform)Instantiate(BaseUI.Instance.rowPrefab, root).transform;
            row.anchoredPosition = new Vector2(25f, posY);
            objects.Add(row);

            float sizeX = 25;
            float deltaX;

            foreach (Part ingredient in recipe.ingredient) {
                SpawnItem(ingredient, row, new Vector2(0, -25f), ref sizeX, out deltaX);
            }

            if (recipe.station == null) {
                SpawnRowElement(BaseUI.Instance.arrowPrefab, row, new Vector2(-15f, -25f), ref sizeX, out deltaX);
            } else {
                float tmp = sizeX;
                SpawnRowElement(BaseUI.Instance.arrowPrefab, row, new Vector2(-5f, -15f), ref tmp, out _);
                tmp = sizeX;
                RectTransform spawned = SpawnRowElement(BaseUI.Instance.itemPrefab, row, new Vector2(-5f, -40f), ref tmp, out _);
                spawned.sizeDelta = new Vector2(30f, 30f);
                deltaX = 40f;
                sizeX += deltaX;
                spawned.GetComponent<DisplayItem>().SetItem(recipe.station.item, recipe.station.quality);
            }

            if (recipe.droppedCount.min != 1 || recipe.droppedCount.max != 1 || Math.Abs(recipe.droppedCount.chance - 1f) > 0.01f) {
                RectTransform recipeDropped = SpawnRowElement(BaseUI.Instance.recipeDroppedTextPrefab, row, new Vector2(0, -25f),
                                                              ref sizeX, out deltaX);
                Text recipeDroppedText = recipeDropped.GetComponent<Text>();
                recipeDroppedText.text = recipe.droppedCount.ToString();
                Styling.ApplyText(recipeDroppedText, GUIManager.Instance.AveriaSerif, Color.white, 14);
            }

            foreach (Part result in recipe.result) {
                SpawnItem(result, row, new Vector2(0, -25f), ref sizeX, out deltaX);
            }

            return sizeX - deltaX / 2f;
        }

        private static void SpawnItem(Part part, Transform root, Vector2 relPos, ref float posX, out float deltaX) {
            RectTransform spawned = SpawnRowElement(BaseUI.Instance.itemPrefab, root, relPos, ref posX, out deltaX);

            DisplayItem displayItem = spawned.GetComponent<DisplayItem>();
            displayItem.SetItem(part.item, part.quality);
            displayItem.SetCount(part.amount.ToString());
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

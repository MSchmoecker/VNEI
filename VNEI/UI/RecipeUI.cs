using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class RecipeUI : MonoBehaviour {
        public RecipeScroll obtainingScroll;
        public RecipeScroll usingScroll;
        public RecipeScroll bothScroll;
        public RecipeScroll combinedScroll;

        public CraftingStationList craftingStationList;

        public DisplayItem infoIcon;
        public Text infoName;
        public Text infoInternalName;
        public Text infoModName;
        public Text infoDescription;
        public Image favorite;

        public Button showOnlyObtainingButton;
        public Button showOnlyUsingButton;
        public Button showBothButton;

        public BaseUI baseUI;
        public event Action<Item> OnSetItem;

        private Item currentItem;
        private RecipeView currentView = RecipeView.Combined;

        private void Awake() {
            obtainingScroll.Init(baseUI);
            usingScroll.Init(baseUI);
            bothScroll.Init(baseUI);
            combinedScroll.Init(baseUI);
            infoIcon.Init(baseUI);
            craftingStationList.OnChange += UpdateRecipeView;
        }

        public void SetItem(Item item) {
            currentItem = item;
            OnSetItem?.Invoke(currentItem);

            craftingStationList.SetStations(currentItem.GetStations().OrderBy(StationOrder.ByType).ThenBy(StationOrder.ByName).ToList());

            UpdateRecipeView();

            infoName.text = currentItem.preLocalizeName;
            infoInternalName.text = currentItem.internalName;
            infoModName.text = currentItem.GetModName();
            infoDescription.text = currentItem.GetDescription();
            infoIcon.SetItem(currentItem, 1);

            Localize(infoName);
            Localize(infoDescription);

            UpdateFavoriteButton();
        }

        private static void Localize(Text text) {
            // not using Localization.instance(text) because it will cache the key only if the localization is found, which can cause issues on re-opening the UI
            Localization.instance.textStrings[text] = text.text;
            text.text = Localization.instance.Localize(text.text);
        }

        private void UpdateRecipeView() {
            obtainingScroll.Clear();
            usingScroll.Clear();
            bothScroll.Clear();
            combinedScroll.Clear();

            RecipeView view = currentView;

            showOnlyObtainingButton.interactable = currentItem.result.Count != 0;
            showOnlyUsingButton.interactable = currentItem.ingredient.Count != 0;

            if (view == RecipeView.Obtaining && currentItem.result.Count == 0) {
                view = RecipeView.ObtainingAndUsing;
            }

            if (view == RecipeView.Using && currentItem.ingredient.Count == 0) {
                view = RecipeView.ObtainingAndUsing;
            }

            bothScroll.SetActive(view == RecipeView.Obtaining || view == RecipeView.Using);
            obtainingScroll.SetActive(view == RecipeView.ObtainingAndUsing);
            usingScroll.SetActive(view == RecipeView.ObtainingAndUsing);
            combinedScroll.SetActive(view == RecipeView.Combined);

            switch (view) {
                case RecipeView.Obtaining:
                    bothScroll.SetRecipes(currentItem, craftingStationList.FilterRecipes(currentItem.result));
                    bothScroll.SetTitle("$vnei_obtaining");
                    break;
                case RecipeView.Using:
                    bothScroll.SetRecipes(currentItem, craftingStationList.FilterRecipes(currentItem.ingredient));
                    bothScroll.SetTitle("$vnei_using");
                    break;
                case RecipeView.ObtainingAndUsing:
                    obtainingScroll.SetRecipes(currentItem, craftingStationList.FilterRecipes(currentItem.result));
                    usingScroll.SetRecipes(currentItem, craftingStationList.FilterRecipes(currentItem.ingredient));
                    break;
                case RecipeView.Combined:
                    combinedScroll.SetRecipes(currentItem, craftingStationList.FilterRecipes(currentItem.result.Concat(currentItem.ingredient)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        [UsedImplicitly]
        public void SetView(int view) {
            SetView((RecipeView)view);
        }

        public void SetView(RecipeView view) {
            currentView = view;
            UpdateRecipeView();
        }

        public Item GetItem() {
            return currentItem;
        }
    }
}

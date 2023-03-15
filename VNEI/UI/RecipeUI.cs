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
        private RecipeView currentView = RecipeView.ObtainingAndUsing;

        private void Awake() {
            obtainingScroll.Init(baseUI);
            usingScroll.Init(baseUI);
            bothScroll.Init(baseUI);
            infoIcon.Init(baseUI);
        }

        public void SetItem(Item item) {
            currentItem = item;
            OnSetItem?.Invoke(currentItem);

            UpdateRecipeView();

            infoName.text = currentItem.preLocalizeName;
            infoInternalName.text = currentItem.internalName;
            infoModName.text = currentItem.GetModName();
            infoDescription.text = currentItem.GetDescription();
            infoIcon.SetItem(currentItem, 1);

            Localization.instance.Localize(transform);

            UpdateFavoriteButton();
        }

        private void UpdateRecipeView() {
            obtainingScroll.Clear();
            usingScroll.Clear();
            bothScroll.Clear();

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

            switch (view) {
                case RecipeView.Obtaining:
                    bothScroll.SetRecipes(currentItem.result);
                    bothScroll.SetTitle("$vnei_obtaining");
                    break;
                case RecipeView.Using:
                    bothScroll.SetRecipes(currentItem.ingredient);
                    bothScroll.SetTitle("$vnei_using");
                    break;
                case RecipeView.ObtainingAndUsing:
                    obtainingScroll.SetRecipes(currentItem.result);
                    usingScroll.SetRecipes(currentItem.ingredient);
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

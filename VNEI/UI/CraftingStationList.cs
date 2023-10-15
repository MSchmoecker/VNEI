using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class CraftingStationList : MonoBehaviour {
        public Button prevPageButton;
        public Button nextPageButton;
        public List<CraftingStationElement> stationElements = new List<CraftingStationElement>();
        private int currentPage;

        private float elementXSpace = 5f;

        public Item ActiveStation { get; private set; }

        public event Action OnChange;

        // private void Awake() {
        //     Plugin.GetMainUI().GetBaseUI().RebuildedSize
        // }

        public void SetStations(List<Item> stations) {
            ActiveStation = stations.Contains(ActiveStation) ? ActiveStation : stations.FirstOrDefault();
            ClearStations();

            float elementSizeX = ((RectTransform)Plugin.Instance.craftingStationTemplate.transform).sizeDelta.x + elementXSpace;
            float width = ((RectTransform)transform).rect.width;
            int stationsPerPage = Mathf.FloorToInt(width / elementSizeX);
            float centerOffset = (width - elementSizeX * stationsPerPage + elementSizeX) / 2f;

            for (int i = 0; i < stations.Count; i++) {
                Item station = stations[i];
                CraftingStationElement stationElement = Instantiate(Plugin.Instance.craftingStationTemplate, transform).GetComponent<CraftingStationElement>();
                stationElements.Add(stationElement);
                stationElement.name = station.internalName;
                stationElement.station = station;
                stationElement.icon.sprite = station.GetIcon();
                stationElement.tooltip.Set(station.preLocalizeName, "");
                stationElement.gameObject.SetActive(i >= currentPage * stationsPerPage && i < (currentPage + 1) * stationsPerPage);

                RectTransform rectTransform = (RectTransform)stationElement.transform;
                float posX = centerOffset + elementSizeX * (i % stationsPerPage);
                rectTransform.anchoredPosition = new Vector2(posX, rectTransform.anchoredPosition.y);

                stationElement.button.onClick.AddListener(() => {
                    ActiveStation = station;
                    UpdateButtons();
                    OnChange?.Invoke();
                });
            }

            UpdateButtons();
        }

        private void UpdateButtons() {
            float elementSizeX = ((RectTransform)Plugin.Instance.craftingStationTemplate.transform).sizeDelta.x + elementXSpace;
            float width = ((RectTransform)transform).rect.width;
            int stationsPerPage = Mathf.FloorToInt(width / elementSizeX);

            for (int i = 0; i < stationElements.Count; i++) {
                CraftingStationElement stationElement = stationElements[i];

                ColorBlock colors = stationElement.button.colors;
                colors.normalColor = ActiveStation == stationElement.station ? Color.white : Color.gray;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                stationElement.button.colors = colors;

                stationElement.icon.color = ActiveStation == stationElement.station ? Color.white : Color.gray;
                stationElement.gameObject.SetActive(i >= currentPage * stationsPerPage && i < (currentPage + 1) * stationsPerPage);
            }

            prevPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < GetPageCount() - 1;
        }

        private void ClearStations() {
            foreach (CraftingStationElement stationElement in stationElements) {
                Destroy(stationElement.gameObject);
            }

            stationElements.Clear();
            currentPage = 0;
        }

        public IEnumerable<RecipeInfo> FilterRecipes(IEnumerable<RecipeInfo> recipes) {
            if (ActiveStation == Plugin.Instance.allStations) {
                return recipes;
            }

            if (ActiveStation == Plugin.Instance.noStation) {
                return recipes.Where(r => r.Stations.Count == 0 || r.Stations.All(s => s.item == null));
            }

            return recipes.Where(r => r.Stations.Count >= 1 && r.Stations[0].item == ActiveStation);
        }

        private int GetPageCount() {
            float elementSizeX = ((RectTransform)Plugin.Instance.craftingStationTemplate.transform).sizeDelta.x + elementXSpace;
            float width = ((RectTransform)transform).rect.width;
            return Mathf.CeilToInt(elementSizeX * stationElements.Count / width);
        }

        public void PrevPage() {
            currentPage = Mathf.Max(currentPage - 1, 0);
            UpdateButtons();
        }

        public void NextPage() {
            currentPage = Mathf.Min(currentPage + 1, GetPageCount());
            UpdateButtons();
        }
    }
}

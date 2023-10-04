using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class CraftingStationList : MonoBehaviour {
        private List<CraftingStationElement> stationelements = new List<CraftingStationElement>();

        public Item ActiveStation { get; private set; }

        public event Action OnChange;

        public void SetStations(List<Item> stations) {
            ActiveStation = stations.Contains(ActiveStation) ? ActiveStation : stations.FirstOrDefault();
            ClearStations();

            float posX = 20f;

            foreach (Item station in stations) {
                CraftingStationElement stationElement = Instantiate(Plugin.Instance.craftingStationTemplate, transform).GetComponent<CraftingStationElement>();
                stationelements.Add(stationElement);
                stationElement.station = station;
                stationElement.icon.sprite = station.GetIcon();
                stationElement.tooltip.Set(station.preLocalizeName, "");

                RectTransform rectTransform = (RectTransform)stationElement.transform;
                rectTransform.anchoredPosition = new Vector2(posX, rectTransform.anchoredPosition.y);
                posX += rectTransform.sizeDelta.x + 5;

                stationElement.button.onClick.AddListener(() => {
                    ActiveStation = station;
                    UpdateButtons();
                    OnChange?.Invoke();
                });
            }

            UpdateButtons();
        }

        private void UpdateButtons() {
            foreach (CraftingStationElement stationElement in stationelements) {
                ColorBlock colors = stationElement.button.colors;
                colors.normalColor = ActiveStation == stationElement.station ? Color.white : Color.gray;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                stationElement.button.colors = colors;

                stationElement.icon.color = ActiveStation == stationElement.station ? Color.white : Color.gray;
            }
        }

        private void ClearStations() {
            foreach (CraftingStationElement stationElement in stationelements) {
                Destroy(stationElement.gameObject);
            }

            stationelements.Clear();
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
    }
}

using VNEI.Logic;

namespace VNEI.UI {
    public static class StationOrder {
        public static int ByType(Item item) {
            if (item == Plugin.Instance.allStations) {
                return 10;
            }

            if (item == Plugin.Instance.noStation) {
                return 15;
            }

            if (item == Plugin.Instance.handStation) {
                return 20;
            }
            
            if (!item.prefab) {
                return 100;
            }

            if (item.prefab.GetComponent<CraftingStation>()) {
                return 30;
            }

            if (item.prefab.TryGetComponent(out ItemDrop itemDrop) && itemDrop.m_itemData.m_shared.m_buildPieces) {
                return 40;
            }

            if (item.prefab.GetComponent<Smelter>()) {
                return 50;
            }

            if (item.prefab.GetComponent<CookingStation>()) {
                return 60;
            }

            if (item.prefab.GetComponent<Fermenter>()) {
                return 70;
            }

            if (item.prefab.GetComponent<Incinerator>()) {
                return 80;
            }

            return 100;
        }

        public static int ByName(Item item) {
            if (!item.prefab) {
                return 100 + item.preLocalizeName.Length;
            }

            switch (item.prefab.name) {
                case "piece_workbench":
                    return 10;
                case "forge":
                    return 20;
                case "piece_stonecutter":
                    return 30;
                case "piece_cauldron":
                    return 40;
                case "piece_artisanstation":
                    return 50;
                case "blackforge":
                    return 60;
                case "piece_magetable":
                    return 70;
            }

            switch (item.prefab.name) {
                case "Hammer":
                    return 10;
                case "Hoe":
                    return 20;
                case "Cultivator":
                    return 30;
            }

            return 100 + item.prefab.name.Length;
        }
    }
}

using VNEI.Logic;

namespace VNEI.UI {
    public class HistorySnapshot {
        private readonly BaseUI targetUI;
        private readonly Window window;
        private readonly Item item;

        public HistorySnapshot(BaseUI targetUI, Window window, Item item) {
            this.targetUI = targetUI;
            this.window = window;
            this.item = item;
        }

        public void Show() {
            if (!targetUI) {
                return;
            }

            if (window == Window.Search) {
                targetUI.ShowSearch(false);
            } else if (window == Window.Recipe) {
                targetUI.ShowRecipe(item, false);
            }
        }

        public string Description() {
            return $"Window: {window}, Item: {item?.GetName() ?? "-"}";
        }
    }
}

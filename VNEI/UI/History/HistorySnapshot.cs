using VNEI.Logic;

namespace VNEI.UI {
    public abstract class HistorySnapshot {
        protected readonly BaseUI targetUI;

        public HistorySnapshot(BaseUI targetUI) {
            this.targetUI = targetUI;
        }

        public abstract void Show();
        public abstract string Description();
    }

    public class HistorySnapshotSearch : HistorySnapshot {
        public HistorySnapshotSearch(BaseUI targetUI) : base(targetUI) {
        }

        public override void Show() {
            if (targetUI) {
                targetUI.ShowSearch(false);
            }
        }

        public override string Description() {
            return $"Search";
        }
    }

    public class HistorySnapshotRecipe : HistorySnapshot {
        private readonly Item item;

        public HistorySnapshotRecipe(BaseUI targetUI, Item item) : base(targetUI) {
            this.item = item;
        }

        public override void Show() {
            if (targetUI) {
                targetUI.ShowRecipe(item, false);
            }
        }

        public override string Description() {
            return $"Recipe: {item?.GetPrimaryName() ?? "-"}";
        }
    }
}

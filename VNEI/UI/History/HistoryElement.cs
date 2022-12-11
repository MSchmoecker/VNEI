using Jotunn.Managers;

namespace VNEI.UI {
    public class HistoryElement : UndoManager.IUndoAction {
        private readonly HistorySnapshot previous;
        private readonly HistorySnapshot next;

        public HistoryElement(HistorySnapshot previous, HistorySnapshot next) {
            this.previous = previous;
            this.next = next;
        }

        public void Undo() {
            previous.Show();
        }

        public void Redo() {
            next.Show();
        }

        public string Description() => $"Previous: {previous.Description()}, Next: {next.Description()}";

        public string UndoMessage() => string.Empty;

        public string RedoMessage() => string.Empty;
    }
}

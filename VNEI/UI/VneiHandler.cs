namespace VNEI.UI {
    public interface VneiHandler {
        BaseUI CreateBaseUI(bool forceRecreate = false);
        BaseUI GetBaseUI();
        void SetTabActive();
    }
}

namespace VNEI.UI {
    public class SearchKey {
        public string key;
        public bool isNegative;
        public bool isMod;
        public bool isItemDropType;

        public SearchKey(string searchKey) {
            key = searchKey;
            isNegative = key.StartsWith("-");

            if (isNegative) {
                key = key.Substring(1);
            }

            isMod = key.StartsWith("@");
            isItemDropType = key.StartsWith("#");

            if (isMod || isItemDropType) {
                key = key.Substring(1);
            }
        }
    }
}

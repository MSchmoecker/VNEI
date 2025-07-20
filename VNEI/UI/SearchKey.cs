namespace VNEI.UI {
    public class SearchKey {
        public string key;
        public bool isNegative;
        public bool isMod;
        public bool isItemDropType;
        public bool isIngredient;

        public SearchKey(string searchKey) {
            key = searchKey;
            isNegative = key.StartsWith("-");

            if (isNegative) {
                key = key.Substring(1);
            }

            isMod = key.StartsWith("@");
            isItemDropType = key.StartsWith("#");
            isIngredient = key.StartsWith("^");

            if (isMod || isItemDropType || isIngredient) {
                key = key.Substring(1);
            }
        }
    }
}

namespace VNEI.UI {
    public class SearchKey {
        public string key;
        public bool isNegative;
        public bool isMod;

        public SearchKey(string searchKey) {
            key = searchKey;
            isNegative = key.StartsWith("-");

            if (isNegative) {
                key = key.Substring(1);
            }

            isMod = key.StartsWith("@");

            if (isMod) {
                key = key.Substring(1);
            }
        }
    }
}

namespace VNEI.Logic {
    public class Part {
        public readonly Item item;
        public readonly Amount amount;
        public readonly int quality;

        public Part(Item item, Amount amount, int quality) {
            this.item = item;
            this.amount = amount;
            this.quality = quality;
        }

        protected bool Equals(Part other) {
            return Equals(item, other.item) && amount.Equals(other.amount) && quality == other.quality;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Part)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (item != null ? item.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ amount.GetHashCode();
                hashCode = (hashCode * 397) ^ quality;
                return hashCode;
            }
        }
    }
}

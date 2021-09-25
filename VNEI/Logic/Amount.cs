using UnityEngine;

namespace VNEI.Logic {
    public readonly struct Amount {
        public readonly int min;
        public readonly int max;
        public readonly float chance;

        public static Amount Zero { get; } = new Amount(0, 0, 0f);
        public static Amount One { get; } = new Amount(1);

        public Amount(int min, int max, float chance = 1f) {
            this.min = min;
            this.max = max;
            this.chance = chance;
        }

        public Amount(int amount, float chance = 1f) {
            min = amount;
            max = amount;
            this.chance = chance;
        }

        public static bool IsSameMinMax(Amount a, Amount b) {
            return a.min == b.min && a.max == b.max;
        }

        public override string ToString() {
            int percent = Mathf.RoundToInt(chance * 100f);
            string value = "";
            bool hasPercent = false;

            if (percent != 100) {
                value += $"{percent}% ";
                hasPercent = true;
            }

            if (min == max) {
                if (!(hasPercent && max == 1)) {
                    value += $"{max}x";
                }
            } else {
                value += $"{min}-{max}x";
            }

            return value;
        }

        public bool Equals(Amount other) {
            return min == other.min && max == other.max && chance.Equals(other.chance);
        }

        public override bool Equals(object obj) {
            return obj is Amount other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = min;
                hashCode = (hashCode * 397) ^ max;
                hashCode = (hashCode * 397) ^ chance.GetHashCode();
                return hashCode;
            }
        }
    }
}

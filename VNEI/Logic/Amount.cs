﻿using UnityEngine;

namespace VNEI.Logic {
    public struct Amount {
        public int min;
        public int max;
        public bool fixedCount;
        public float chance;

        public static Amount Zero { get; } = new Amount(0, 0, 0f);

        public Amount(int min, int max, float chance = 1f) {
            this.min = min;
            this.max = max;
            fixedCount = min == max;
            this.chance = chance;
        }

        public Amount(int amount, float chance = 1f) {
            min = amount;
            max = amount;
            fixedCount = true;
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

            if (fixedCount) {
                if (!(hasPercent && max == 1)) {
                    value += $"{max}x";
                }
            } else {
                value += $"{min}-{max}x";
            }

            return value;
        }
    }
}

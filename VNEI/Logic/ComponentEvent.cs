using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VNEI.Logic {
    public class ComponentEvent {
        private readonly Dictionary<Component, Action> listener = new Dictionary<Component, Action>();

        public void AddListener(Component component, Action action) {
            if (!listener.ContainsKey(component)) {
                listener.Add(component, action);
            }
        }

        public void RemoveListener(Component component) {
            listener.Remove(component);
        }

        public void Invoke() {
            bool anyDestroyedKey = false;

            foreach (KeyValuePair<Component, Action> pair in listener) {
                if (!pair.Key) {
                    anyDestroyedKey = true;
                    continue;
                }

                pair.Value?.Invoke();
            }

            if (anyDestroyedKey) {
                RemoveDestroyedKeys();
            }
        }

        private void RemoveDestroyedKeys() {
            listener.Keys
                    .Where(i => !i)
                    .ToList()
                    .ForEach(i => listener.Remove(i));
        }
    }
}

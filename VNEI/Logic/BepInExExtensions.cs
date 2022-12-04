using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace VNEI.Logic {
    public static class BepInExExtensions {
        /// <summary>
        ///     Checks if the main key was just pressed and all modifiers are pressed.
        ///     This also works if other unrelated keys are pressed down, not like BepInEx's build-in <see cref="KeyboardShortcut.IsDown"/>
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        public static bool IsKeyDown(this KeyboardShortcut shortcut) {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }
    }
}

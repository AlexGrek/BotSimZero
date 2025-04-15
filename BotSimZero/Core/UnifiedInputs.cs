using Stride.Engine;
using Stride.Input;

namespace BotSimZero.Core
{
    public static class ScriptComponentExtensions
    {
        /// <summary>
        /// Checks if the left mouse button was clicked or A gamepad key.
        /// </summary>
        /// <param name="scriptComponent">The ScriptComponent instance.</param>
        /// <returns>True if the left mouse button was clicked or A gamepad key, otherwise false.</returns>
        public static bool IsClicked(this ScriptComponent scriptComponent)
        {
            var input = scriptComponent.Input;
            return input.IsMouseButtonPressed(MouseButton.Left) || scriptComponent.IsGamepadAPressed();
        }

        /// <summary>
        /// Enumerates all connected gamepads and checks if the A button is pressed (not held down).
        /// </summary>
        /// <param name="scriptComponent">The ScriptComponent instance.</param>
        /// <returns>True if the A button was pressed on any gamepad, otherwise false.</returns>
        public static bool IsGamepadAPressed(this ScriptComponent scriptComponent)
        {
            foreach (var gamepad in scriptComponent.Input.GamePads)
            {
                if (gamepad.IsButtonPressed(GamePadButton.A))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

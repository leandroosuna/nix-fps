using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
    public class MouseKey : Key
    {
        MouseButton button;
        public MouseKey(MouseButton button)
        {
            this.button = button;
        }

        public override bool IsDown()
        {
            switch(button)
            {
                case MouseButton.Left: return mouseState.LeftButton == ButtonState.Pressed;
                case MouseButton.Right: return mouseState.RightButton == ButtonState.Pressed;
                case MouseButton.Middle: return mouseState.MiddleButton == ButtonState.Pressed;
                default: return false;
            }
        }
        public override string Name()
        {
            switch(button)
            {
                case MouseButton.Left: return "MB1";
                case MouseButton.Right: return "MB2";
                case MouseButton.Middle: return "MB3";
            }
            return "MB1";
        }
    }
    public class KeyboardKey : Key
    {
        Keys key;
        public KeyboardKey(Keys key)
        {
            this.key = key;
        }
        public override bool IsDown()
        {
            return keyState.IsKeyDown(key);
        }
        public override string Name()
        {
            return key.ToString();
        }
    }
    public class ScrollWheel : Key
    {
        int lastValue;
        bool retVal;
        bool isUp;
        public ScrollWheel(bool isUp)
        {
            lastValue = mouseState.ScrollWheelValue;
            this.isUp = isUp;
        }

        public override bool IsDown()
        {
            if (isUp)
                retVal = lastValue < mouseState.ScrollWheelValue;
            else
                retVal = lastValue > mouseState.ScrollWheelValue;
                
            lastValue = mouseState.ScrollWheelValue;

            return retVal;
        }
        public override string Name()
        {
            return isUp ? "MW UP" : "MW Down";
        }
    }
    public abstract class Key
    {
        public static MouseState mouseState;
        public static KeyboardState keyState;

        public static void Update(MouseState ms, KeyboardState ks)
        {
            mouseState = ms;
            keyState = ks;
        }
        public abstract bool IsDown();

        public abstract string Name();
    }
}

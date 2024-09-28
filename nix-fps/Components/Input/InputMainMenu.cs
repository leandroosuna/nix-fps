using Microsoft.Xna.Framework;
using nixfps.Components.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    public class InputMainMenu : InputManager
    {
        public InputMainMenu() : base()
        {
            mouseLocked = false;
        }
        public new void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
        public override void ProcessInput(float deltaTime)
        {
            //if (keyMappings.Escape.IsDown())
            //{
            //    if (!keysDown.Contains(keyMappings.Escape))
            //        game.StopGame();
            //}
            //if (keyMappings.Enter.IsDown())
            //{
            //    keysDown.Add(keyMappings.Enter);
            //    GameStateManager.SwitchTo(State.RUN);
            //}
        }
        public override void ApplyInput(ClientInputState state)
        {

        }
    }
}
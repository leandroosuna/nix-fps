using Microsoft.Xna.Framework;
using nixfps.Components.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    public class InputOptions : InputManager
    {
        public InputOptions() : base()
        {
            mouseLocked = false;
        }
        public new void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
        public override void ProcessInput(float deltaTime)
        {
            
        }
        public override void ApplyInput(ClientInputState state)
        {

        }
    }
}
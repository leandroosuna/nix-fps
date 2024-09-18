using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.States
{
    public class StateOptions : GameState
    {
        public StateOptions() : base()
        {
            
        }
        public override void OnSwitch()
        {
            game.IsMouseVisible = true;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

    }
}

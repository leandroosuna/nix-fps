using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.HUD
{
    public class Hud
    {
        public Crosshair crosshair;
        NixFPS game;
        public SpriteBatch spriteBatch;
        public Hud()
        {
            game = NixFPS.GameInstance();
            crosshair = new Crosshair(game);
           
        }
        public void Update(float deltaTime)
        {
            crosshair.Update();
        }
        public void DrawMenu(float deltaTime)
        {
            
        }
        public void DrawRun(float deltaTime)
        {
            spriteBatch.Begin();
            crosshair.Draw();
            spriteBatch.End();
        }
    }
}

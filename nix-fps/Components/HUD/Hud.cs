using Microsoft.Xna.Framework;
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
            var lp = game.localPlayer;

            spriteBatch.Begin();
            crosshair.Draw();

            var gun = game.gunManager.currentGun;
            var magStr = $"{gun.magSize - gun.shotsFired}/{gun.magSize}";

            if (gun.reload)
                magStr += " R";

            spriteBatch.DrawString(game.fontXLarge, "+", new Vector2(game.screenWidth / 3, game.screenHeight - 55), Color.White);
            spriteBatch.DrawString(game.fontLarge, ""+lp.health, new Vector2(game.screenWidth / 3 + 25, game.screenHeight - 50), Color.White);
            spriteBatch.DrawString(game.fontLarge,
               magStr, new Vector2(game.screenWidth  *2 / 3 + 25, game.screenHeight - 50), Color.White);
            spriteBatch.End();
        }
    }
}

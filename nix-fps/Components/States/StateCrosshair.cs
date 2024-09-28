using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.GUI;
using nixfps.Components.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.States
{
    public class StateCrosshair : GameState
    {
        public StateCrosshair() : base()
        {
            inputManager = new InputOptions();
            gui = new GuiCrosshair(); 
        }
        public override void OnSwitch()
        {
            game.IsMouseVisible = true;
            
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            game.camera.RotateBy(new Vector2(uDeltaTimeFloat, 0));
            game.hud.crosshair.Center(game.screenWidth / 3, 0);
            game.hud.Update(uDeltaTimeFloat);

        }
        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.SetRenderTarget(null);
            game.GraphicsDevice.Clear(Color.Black);
            game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            game.skybox.Draw();
            game.camera.RotateBy(new Vector2(dDeltaTimeFloat * 2, 0));
            gui.Draw(gameTime);
            game.hud.DrawRun(uDeltaTimeFloat);

            base.Draw(gameTime);

        }

    }
}

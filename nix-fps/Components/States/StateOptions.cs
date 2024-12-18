﻿using Microsoft.Xna.Framework;
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
    public class StateOptions : GameState
    {
        public StateOptions() : base()
        {
            inputManager = new InputOptions();
            gui = new GuiOptions(this); 
        }
        public override void OnSwitch()
        {
            game.IsMouseVisible = true;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            game.camera.RotateBy(new Vector2(uDeltaTimeFloat, 0));

        }
        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.SetRenderTarget(null);
            game.GraphicsDevice.Clear(Color.Black);
            game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            game.skybox.Draw(game.camera.view, game.camera.projection, game.camera.position);
            game.camera.RotateBy(new Vector2(dDeltaTimeFloat * 2, 0));
            gui.Draw(gameTime);

            base.Draw(gameTime);

        }

    }
}

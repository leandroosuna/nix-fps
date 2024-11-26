using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Input;
using nixfps.Components.GUI;

namespace nixfps.Components.States
{
    public class StateMainMenu : GameState
    {
        

        public StateMainMenu() : base()
        {
            inputManager = new InputMainMenu();
            gui = new GuiMain(this);
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
            base.Draw(gameTime);

            game.GraphicsDevice.SetRenderTarget(null);
            game.GraphicsDevice.Clear(Color.Black);
            game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            game.camera.RotateBy(new Vector2(dDeltaTimeFloat * 2, 0));
            game.skybox.Draw(game.camera.view, game.camera.projection, game.camera.position, false);

            gui.Draw(gameTime);
        }

    }
}

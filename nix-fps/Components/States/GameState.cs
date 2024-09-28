using Microsoft.Xna.Framework;
using nixfps.Components.GUI;
using nixfps.Components.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.States
{
    public abstract class GameState
    {
        public InputManager inputManager;
        public NixFPS game;
        public double uDeltaTimeDouble;
        public float uDeltaTimeFloat;
        public double dDeltaTimeDouble;
        public float dDeltaTimeFloat;
        public int FPS;
        public Gui gui;
        public GameState()
        {
            game = NixFPS.GameInstance();
            //gui = new GuiRun();
        }
        public abstract void OnSwitch();
        public virtual void Update(GameTime gameTime)
        {
            uDeltaTimeDouble = gameTime.ElapsedGameTime.TotalSeconds;
            uDeltaTimeFloat = (float)uDeltaTimeDouble;
            
            inputManager.Update(uDeltaTimeFloat);

        }
        public virtual void Draw(GameTime gameTime)
        {
            dDeltaTimeDouble = gameTime.ElapsedGameTime.TotalSeconds;
            dDeltaTimeFloat = (float)uDeltaTimeDouble;
            FPS = (int)(1 / dDeltaTimeDouble);
        }

    }
}

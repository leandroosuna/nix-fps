using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using nixfps.Components.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.States
{
    public class GameStateManager
    {
        static NixFPS game;
        public static StateMainMenu stateMainMenu;
        public static StateOptions stateOptions;
        public static StateRun stateRun;
        public static StateInputMap stateInputMap;
        public static StateCrosshair stateCrosshair;
        public static bool paused = false;
        static GameState last;
        public static void Init()
        {
            game = NixFPS.GameInstance();
            stateMainMenu = new StateMainMenu();
            stateOptions = new StateOptions();
            stateRun = new StateRun();
            stateInputMap = new StateInputMap();
            stateCrosshair = new StateCrosshair();
        }
        public static void SwitchTo(State state)
        {
            last = game.gameState;
            GameState newState = game.gameState;
            switch (state)
            {
                case State.MAIN:
                    newState = stateMainMenu;
                    break;
                case State.OPTIONS:
                    newState = stateOptions;
                    break;
                case State.RUN:

                    newState = stateRun;
                    break;
                case State.PAUSE:

                    break;
                case State.INPUTMAP:
                    newState = stateInputMap;
                    break;
                case State.CROSSHAIR:
                    newState = stateCrosshair;
                    break;

            }

            game.gameState = newState;
            newState.OnSwitch();

        }
        public static void TogglePause()
        {
            paused = !paused;
            //SwitchTo(paused ? State.PAUSE : State.RUN);

            if (paused)
            {
                game.IsMouseVisible = true;
                game.gameState.inputManager.mouseLocked = false;
            }
            else
            {
                game.gameState.inputManager.mouseLocked = true;
                System.Windows.Forms.Cursor.Position = game.gameState.inputManager.center;
                game.IsMouseVisible = false;
            }

        }

        public static void SwitchToLast()
        {
            
            game.gameState = last == stateCrosshair? stateMainMenu : last;
            game.gameState.OnSwitch();
        }
    }
    public enum State
    {
        MAIN,
        OPTIONS,
        INPUTMAP,
        RUN,
        PAUSE,
        CROSSHAIR
    }
}

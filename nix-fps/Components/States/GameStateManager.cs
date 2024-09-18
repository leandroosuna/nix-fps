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
        public static StatePause statePause;

        public static void Init()
        { 
            game = NixFPS.GameInstance();
            stateMainMenu = new StateMainMenu();
            stateOptions = new StateOptions();
            stateRun = new StateRun();
            statePause = new StatePause();
        }
        public static void SwitchTo(State state)
        {
            GameState newState = stateMainMenu;
            switch(state)
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
                    newState = statePause;
                    break;

            }

            game.gameState = newState;
            newState.OnSwitch();
            
        }
        
    }
    public enum State
    {
        MAIN,
        OPTIONS,
        RUN,
        PAUSE
    }
}

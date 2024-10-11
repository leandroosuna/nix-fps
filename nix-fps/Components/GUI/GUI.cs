using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.GUI.ImGuiNET;
using nixfps.Components.GUI.Modifiers;
using nixfps.Components.Network;
using nixfps.Components.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nixfps.NixFPS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace nixfps.Components.GUI
{
    public abstract class Gui
    {
        public static NixFPS game;
        public static ImGuiRenderer renderer;
        public ModifierController controller;
        public GameState linkedGameState;
        public static void Init()
        {
            game = NixFPS.GameInstance();
            renderer = new ImGuiRenderer(game);
            //controller = new ModifierController();
            renderer.RebuildFontAtlas();
        }
        public Gui(GameState gs) 
        {
            controller = new ModifierController();
            AddControllers();
            controller.Bind(renderer);
            linkedGameState = gs;
        }

        public abstract void AddControllers();

        //private static void onFloatChange(float obj)
        //{
        //    Debug.WriteLine("testfloat " + obj);
        //}

        public void Draw(GameTime gameTime)
        {
            renderer.BeforeLayout(gameTime);
            DrawLayout(gameTime);
            //controller.Draw();
            renderer.AfterLayout();
        }
        public abstract void DrawLayout(GameTime gameTime);
    }
}

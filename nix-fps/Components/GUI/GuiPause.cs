using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Network;
using nixfps.Components.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace nixfps.Components.GUI
{
    public class GuiPause : Gui
    {
        public GuiPause() : base()
        { }

        public override void AddControllers()
        {
            
        }

        public override void DrawLayout(GameTime gameTime)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(450,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));
            var style = ImGui.GetStyle();
            style.Colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.HeaderActive] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(0, .5f, .5f, 1);

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .75f, .75f, 1);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, 1, 1, 1);

            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new System.Numerics.Vector4(0, .5f, .5f, 1);

            style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(0, .75f, .75f, 1);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(0, 1, 1, 1);

            style.Alpha = 1;
            ImGui.Begin("nix FPS | Pausa", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            
            
            ImGui.Dummy(new System.Numerics.Vector2(1, 90));

            if (ImGui.Button("Volver al juego"))
            {
                GameStateManager.TogglePause();
            }
            ImGui.Dummy(new System.Numerics.Vector2(1, 30));
            if (ImGui.Button("Opciones"))
            {
                GameStateManager.TogglePause();
                GameStateManager.SwitchTo(State.OPTIONS);
            }
            ImGui.Dummy(new System.Numerics.Vector2(1, 30));
            if (ImGui.Button("Salir al menu"))
            {
                GameStateManager.TogglePause();
                GameStateManager.SwitchTo(State.MAIN);
            }
        }
    }
}

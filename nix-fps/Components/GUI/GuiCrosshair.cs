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
    public class GuiCrosshair : Gui
    {
        public GuiCrosshair(GameState gs) : base(gs)
        { }

        public override void AddControllers()
        {
            var c = game.hud.crosshair;
            controller.AddToggle("habilitado", c.SetEnabled, GetEnabled());             //0

            controller.AddToggle("lineas", c.SetEnableLines, GetEnableLines());         //1
            controller.AddColor("color lineas", c.SetColor, GetColor());                //2
            controller.AddFloat("largo lineas", c.SetLength, GetLineLength(),1,400);          //3
            controller.AddFloat("grosor lineas", c.SetThickness, GetLineThickness(),1,400);   //4
            controller.AddFloat("offset lineas", c.SetOffset, GetLineOffset(),0,400);         //5

            controller.AddToggle("dot", c.SetEnableDot, GetEnableDot());                //6
            controller.AddColor("color dot", c.SetDotColor, GetColor());                   //7
            controller.AddFloat("size dot", c.SetDotSize, GetDotSize(),1,400);                //8

            controller.AddToggle("outline", c.SetEnableOutline, GetEnableOutline());    //9
            controller.AddColor("color outline", c.SetColorOutline, GetOutlineColor()); //10
            controller.AddFloat("grosor outline", c.SetThicknessOutline, GetOutlineThickness(),1,200); //11

        }


        public override void DrawLayout(GameTime gameTime)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(700,
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
            ImGui.Begin("nix FPS | Crosshair", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

            var c = game.hud.crosshair;
            controller.Draw(0, 1);
            if(c.enabled)
            {
                controller.Draw(1, 2);
                if(c.mainLines)
                    controller.Draw(2, 6);
                controller.Draw(6, 7);
                if(c.centerDot)
                    controller.Draw(7, 9);
                controller.Draw(9, 10);
                if (c.outline)
                    controller.Draw(10, 12);



            }

            ImGui.Text("presets");
            if(ImGui.Button("1"))
            {

            }
            ImGui.SameLine();
            if (ImGui.Button("2"))
            {
            }
            ImGui.SameLine();
            if (ImGui.Button("3"))
            {
            }
            ImGui.SameLine();
            if (ImGui.Button("4"))
            {
            }
            ImGui.SameLine();
            if (ImGui.Button("5"))
            {
            }
            ImGui.SameLine();
            if (ImGui.Button("6"))
            {
            }

            ImGui.Dummy(new System.Numerics.Vector2(1, 10));
            if (ImGui.Button("Atras"))
            {
                ///TODO: save to file

                GameStateManager.SwitchTo(State.OPTIONS);
            }
            //ImGui.SameLine();
            //if (ImGui.Button("Guardar cambios"))
            //{
                
            //}
        }

        Color GetColor()
        {
            return Color.White;
        }
        Color GetOutlineColor()
        {
            return Color.Black;
        }
        bool GetEnableLines()
        {
            return true;
        }
        bool GetEnableOutline()
        {
            return true;
        }
        bool GetEnableDot()
        {
            return false;
        }
        bool GetEnabled()
        {
            return true;
        }

        int GetLineLength()
        {
            return 5;
        }
        int GetLineThickness()
        {
            return 2;
        }
        int GetLineOffset()
        {
            return 4;
        }
        int GetOutlineThickness()
        {
            return 1;
        }
        int GetDotSize()
        {
            return 2;
        }

    }
}

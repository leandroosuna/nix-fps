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
            controller.AddToggle("habilitado", c.SetEnabled, c.GetEnabled());             //0

            controller.AddToggle("lineas", c.SetEnableLines, c.GetEnableLines());         //1
            controller.AddColor("color lineas", c.SetColor, c.GetColor());                //2
            controller.AddFloat("largo lineas", c.SetLength, c.GetLineLength(),1,400);          //3
            controller.AddFloat("grosor lineas", c.SetThickness, c.GetLineThickness(),1,400);   //4
            controller.AddFloat("offset lineas", c.SetOffset, c.GetLineOffset(),0,400);         //5

            controller.AddToggle("dot", c.SetEnableDot, c.GetEnableDot());                //6
            controller.AddColor("color dot", c.SetDotColor, c.GetDotColor());                   //7
            controller.AddFloat("size dot", c.SetDotSize, c.GetDotSize(),1,400);                //8

            controller.AddToggle("outline", c.SetEnableOutline, c.GetEnableOutline());    //9
            controller.AddColor("color outline", c.SetColorOutline, c.GetOutlineColor()); //10
            controller.AddFloat("grosor outline", c.SetThicknessOutline, c.GetOutlineThickness(),1,200); //11

        }


        public override void DrawLayout(GameTime gameTime)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(700,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));
            var style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1, 1f, 1f, 1);
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
            ImGui.Text("Modificando slot");

            //style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .25f, .25f, 1);
            //style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .25f, .25f, 1);
            //style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, .25f, .25f, 1);
            //style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(.5f, .5f, .5f, 1);
            var slotActive = c.GetSlotActive();


            HighlightButton(ref style, slotActive == 0);
            if (ImGui.Button("1"))
                c.SetSlotActive(0);
            HighlightButton(ref style, slotActive == 1);
            ImGui.SameLine();
            if (ImGui.Button("2"))
                c.SetSlotActive(1);
            HighlightButton(ref style, slotActive == 2);
            ImGui.SameLine();
            if (ImGui.Button("3"))
                c.SetSlotActive(2);
            HighlightButton(ref style, slotActive == 3);
            ImGui.SameLine();
            if (ImGui.Button("4"))
                c.SetSlotActive(3);
            HighlightButton(ref style, slotActive == 4);
            ImGui.SameLine();
            if (ImGui.Button("5"))
                c.SetSlotActive(4);
            HighlightButton(ref style, slotActive == 5);
            ImGui.SameLine();
            if (ImGui.Button("6"))
                c.SetSlotActive(5);
            HighlightButton(ref style, slotActive == 6);
            ImGui.SameLine();
            if (ImGui.Button("7"))
                c.SetSlotActive(6);
            HighlightButton(ref style, slotActive == 7);
            ImGui.SameLine();
            if (ImGui.Button("8"))
                c.SetSlotActive(7);
            HighlightButton(ref style, slotActive == 8);
            ImGui.SameLine();
            if (ImGui.Button("9"))
                c.SetSlotActive(8);
            HighlightButton(ref style, slotActive == 9);
            ImGui.SameLine();
            if (ImGui.Button("10"))
                c.SetSlotActive(9);


            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .75f, .75f, 1);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, 1, 1, 1);

            ImGui.Dummy(new System.Numerics.Vector2(1, 20));

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

            

            ImGui.Dummy(new System.Numerics.Vector2(1, 10));
            if (ImGui.Button("Atras"))
            {
                GameStateManager.SwitchTo(State.OPTIONS);
            }
            ImGui.SameLine();
            if (ImGui.Button("Guardar cambios"))
            {
                c.SaveFile();
            }
        }

        void HighlightButton(ref ImGuiStylePtr style, bool val)
        {
            if(val)
            {
                style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .8f, .2f, 1);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .8f, .2f, 1);
                style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, .8f, .2f, 1);
            }
            else
            {
                style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .5f, .5f, 1);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .75f, .75f, 1);
                style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, 1, 1, 1);
            }
        }

    }
}

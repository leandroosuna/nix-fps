using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using nixfps.Components.Input;
using nixfps.Components.Network;
using nixfps.Components.States;
using System.IO;


namespace nixfps.Components.GUI
{
    public class GuiInputMap : Gui
    {
        public GuiInputMap(GameState gs) : base(gs)
        { }

        public override void AddControllers()
        {
            //var km = InputManager.keyMappings;
            //controller.AddButton(km.Forward.Name() + " Adelante", EditForward(2);
        }


        

        public void KeyPressed()
        {

        }

        public void StopKeyAssign()
        {

        }
        bool cfgChange;
         
        string btnForward = InputManager.keyMappings.Forward.Name() + ": Adelante";

        string strPressAnyKey = "Presione una tecla";
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
            ImGui.Begin("nix FPS | Opciones (Mapeo de Teclas)", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.SeparatorText("Movimiento");
            var km = InputManager.keyMappings;
            if (ImGui.Button(btnForward))
            {
                if (btnForward != strPressAnyKey)
                {
                    btnForward = strPressAnyKey;
                    //start key input
                }
                else
                {
                    btnForward = km.Forward.Name() + ": Adelante";
                }
            }

            //controller.Draw(0,1);
            ImGui.SeparatorText("Acciones");
            //controller.Draw(0, 6);

            if (ImGui.Button("Guardar cambios"))
            {
                bool anyChange = false;
                
                if(anyChange || cfgChange)
                {
                    cfgChange = false;
                    //km.UpdateKey("");
                }
            }
            ImGui.Dummy(new System.Numerics.Vector2(1, 15));

            if (ImGui.Button("Atras"))
            {
                //GameStateManager.SwitchTo(State.MAIN);
                GameStateManager.SwitchToLast();

            }
        }
    }
}

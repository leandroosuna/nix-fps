using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
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
    public class GuiMain : Gui
    {
        public GuiMain(GameState gs) : base(gs)
        { }

        public override void AddControllers()
        {

        }
        static String username = game.CFG.ContainsKey("PlayerName") ? game.CFG["PlayerName"].Value<string>() : "nombre";
        static String user = "";
        public override void DrawLayout(GameTime gameTime)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(450,
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
            ImGui.Begin("nix FPS | Menu principal", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.Dummy(new System.Numerics.Vector2(1, 30));

            ImGui.Text("Estado del Server:");
            if (NetworkManager.Client.IsConnecting)
            {
                ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 0, 1), $"Conectando... (Intento {NetworkManager.ConnectionAttempts})");
            }
            else if (NetworkManager.Client.IsConnected)
            {
                System.Numerics.Vector4 col = new System.Numerics.Vector4(0f, 0f, 0f, 1);
                if (NetworkManager.Client.RTT < 25)
                    col = new System.Numerics.Vector4(0f, 1f, 1f, 1);
                else if (NetworkManager.Client.RTT < 50)
                    col = new System.Numerics.Vector4(0f, 1f, 0f, 1);
                else if (NetworkManager.Client.RTT < 100)
                    col = new System.Numerics.Vector4(1f, 1f, 0, 1);
                else
                    col = new System.Numerics.Vector4(1f, 0, 0, 1);

                ImGui.TextColored(col, "Online " + NetworkManager.Client.RTT + " ms");
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(.25f, .25f, .25f, 1), "Offline");
            }

            ImGui.Dummy(new System.Numerics.Vector2(1, 30));
            
            
            if(!NetworkManager.Client.IsConnected)
            {
                style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .25f, .25f, 1);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .25f, .25f, 1);
                style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, .25f, .25f, 1);
                style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(.5f, .5f, .5f, 1);
            }
            if (ImGui.Button("Entrar al juego"))
            {
                if(NetworkManager.Client.IsConnected)
                {
                    game.CFG["PlayerName"] = username;
                    NetworkManager.SetLocalPlayerName(username);
                    NetworkManager.SendPlayerIdentity();
                    GameStateManager.SwitchTo(State.RUN);
                }
            }

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0, .5f, .5f, 1);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0, .75f, .75f, 1);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0, 1, 1, 1);
            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1, 1f, 1f, 1);
            ImGui.Dummy(new System.Numerics.Vector2(1, 30));

            ImGui.Text("Nombre del jugador");
            ImGui.InputText("##username", ref username, 20);
            ImGui.Dummy(new System.Numerics.Vector2(1, 60));

            if (ImGui.Button("Opciones"))
            {
                GameStateManager.SwitchTo(State.OPTIONS);
            }
            //ImGui.Dummy(new System.Numerics.Vector2(1, 15));
            //if (ImGui.Button("Mapeo de teclas"))
            //{
            //    GameStateManager.SwitchTo(State.INPUTMAP);
            //}
            ImGui.Dummy(new System.Numerics.Vector2(1, 15));
            if (ImGui.Button("Salir"))
            {
                game.StopGame();
            }
        }
    }
}

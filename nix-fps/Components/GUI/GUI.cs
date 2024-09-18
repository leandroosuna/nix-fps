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
    public class Gui
    {
        static NixFPS game;
        static ImGuiRenderer renderer;
        static ModifierController controller;

        public static void Init()
        {
            game = NixFPS.GameInstance();
            renderer = new ImGuiRenderer(game);
            controller = new ModifierController();
            renderer.RebuildFontAtlas();
        }

        public static void AddControllers()
        {
            //controller.AddFloat("testfloat", onFloatChange, 0);
        }

        private static void onFloatChange(float obj)
        {
            Debug.WriteLine("testfloat " + obj);
        }

        public static void Bind()
        {
            controller.Bind(renderer);
        }
        public static void Draw(GameTime gameTime)
        {
            renderer.BeforeLayout(gameTime);

            if (game.gameState is StateMainMenu)
            {
                DrawLayoutMainMenu();
            }
            else if (game.gameState is StateRun)
            {
                //DrawLayoutRun();
                DrawLayoutMainMenu();
            }
            

            controller.Draw();
            renderer.AfterLayout();
        }
        static String username = "nix";
        static String user = "";

        public static void DrawLayoutMainMenu()
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(2, 0));
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
            
            ImGui.Dummy(new System.Numerics.Vector2(1,30));
            ImGui.Text("Nombre del jugador");
            ImGui.InputText("##username", ref username, 20);
            ImGui.Dummy(new System.Numerics.Vector2(1, 15));
            if (ImGui.Button("Entrar a los tiros"))
            {
                user = username;
                NetworkManager.SendPlayerIdentity();
            }

            ImGui.Dummy(new System.Numerics.Vector2(1, 30));

            if (ImGui.Button("Opciones"))
            {
                
            }
            ImGui.Dummy(new System.Numerics.Vector2(1, 30));
            if (ImGui.Button("Salir"))
            {
                game.StopGame();
            }

        }

        public static void DrawLayoutRun()
        {

        }
    }
}

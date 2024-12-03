using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using nixfps.Components.Input;
using nixfps.Components.Network;
using nixfps.Components.States;
using System;
using System.IO;


namespace nixfps.Components.GUI
{
    public class GuiOptions : Gui
    {
        public GuiOptions(GameState gs) : base(gs)
        { }

        public override void AddControllers()
        {
            string []resolutions = { "1280x720", "1366x768","1600x900","1920x1080","2160x1440", "3840x2160"};
            string[] modes = { "Fullscreen", "Borderless" };
            string[] presets = { "Ultra", "Alto", "Medio", "Bajo" };
            controller.AddOptions("Resolucion", resolutions, GetResolution(), ResolutionChange);
            controller.AddOptions("Modo pantalla", modes, GetMode(), ScreenModeChange);
            controller.AddOptions("Graficos", presets, GetGraphicsPreset(), GraphicsPresetChange);
            controller.AddToggle("V SYNC", EnableVsync, GetVsync());
            controller.AddFloat("MAX FPS", game.SetFPSLimit, GetFPSLimit(), 0, 1000);


            controller.AddFloat("FOV", ChangeFOV, GetFOV(), 45, 170);

            controller.AddFloat("Mouse Sens", ChangeMouseSens, GetMouseSens(), 0.001f, 5);

            controller.AddFloat("Escala", game.hud.ChangeMapSize, .7f, 0f, 1f);
            controller.AddToggle("Rotacion", game.hud.EnableRotation, false);
                

        }

        enum Resolution
        {
            r1280x720,
            r1366x768,
            r1600x900,
            r1920x1080,
            r2560x1440,
            r3840x2160
        }
        enum ScreenMode
        {
            Fullscreen,
            Borderless
        }
        enum GraphicsPreset
        {
            ultra,
            high,
            medium,
            low
        }
        int newResWidth;
        int newResHeight;
        bool newFullscreen;
        bool newVysnc;
        string newGraphics;
        private float GetFPSLimit()
        {
            return game.CFG["FPSLimit"].Value<int>();
        }


        Resolution GetResolution()
        {
            switch(game.CFG["ScreenWidth"].Value<int>())
            {
                case 1280: return Resolution.r1280x720;
                case 1366: return Resolution.r1366x768;
                case 1600: return Resolution.r1600x900;
                case 1920: return Resolution.r1920x1080;
                case 2560: return Resolution.r2560x1440;
                case 3840: return Resolution.r3840x2160;

            }
            return Resolution.r1280x720;
        }
        ScreenMode GetMode()
        {
            return game.CFG["Fullscreen"].Value<bool>() ? ScreenMode.Fullscreen : ScreenMode.Borderless;
        }
        GraphicsPreset GetGraphicsPreset()
        {
            switch(game.CFG["GraphicsPreset"].Value<string>())
            {
                case "ultra": return GraphicsPreset.ultra;
                case "high": return GraphicsPreset.high;
                case "medium": return GraphicsPreset.medium;
                case "low": return GraphicsPreset.low;
            }
            return GraphicsPreset.high;
        }
        bool GetVsync()
        {
            return game.CFG["VSync"].Value<bool>();
        }
        float GetFOV()
        {
            return game.CFG["FOV"].Value<int>();
        }
        float GetMouseSens()
        {
            return game.CFG["MouseSensitivity"].Value<float>();
        }
        
        void ResolutionChange(Resolution res)
        {
            int screenWidth = game.CFG["ScreenWidth"].Value<int>();
            int screenHeight = game.CFG["ScreenHeight"].Value<int>();
            switch (res)
            {
                case Resolution.r1280x720: screenWidth = 1280; screenHeight = 720; break;
                case Resolution.r1366x768: screenWidth = 1366; screenHeight = 768; break;
                case Resolution.r1600x900: screenWidth = 1600; screenHeight = 900; break;
                case Resolution.r1920x1080: screenWidth = 1920; screenHeight = 1080; break;
                case Resolution.r2560x1440: screenWidth = 2560; screenHeight = 1440; break;
                case Resolution.r3840x2160: screenWidth = 3840; screenHeight = 2160; break;

            }

            newResWidth = screenWidth;
            newResHeight = screenHeight;

        }

        void ScreenModeChange(ScreenMode screenMode)
        {
            newFullscreen = screenMode == ScreenMode.Fullscreen;
        }
        void GraphicsPresetChange(GraphicsPreset preset)
        {
            newGraphics = preset.ToString();
        }
        void EnableVsync(bool enable)
        {
            if (game == null)
                return;

            newVysnc = enable;
            
        }
        void ChangeFOV(float fov)
        {
            if (game == null)
                return;
            game.CFG["FOV"] = (int)fov;
            game.camera.ChangeFOV((int)fov);
            cfgChange = true;
        }
        void ChangeMouseSens(float sens)
        {
            game.CFG["MouseSensitivity"] = sens;
            InputManager.mouseSensitivity = sens;
            cfgChange = true;
        }
        bool cfgChange;
        
        
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
            ImGui.Begin("nix FPS | Opciones", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            
            ImGui.SeparatorText("Graficos");
            controller.Draw(0, 5);
            ImGui.SeparatorText("Camara");
            controller.Draw(5, 7);
            ImGui.SeparatorText("Mini mapa");
            controller.Draw(7, 9);

            ImGui.Dummy(new System.Numerics.Vector2(1, 30));
            
            ImGui.Dummy(new System.Numerics.Vector2(1, 15));

            if (ImGui.Button("Crosshair"))
            {
                GameStateManager.SwitchTo(State.CROSSHAIR);
            }


            ImGui.Dummy(new System.Numerics.Vector2(1, 15));

            if (ImGui.Button("Atras"))
            {
                //GameStateManager.SwitchTo(State.MAIN);
                GameStateManager.SwitchToLast();

            }
            ImGui.SameLine();
            int dw = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int dh = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            if (ImGui.Button("Guardar cambios"))
            {
                bool anyChange = false;
                if (game.Graphics.IsFullScreen != newFullscreen)
                {
                    anyChange = true;
                    game.Graphics.IsFullScreen = newFullscreen;
                    game.Window.IsBorderless = true;
                    game.CFG["Fullscreen"] = newFullscreen;
                    
                }
                if (game.Graphics.PreferredBackBufferWidth != newResWidth)
                {
                    anyChange = true;
                    
                    
                    
                    game.Graphics.PreferredBackBufferWidth = newResWidth;
                    game.Graphics.PreferredBackBufferHeight = newResHeight;
                    game.CFG["ScreenWidth"] = newResWidth;
                    game.CFG["ScreenHeight"] = newResHeight;
                    game.screenWidth = newResWidth;
                    game.screenHeight = newResHeight;

                        
                    game.SetupRenderTargets();
                    game.hud.crosshair.Center();
                    
                }
                
                if (game.graphicsPreset != newGraphics)
                {
                    anyChange = true;
                    game.graphicsPreset = newGraphics;
                    game.CFG["GraphicsPreset"] = newGraphics;

                    game.SetupRenderTargets();

                }
                if (game.Graphics.SynchronizeWithVerticalRetrace != newVysnc)
                {
                    anyChange = true;
                    game.Graphics.SynchronizeWithVerticalRetrace = newVysnc;
                    game.CFG["VSync"] = newVysnc;
                }
                
                if (anyChange || cfgChange)
                {
                    cfgChange = false;
                    game.Graphics.ApplyChanges();
                    if(!game.Graphics.IsFullScreen)
                    {
                        game.Window.IsBorderless = true;

                        game.Window.Position = new Point((dw - newResWidth) / 2, (dh - newResHeight) / 2);
                        if (newResWidth > dw || newResHeight > dh)
                            game.Window.Position = new Point(0, 0);
                    }
                    game.hud.crosshair.Center();
                    File.WriteAllText("app-settings.json", game.CFG.ToString());
                }
            }
        }
    }
}

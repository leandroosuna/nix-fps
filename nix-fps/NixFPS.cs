using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Cameras;
using nixfps.Components.Effects;
using nixfps.Components.Lights;
using nixfps.Components.Network;
using nixfps.Components.Skybox;
using nixfps.Components.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using nixfps.Components.HUD;

namespace nixfps
{
    public class NixFPS: Game
    {
        public const string ContentFolder2D = "2D/";
        public const string ContentFolder3D = "3D/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderFonts = "Fonts/";

        static NixFPS game;
        public DeferredEffect deferredEffect;
        public BasicModelEffect basicModelEffect;
        public int screenWidth, screenHeight;
        public GraphicsDeviceManager Graphics;
        public SpriteBatch spriteBatch;
        public FullScreenQuad fullScreenQuad;
        SpriteFont font;

        Skybox skybox;
        public static Texture2D Pixel { get; set; }

        //public IConfigurationRoot CFG;
        public Camera camera;
        public Point screenCenter;

        public Model plane;
        public Model cube;
        Texture2D floorTex;

        public LightsManager lightsManager;
        RenderTarget2D colorTarget;
        RenderTarget2D normalTarget;
        RenderTarget2D positionTarget;
        RenderTarget2D lightTarget;
        public AnimationManager animationManager;
        public Player localPlayer;
        public JObject CFG;
        public GState gameState;
        public InputManager inputManager;
        InputManager inputRun;
        InputManager inputMainMenu;
        Mutex updateMutex;
        public Mutex playerCacheMutex = new Mutex(false, "player-data-cache");

        public Hud hud;
        public NixFPS()
        {
            updateMutex = new Mutex();
            CFG = JObject.Parse(File.ReadAllText("app-settings.json"));
            game = this;
            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            screenWidth = CFG["ScreenWidth"].Value<int>();
            screenHeight = CFG["ScreenHeight"].Value<int>();
            Graphics.PreferredBackBufferWidth = screenWidth;
            Graphics.PreferredBackBufferHeight = screenHeight;
            Window.IsBorderless = CFG["Borderless"].Value<bool>();
            Graphics.IsFullScreen = CFG["Fullscreen"].Value<bool>();
            Window.Position = new Point(320, 180);
            screenCenter = new Point(screenWidth / 2 + Window.Position.X, screenHeight / 2 + Window.Position.Y);

            var framerateLimit = CFG["FramerateLimit"].Value<int>();
            IsFixedTimeStep = framerateLimit != 0;
            if(framerateLimit > 0)
                TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0/framerateLimit);
            Graphics.SynchronizeWithVerticalRetrace = CFG["VSync"].Value<bool>();

            Graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            
            
            
            if (!CFG.ContainsKey("ClientID"))
            {
                var ri = new Random().NextInt64();
                CFG["ClientID"] = (uint) ri;

                File.WriteAllText("app-settings.json", CFG.ToString());
            }



            Exiting += (s, e) => {
                NetworkManager.Client.Disconnect();
                NetworkManager.StopNetThread();
            };
        }
        public static NixFPS GameInstance() { return game; } 
        
        public static void AssignEffectToModel(Model m, Effect e)
        {
            foreach (var mesh in m.Meshes)
                foreach (var part in mesh.MeshParts)
                    part.Effect = e;
        }
        protected override void Initialize()
        {
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

            inputMainMenu = new InputMainMenu();
            inputRun = new InputGameRun();


            NetworkManager.Connect();
            localPlayer = NetworkManager.localPlayer;
            InputManager.Init();
            SwitchGameState(GState.MAIN_MENU);

            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            basicModelEffect = new BasicModelEffect("basic");
            deferredEffect = new DeferredEffect("deferred");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            fullScreenQuad = new FullScreenQuad(GraphicsDevice);
            plane = Content.Load<Model>(ContentFolder3D + "basic/plane");
            cube = Content.Load<Model>(ContentFolder3D + "basic/cube");
            floorTex = Content.Load<Texture2D>(ContentFolder3D + "basic/tex/metalfloor");
            font = Content.Load<SpriteFont>(ContentFolderFonts + "tahoma/15");

            LightVolume.Init();

            AssignEffectToModel(plane, basicModelEffect.effect);


            lightsManager = new LightsManager();
            lightsManager.ambientLight = new AmbientLight(new Vector3(20, 50, 20), new Vector3(1f, 1f, 1f), Vector3.One, Vector3.One);


            var mrt = Matrix.CreateScale(0.025f) * Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(0, 0f, 0);
            var mrt2 = Matrix.CreateScale(0.025f) * Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(10, 0f, 0);

            animationManager = new AnimationManager();
            //animationManager.SetPlayerData(0, mrt, "idle");
            //animationManager.SetPlayerData(1, mrt2, "idle");

            //Random r = new Random();
            //uint p = 0;
            //var count = 4;
            //var countMax = 10 * count;
            //for (int x = 0; x < countMax; x += 10)
            //{
            //    for (int z = 0; z < countMax; z += 10)
            //    {
            //        var mx = Matrix.CreateScale(0.025f) * Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(x, 0f, z);
            //        //int index = r.Next(11);
            //        animationManager.SetPlayerData(p, mx, "run forward");
            //        p++;
            //    }
            //}

            // Create many point lights
            GeneratePointLights();

            // Create the render targets we are going to use
            SetupRenderTargets();

            camera = new Camera(Graphics.GraphicsDevice.Viewport.AspectRatio);
            skybox = new Skybox();
            InputManager.camera = camera;

            hud = new Hud();

            mainStopwatch.Start();

            


            
        }
        double time = 0;
        float deltaTimeU;
        float onesec = 0f;
        int packetsIn;
        int packetsOut;
        public Stopwatch mainStopwatch = new Stopwatch();
        TimeSpan timespan = TimeSpan.Zero;
        protected override void Update(GameTime gameTime)
        {
            deltaTimeU = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //time += gameTime.ElapsedGameTime.TotalMilliseconds;
            timespan = timespan.Add(gameTime.ElapsedGameTime);
            NetworkManager.InterpolatePlayers(mainStopwatch.ElapsedMilliseconds);
            inputManager.Update(deltaTimeU);

            //camera.Update(inputManager);
            animationManager.Update(gameTime);
            UpdatePointLights(deltaTimeU);

            lightsManager.Update(deltaTimeU);

            hud.Update(deltaTimeU);
            //TPS = 200
            //while (time >= 0.005)
            //{
            //    time -= 0.005;
            //    NetworkManager.Client.Update();
            //    NetworkManager.SendData();
            //}
            //TPS = min(200,FPS)

            //updateMutex.WaitOne();

            //NetworkManager.Client.Update();
            //if (timespan.CompareTo(TimeSpan.FromMilliseconds(5)) > 0)
            //{
            //    timespan = TimeSpan.Zero;
            //    NetworkManager.SendData();
            //}
            //updateMutex.ReleaseMutex();

            onesec += deltaTimeU;
            if (onesec >= 1)
            {
                onesec = 0f;
                packetsIn = NetworkManager.Client.Connection.Metrics.UnreliableIn;
                packetsOut = NetworkManager.Client.Connection.Metrics.UnreliableOut;
                NetworkManager.Client.Connection.Metrics.Reset();
            }
            //NetworkManager.SendData();
            base.Update(gameTime);
        }

        float deltaTimeD;
        double frameTime;
        int fps;
        int updatefps;
        float timeD;
        bool debugRTs = false;
        protected override void Draw(GameTime gameTime)
        {
            //TODO: custom basic and skinning effect, decide deferred or not 
            // skybox (fix
            deltaTimeD = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeD += deltaTimeD;
            timeD %= 0.12f;
            if (timeD <= .025f)
            {
                fps = (int)(1 / deltaTimeD);
                updatefps = (int)(1 / deltaTimeU);
                frameTime = deltaTimeD * 1000;
            }

            switch (gameState)
            {
                case GState.MAIN_MENU: DrawMenu(deltaTimeD); break;
                case GState.RUN: DrawRun(deltaTimeD); break;
            }
            

            base.Draw(gameTime);
        }

        void DrawMenu(float deltaTime)
        {
            hud.DrawMenu(deltaTime);
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            var str = "MAIN MENU, enter to start";
            spriteBatch.DrawString(font, str , new Vector2(screenWidth/2 - (font.MeasureString(str).X) /2 , screenHeight/2), Color.White);
            spriteBatch.End();
        }
        void DrawRun(float deltaTime)
        {
            basicModelEffect.SetView(camera.view);
            basicModelEffect.SetProjection(camera.projection);
            deferredEffect.SetView(camera.view);
            deferredEffect.SetProjection(camera.projection);
            deferredEffect.SetCameraPosition(camera.position);
            /// Target 1 (colorTarget) RGB = color, A = KD
            /// Target 2 (normalTarget) RGB = normal(scaled), A = KS
            /// Target 3 (positionTarget) RGB = world position, A = shininess(scale if necessary)
            /// Target 4 (not in use, but could be used) 
            GraphicsDevice.SetRenderTargets(colorTarget, normalTarget, positionTarget);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            skybox.Draw();
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw a simple plane, enable lighting on it
            DrawPlane();
            
            animationManager.DrawPlayers();

            // Draw the geometry of the lights in the scene, so that we can see where the generators are
            lightsManager.DrawLightGeo();

            /// Now we calculate the lights. first we start by sending the targets from before as textures
            /// First, we use a fullscreen quad to calculate the ambient light, as a baseline (optional)
            /// Then, we iterate our point lights and render them as spheres in the correct position. 
            /// This will launch pixel shader functions only for the necessary pixels in range of that light.
            /// From the G-Buffer we sample the required information for that pixel, and we compute the color
            /// BlendState should be additive, to correctly sum up the contributions of multiple lights in
            /// the same pixel.
            /// For pixels that shouldnt be lit, for example the light geometry, normals are set to rgb = 0
            /// and we can use that to simply output white in our lightTarget for that pixel.
            GraphicsDevice.SetRenderTargets(lightTarget);
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            deferredEffect.SetColorMap(colorTarget);
            deferredEffect.SetNormalMap(normalTarget);
            deferredEffect.SetPositionMap(positionTarget);

            lightsManager.Draw();

            /// Finally, we have our color texture we calculated in step one, and the lights from step two
            /// we combine them here by simply multiplying them, finalColor = color * light, 
            /// using a final fullscreen quad pass.
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            deferredEffect.SetLightMap(lightTarget);
            deferredEffect.SetScreenSize(new Vector2(screenWidth, screenHeight));
            deferredEffect.SetTech("integrate");

            fullScreenQuad.Draw(deferredEffect.effect);

            var lightCount = lightsManager.lightsToDraw.Count;
            var rec = new Rectangle(0, 0, screenWidth, screenHeight);

            /// In this example, by hitting key 0 you can see the targets in the corners of the screen
            if (debugRTs)
            {
                spriteBatch.Begin(blendState: BlendState.Opaque);

                spriteBatch.Draw(colorTarget, Vector2.Zero, rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                spriteBatch.Draw(normalTarget, new Vector2(0, screenHeight - screenHeight / 4), rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                spriteBatch.Draw(positionTarget, new Vector2(screenWidth - screenWidth / 4, 0), rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
                spriteBatch.Draw(lightTarget, new Vector2(screenWidth - screenWidth / 4, screenHeight - screenHeight / 4), rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);

                spriteBatch.End();
            }

            string ft = (frameTime * 1000).ToString("0,####");
            string fpsStr = "FPS " + fps +"|"+updatefps;
            var pc = " PC " + NetworkManager.players.Count;
            var p = NetworkManager.localPlayer;
            var camStr = string.Format("({0:F2}, {1:F2}, {2:F2})", p.position.X, p.position.Y, p.position.Z);
            var pos = " camlocal " + camStr;
            var pNetStr = "";
            var pos2 = "";
            if (NetworkManager.players.Count > 0)
            {
                var player2 = NetworkManager.players[0];
                var pNet = player2.position;

                //pNetStr += string.Format("({0:F2}, {1:F2}, {2:F2})", pNet.X, pNet.Y, pNet.Z);

                //pos2 = " player2 " + pNetStr;
                pos2 = " player2Cache " + player2.netDataCache.Count + " ";
            }

            fpsStr += pos + pos2 + NetworkManager.Client.RTT + " ms, in " + packetsIn;
            fpsStr += " out " + packetsOut;

            fpsStr += " diff " + string.Format("({0:F2}, {1:F2}, {2:F2})", NetworkManager.posDiff.X, NetworkManager.posDiff.Y, NetworkManager.posDiff.Z);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, fpsStr, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(font, str, new Vector2(screenWidth - font.MeasureString(str).X, 0), Color.White);
            spriteBatch.End();

            hud.DrawRun(deltaTimeD);


        }

        private void DrawPlane()
        {
            basicModelEffect.SetTech("colorTex_lightEn");
            basicModelEffect.SetTiling(Vector2.One * 500);
            foreach (var mesh in plane.Meshes)
            {
                var w = mesh.ParentBone.Transform * Matrix.CreateScale(10f) * Matrix.CreateTranslation(0, 0, 0);
                basicModelEffect.SetColor(Color.DarkGray.ToVector3());
                basicModelEffect.SetWorld(w);
                basicModelEffect.SetKA(0.3f);
                basicModelEffect.SetKD(0.8f);
                basicModelEffect.SetKS(0.8f);
                basicModelEffect.SetShininess(30f);
                basicModelEffect.SetColorTexture(floorTex);
                basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));

                mesh.Draw();
            }
            basicModelEffect.SetTiling(Vector2.One);
        }

        void SetupRenderTargets()
        {
            colorTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            normalTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            positionTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            lightTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
        }
        float timeL = 0f;
        int maxPointLights = 1;
        void UpdatePointLights(float deltaTime)
        {
            timeL += deltaTime * .5f;
            for (int i = 0; i < maxPointLights; i++)
            {
                if(i%2 == 0)
                    lights[i].position = new Vector3(MathF.Sin(timeL + offsetT[i]) * offsetR[i], 4, MathF.Cos(timeL + offsetT[i]) * offsetR[i]);
                else
                    lights[i].position = new Vector3(MathF.Sin(-timeL + offsetT[i]) * offsetR[i], 4, MathF.Cos(-timeL + offsetT[i]) * offsetR[i]);
            }
        }

        List<int> offsetR = new List<int>();
        List<float> offsetT = new List<float>();
        List<LightVolume> lights = new List<LightVolume>();
        void GeneratePointLights()
        {
            var random = new Random();
            var light = new PointLight(Vector3.Zero, 15f, new Vector3(1,0,0), new Vector3(1, 0, 0));
            lightsManager.register(light);
            lights.Add(light);
            light.hasLightGeo = true;
            offsetR.Add(5);
            offsetT.Add(0);

            //for (int i = 1; i < maxPointLights; i++)
            //{
            //    offsetR.Add((int)random.NextInt64(-300, 300));
            //    offsetT.Add((float)random.NextDouble() * 50);

            //    var randomColor = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            //    while (randomColor == Vector3.Zero || randomColor == Vector3.One)
            //    {
            //        randomColor = new Vector3(random.NextInt64(0, 2), random.NextInt64(0, 2), random.NextInt64(0, 2));
            //    }
            //    light = new PointLight(Vector3.Zero, 15f, randomColor, randomColor);
            //    lightsManager.register(light);
            //    lights.Add(light);
            //    light.hasLightGeo = true;
            //}

        }

        public void SwitchGameState(GState state)
        {
            gameState = state;
            switch(gameState)
            {
                case GState.MAIN_MENU:
                    inputManager = inputMainMenu;
                    IsMouseVisible = true;
                    break;
                case GState.RUN:
                    System.Windows.Forms.Cursor.Position = inputManager.center;
                    camera.pitch = 0;
                    inputManager = inputRun;
                    IsMouseVisible = CFG["MouseVisible"].Value<bool>();
                    break;
                case GState.PAUSE:
                    break;
                case GState.OPTIONS:
                    break;
            }

        }

        public enum GState
        {
            MAIN_MENU,
            RUN,
            PAUSE,
            OPTIONS
        }

    }
}

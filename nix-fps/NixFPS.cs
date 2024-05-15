using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nixfps.Components.Cameras;
using nixfps.Components.Effects;
using nixfps.Components.Lights;
using nixfps.Components.Network;
using nixfps.Components.Skybox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.IO;
using Riptide;

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

        public LightsManager lightsManager;
        RenderTarget2D colorTarget;
        RenderTarget2D normalTarget;
        RenderTarget2D positionTarget;
        RenderTarget2D lightTarget;
        public AnimationManager animationManager;

        public JObject CFG;
        public NixFPS()
        {
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
            Window.Position = new Point(0, 0);
            var framerateLimit = CFG["FramerateLimit"].Value<int>();
            IsFixedTimeStep = framerateLimit != 0;
            if(framerateLimit > 0)
                TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0/framerateLimit);
            Graphics.SynchronizeWithVerticalRetrace = CFG["VSync"].Value<bool>();

            Graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            
            
            if (!CFG.ContainsKey("ClientID"))
            {
                var ri = new Random().NextInt64();
                CFG["ClientID"] = (uint) ri;

                File.WriteAllText("app-settings.json", CFG.ToString());
            }



            Exiting += (s, e) => NetworkManager.Client.Disconnect();
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
            
            NetworkManager.Connect();
            screenCenter = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

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

            
        }
        double time = 0;
        float deltaTimeU;
        List<Keys> ignored = new List<Keys>(); 
        float onesec = 0f;
        int packetsIn;
        int packetsOut;

        protected override void Update(GameTime gameTime)
        {
            deltaTimeU = (float)gameTime.ElapsedGameTime.TotalSeconds;
            time += gameTime.ElapsedGameTime.TotalSeconds;
            // TODO: input manager(s)
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyState = Keyboard.GetState();
            if(keyState.IsKeyDown(Keys.W))
            {
                camera.position += camera.frontDirection * 5 * deltaTimeU;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                camera.position -= camera.frontDirection * 5 * deltaTimeU;
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                camera.position -= camera.rightDirection * 5 * deltaTimeU;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                camera.position += camera.rightDirection * 5 * deltaTimeU;
            }
            if (keyState.IsKeyDown(Keys.Space))
            {
                camera.position.Y += 5 * deltaTimeU;
            }
            if (keyState.IsKeyDown(Keys.LeftControl))
            {
                camera.position.Y -= 5 * deltaTimeU;
            }
            var dz = (keyState.IsKeyDown(Keys.Up) ? 1 : 0) - (keyState.IsKeyDown(Keys.Down) ? 1 : 0);
            var dx = (keyState.IsKeyDown(Keys.Right) ? 1 : 0) - (keyState.IsKeyDown(Keys.Left) ? 1 : 0);

            if(dz > 0 && dx == 0)
            {
                if(!keyState.IsKeyDown(Keys.LeftShift))
                    animationManager.SetPlayerData(NetworkManager.localPlayerId,"run forward");
                else
                    animationManager.SetPlayerData(NetworkManager.localPlayerId, "sprint forward");
            }
            else if(dz > 0 && dx > 0)
            {
                if (!keyState.IsKeyDown(Keys.LeftShift))
                    animationManager.SetPlayerData(NetworkManager.localPlayerId, "run forward right");
                else
                    animationManager.SetPlayerData(NetworkManager.localPlayerId, "sprint forward right");
            }
            else if (dz > 0 && dx < 0)
            {
                if (!keyState.IsKeyDown(Keys.LeftShift))
                    animationManager.SetPlayerData(NetworkManager.localPlayerId, "run forward left");
                else
                    animationManager.SetPlayerData(NetworkManager.localPlayerId, "sprint forward left");
            }
            else if (dz < 0 && dx == 0)
            {
                animationManager.SetPlayerData(NetworkManager.localPlayerId, "run backward");
            }
            else if (dz < 0 && dx > 0)
            {
                animationManager.SetPlayerData(NetworkManager.localPlayerId, "run backward right");
            }
            else if (dz < 0 && dx < 0)
            {
                animationManager.SetPlayerData(NetworkManager.localPlayerId, "run backward left");
            }
            else if (dz == 0 && dx > 0)
            {
                animationManager.SetPlayerData(NetworkManager.localPlayerId, "run right");
            }
            else if (dz == 0 && dx < 0)
            {
                animationManager.SetPlayerData(NetworkManager.localPlayerId, "run left");
            }

            else
                animationManager.SetPlayerData(NetworkManager.localPlayerId, "idle");

            
            ignored.RemoveAll(key => keyState.IsKeyUp(key));

            camera.Update(deltaTimeU);
            animationManager.Update(deltaTimeU);
            UpdatePointLights(deltaTimeU);

            lightsManager.Update(deltaTimeU);


            //TPS = 200
            while (time >= 0.005)
            {
                time -= 0.005;
                NetworkManager.Client.Update();
                NetworkManager.SendData();
            }
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
                frameTime = deltaTimeD * 1000;
            }
            

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
            string fpsStr = "FPS " + fps;
            var pc = " PC " + NetworkManager.players.Count;
            var camStr = string.Format("({0:F2}, {1:F2}, {2:F2})", camera.position.X, camera.position.Y, camera.position.Z);
            var pos = " camlocal " + camStr;
            var pNetStr = "";
            var pos2 = "";
            if (NetworkManager.players.Count > 1 )
            {
                var pNet = NetworkManager.GetPlayerFromId(222222).position;
                pNetStr += string.Format("({0:F2}, {1:F2}, {2:F2})", pNet.X, pNet.Y, pNet.Z);
                pos2 = " player2 " + pNetStr;
            }
            
            fpsStr += pc + pos + pos2 + " "+NetworkManager.Client.RTT+" ms, in "+ packetsIn;
            fpsStr += " out " + packetsOut;

            spriteBatch.Begin();
            spriteBatch.DrawString(font, fpsStr, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(font, str, new Vector2(screenWidth - font.MeasureString(str).X, 0), Color.White);
            spriteBatch.End();




            base.Draw(gameTime);
        }

        private void DrawPlane()
        {
            basicModelEffect.SetTech("basic_color");

            foreach (var mesh in plane.Meshes)
            {
                var w = mesh.ParentBone.Transform * Matrix.CreateScale(10f) * Matrix.CreateTranslation(0, 0, 0);
                basicModelEffect.SetColor(Color.DarkGray.ToVector3());
                basicModelEffect.SetWorld(w);
                basicModelEffect.SetKA(0.3f);
                basicModelEffect.SetKD(0.8f);
                basicModelEffect.SetKS(0.8f);
                basicModelEffect.SetShininess(30f);

                basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));

                mesh.Draw();
            }

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

    }
}

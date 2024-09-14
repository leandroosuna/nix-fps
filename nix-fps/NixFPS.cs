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
using nixfps.Components.Gun;
using nixfps.Components.Gizmos;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using nixfps.Components.Collisions;
using nixfps.Components.GUI;



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
        public BoundingBox boundingBox;
        Model aztec;
        Texture2D[] aztecTex;
        Texture2D[] numTex;

        Texture2D floorTex;

        public LightsManager lightsManager;
        RenderTarget2D colorTarget;
        RenderTarget2D normalTarget;
        RenderTarget2D positionTarget;
        RenderTarget2D bloomFilterTarget;
        RenderTarget2D blurHTarget;
        RenderTarget2D blurVTarget;
        RenderTarget2D lightTarget;

        public AnimationManager animationManager;
        public Player localPlayer;
        public JObject CFG;
        public GState gameState;
        public InputManager inputManager;
        InputManager inputRun;
        InputManager inputMainMenu;
        GunManager gunManager;
        Mutex updateMutex;
        public Mutex playerCacheMutex = new Mutex(false, "player-data-cache");

        public Hud hud;
        public Gizmos gizmos;
        public int selectedVertexIndex = 3000;

        public GUI GUI;
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
            gizmos = new Gizmos();

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
        Texture2D boxTex;
        protected override void LoadContent()
        {
            GUI.Init();
            gizmos.LoadContent(GraphicsDevice);
            basicModelEffect = new BasicModelEffect("basic");
            deferredEffect = new DeferredEffect("deferred");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            fullScreenQuad = new FullScreenQuad(GraphicsDevice);
            plane = Content.Load<Model>(ContentFolder3D + "basic/plane");
            cube = Content.Load<Model>(ContentFolder3D + "basic/cube");
            floorTex = Content.Load<Texture2D>(ContentFolder3D + "basic/tex/metalfloor");
            font = Content.Load<SpriteFont>(ContentFolderFonts + "tahoma/15");
            boxTex = Content.Load<Texture2D>(ContentFolder3D + "basic/tex/wood");

            aztec = Content.Load<Model>(ContentFolder3D + "aztec/de_aztec");
            //Content.Load<Texture2D>(ContentFolder3D + "aztec/de_aztec_texture_0"),
            BuildMapCollider();
            aztecTex = new Texture2D[61];
            for(int i = 0; i < 61; i++)
            {
                aztecTex[i] = Content.Load<Texture2D>(ContentFolder3D + "aztec/de_aztec_texture_" + i);
            }
            numTex = new Texture2D[101];
            for (int i = 0; i < 101; i++)
            {
                numTex[i] = Content.Load<Texture2D>(ContentFolder3D + "basic/tex/num/" + i);
            }
            LightVolume.Init();

            AssignEffectToModel(plane, basicModelEffect.effect);
            AssignEffectToModel(aztec, basicModelEffect.effect);


            lightsManager = new LightsManager();
            lightsManager.ambientLight = new AmbientLight(new Vector3(20, 50, 20), new Vector3(1f, 1f, 1f), Vector3.One, Vector3.One);

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
                    //var mx = Matrix.CreateScale(0.025f) * Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(x, 0f, z);
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

            GUI.AddControllers();
            GUI.Bind();
            hud = new Hud();
            gunManager = new GunManager();
            mainStopwatch.Start();
            
        }

        

        double time = 0;
        float deltaTimeU;
        float onesec = 0f;
        int packetsIn;
        int packetsOut;
        public Stopwatch mainStopwatch = new Stopwatch();
        TimeSpan timespan = TimeSpan.Zero;
        Matrix gunWorld;
        protected override void Update(GameTime gameTime)
        {
            deltaTimeU = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //time += gameTime.ElapsedGameTime.TotalMilliseconds;
            timespan = timespan.Add(gameTime.ElapsedGameTime);
            if (gameState == GState.MAIN_MENU)
            {
                camera.RotateBy(new Vector2(deltaTimeU, 0));
            }
            else if (gameState == GState.RUN)
            {

                NetworkManager.InterpolatePlayers(mainStopwatch.ElapsedMilliseconds);
                inputManager.Update(deltaTimeU);

                gizmos.UpdateViewProjection(camera.view, camera.projection);

                //camera.Update(inputManager);
                animationManager.Update(gameTime);
                UpdatePointLights(deltaTimeU);

                lightsManager.Update(deltaTimeU);
                gunManager.Update(gameTime);
                hud.Update(deltaTimeU);

                DistToGround();

                //camera.position.Y -= deltaTimeU * 4;
                var keyState = Keyboard.GetState();
            
                var forward = keyState.IsKeyDown(Keys.I);
                var backward = keyState.IsKeyDown(Keys.K);
                var left = keyState.IsKeyDown(Keys.J);
                var right= keyState.IsKeyDown(Keys.L);
                var sprint = keyState.IsKeyDown(Keys.U);

                var dz = (forward? 1 : 0) - (backward? 1: 0);
                var dx = (right? 1 : 0) - (left? 1 : 0);
                byte clipId = 0;
                if (dz > 0 && dx == 0)
                {
                    if (sprint)
                        clipId = (byte)PlayerAnimation.sprintForward;
                    else
                        clipId = (byte)PlayerAnimation.runForward;
                }
                else if (dz > 0 && dx > 0)
                {
                    if (sprint)
                        clipId = (byte)PlayerAnimation.sprintForwardRight;
                    else
                        clipId = (byte)PlayerAnimation.runForwardRight;
                }
                else if (dz > 0 && dx < 0)
                {
                    if (sprint)
                        clipId = (byte)PlayerAnimation.sprintForwardLeft;
                    else
                        clipId = (byte)PlayerAnimation.runForwardLeft;
                }
                else if (dz < 0 && dx == 0)
                {
                    clipId = (byte)PlayerAnimation.runBackward;
                }
                else if (dz < 0 && dx > 0)
                {
                    clipId = (byte)PlayerAnimation.runBackwardRight;
                }
                else if (dz < 0 && dx < 0)
                {
                    clipId = (byte)PlayerAnimation.runBackwardLeft;
                }
                else if (dz == 0 && dx > 0)
                {
                    clipId = (byte)PlayerAnimation.runRight;
                }
                else if (dz == 0 && dx < 0)
                {
                    clipId = (byte)PlayerAnimation.runLeft;
                }
                else
                    clipId = (byte)PlayerAnimation.idle;

                animationManager.SetClipName(localPlayer, clipId);

                onesec += deltaTimeU;
                if (onesec >= 1)
                {
                    onesec = 0f;
                    packetsIn = NetworkManager.Client.Connection.Metrics.UnreliableIn;
                    packetsOut = NetworkManager.Client.Connection.Metrics.UnreliableOut;
                    NetworkManager.Client.Connection.Metrics.Reset();
                }
            
            }
                


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
                case GState.MAIN_MENU: DrawMenu(deltaTimeD); GUI.Draw(gameTime); break;
                case GState.RUN: DrawRun(deltaTimeD, gameTime); break;
            }
            

            base.Draw(gameTime);
        }

        void DrawMenu(float deltaTime)
        {
            hud.DrawMenu(deltaTime);
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            skybox.Draw();
        }
        void DrawRun(float deltaTime, GameTime gt)
        {
            basicModelEffect.SetView(camera.view);
            basicModelEffect.SetProjection(camera.projection);
            deferredEffect.SetView(camera.view);
            deferredEffect.SetProjection(camera.projection);
            deferredEffect.SetCameraPosition(camera.position);
            /// Target 1 (colorTarget) RGB = color, A = KD
            /// Target 2 (normalTarget) RGB = normal(scaled), A = KS
            /// Target 3 (positionTarget) RGB = world position, A = shininess(scale if necessary)
            /// Target 4 (bloomTarget) RGB = filter, A = (not in use) 
            GraphicsDevice.SetRenderTargets(colorTarget, normalTarget, positionTarget, bloomFilterTarget);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            skybox.Draw();
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw a simple plane, enable lighting on it
            //DrawPlane();
            DrawAztec();
            //DrawBox();

            localPlayer.UpdateColliders();

            //gizmos.DrawSphere(localPlayer.zoneCollider.Center, new Vector3(localPlayer.zoneCollider.Radius), Color.White);
            //gizmos.DrawCylinder(localPlayer.headCollider.Center, localPlayer.headCollider.Rotation, 
            //    new Vector3(localPlayer.headCollider.Radius, localPlayer.headCollider.HalfHeight, localPlayer.headCollider.Radius), Color.Red);
            //gizmos.DrawCylinder(localPlayer.bodyCollider.Center, localPlayer.bodyCollider.Rotation, 
            //    new Vector3(localPlayer.bodyCollider.Radius, localPlayer.bodyCollider.HalfHeight, localPlayer.bodyCollider.Radius), Color.Green);
            //gizmos.Draw();
            gunManager.DrawGun(deltaTimeD);
            
            
            animationManager.DrawPlayers();
            gizmos.Draw();
            
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
            GraphicsDevice.SetRenderTargets(lightTarget, blurHTarget, blurVTarget);
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            deferredEffect.SetColorMap(colorTarget);
            deferredEffect.SetNormalMap(normalTarget);
            deferredEffect.SetPositionMap(positionTarget);
            deferredEffect.SetBloomFilter(bloomFilterTarget);
            lightsManager.Draw();

            /// Finally, we have our color texture we calculated in step one, and the lights from step two
            /// we combine them here by simply multiplying them, finalColor = color * light, 
            /// using a final fullscreen quad pass.
            GraphicsDevice.SetRenderTarget(null);
            //GUI.Draw(gt);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            deferredEffect.SetLightMap(lightTarget);
            deferredEffect.SetScreenSize(new Vector2(screenWidth, screenHeight));
            deferredEffect.SetTech("integrate");
            deferredEffect.SetBlurH(blurHTarget);
            deferredEffect.SetBlurV(blurVTarget);

            fullScreenQuad.Draw(deferredEffect.effect);
            
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
            //fpsStr += "MP " + meshPartDrawCount;
            //fpsStr += lightsManager.lights.Count + " " + lightsManager.lightsToDraw.Count;
            fpsStr += "selected " + selectedVertexIndex;
            spriteBatch.Begin();
            spriteBatch.DrawString(font, fpsStr, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(font, str, new Vector2(screenWidth - font.MeasureString(str).X, 0), Color.White);
            spriteBatch.End();

            //hud.DrawRun(deltaTimeD);


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

        private void DrawBox()
        {
            basicModelEffect.SetTech("colorTex_lightEn");
            basicModelEffect.SetTiling(new Vector2(4.5f, 4.5f));
            foreach (var mesh in cube.Meshes)
            {
                var w = mesh.ParentBone.Transform * Matrix.CreateScale(.01f) * Matrix.CreateTranslation(4, 2, 4);
                basicModelEffect.SetWorld(w);
                basicModelEffect.SetKA(0.3f);
                basicModelEffect.SetKD(0.8f);
                basicModelEffect.SetKS(0.8f);
                basicModelEffect.SetShininess(30f);
                basicModelEffect.SetColorTexture(boxTex);
                basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));

                mesh.Draw();
            }
            basicModelEffect.SetTiling(Vector2.One);

            var min = new Vector3(4, 2, 4) - new Vector3(1, 1f, 1);
            var max = new Vector3(4, 2, 4) + new Vector3(1, 1f, 1);

            boundingBox = new BoundingBox(min, max);
            //gizmos.DrawCube(Matrix.CreateScale(2f) * , Color.Magenta);
            gizmos.DrawCube(new Vector3(4, 2, 4), new Vector3(2, 2, 2), Color.Magenta);
           
        }
        int meshPartDrawCount = 0;



        void DrawAztec()
        {
            meshPartDrawCount = 0;
            //basicModelEffect.SetTech("colorTex_lightEn");
            //basicModelEffect.SetTech("basic_color");
            basicModelEffect.SetTech("number");
            basicModelEffect.SetTiling(new Vector2(2f));
            basicModelEffect.SetKA(.3f);
            basicModelEffect.SetKD(.8f);
            basicModelEffect.SetKS(.8f);
            basicModelEffect.SetShininess(10f);

            foreach (var mesh in aztec.Meshes)
            {
                meshPartDrawCount = 0;


                var w = mesh.ParentBone.Transform * Matrix.CreateScale(1f);
                basicModelEffect.SetWorld(w);

                basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                
                for(int partindex = 0; partindex < mesh.MeshParts.Count; partindex++)
                {
                    //if (camera.FrustumContains(aztecMPColliders[partindex]))
                    //    continue;
                    meshPartDrawCount++;
                    var (found, tex) = AztecTexFor(partindex);
                    basicModelEffect.SetColorTexture(tex);
                    basicModelEffect.SetColor(Color.White.ToVector3());
                    basicModelEffect.SetTiling(new Vector2(1f));
                    if (!found)
                    {
                        basicModelEffect.SetTiling(new Vector2(2f));
                        if (partindex < 100)
                        {
                            basicModelEffect.SetColor(Color.Red.ToVector3());
                        }
                        else if (partindex < 200)
                        {
                            basicModelEffect.SetColor(Color.Green.ToVector3());
                        }
                        else
                        {
                            basicModelEffect.SetColor(Color.Blue.ToVector3());
                        }
                    }


                    var part = mesh.MeshParts[partindex];
                    
                    foreach (var pass in basicModelEffect.effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        game.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        game.GraphicsDevice.Indices = part.IndexBuffer;
                        game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }

                //mesh.Draw();
            }
        }
        Vector3 ColorFor(int index)
        {
            if (index <= 71)
            {
                byte R = (byte)(index * 255 / 71);
                return new Color(R, 0, 0).ToVector3();
            }
            else if (index <= 143)
            {
                byte G = (byte)((index - 72) * 255 / 71);
                return new Color(0, G, 0).ToVector3();
            }
            else
            {
                byte B = (byte)((index - 144) * 255 / 71);
                return new Color(0, 0, B).ToVector3();
            }
            
        }

        public (bool,Texture2D) AztecTexFor(int index)
        {
            switch (index)
            {
                case 0: return (true, aztecTex[0]);
                case 1: return (true, aztecTex[22]);
                case 2: return (true, aztecTex[3]);
                case 3: return (true, aztecTex[3]);

                case 4: return (true, aztecTex[52]);
                case 5: return (true, aztecTex[53]);
                case 6: return (true, aztecTex[49]);
                case 10: return (true, aztecTex[10]);
                case 17: return (true, aztecTex[22]);

                case 26: return (true, aztecTex[31]);
                case 27: return (true, aztecTex[32]);
    
                case 30: return (true, aztecTex[35]);
                case 46: return (true, aztecTex[46]);
                case 200: return (true, aztecTex[60]);
                case 201: return (true, aztecTex[58]);

                case 214: return (true, aztecTex[35]);


                default:
                    return (false,numTex[index % 100]);
            }
        }
        void SetupRenderTargets()
        {
            colorTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            normalTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            positionTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            lightTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            bloomFilterTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            blurHTarget= new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            blurVTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);

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
            lightsManager.Register(light);
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
        class CollisionTriangle
        {
            public Vector3[] v;
            public CollisionTriangle()
            {
                v = new Vector3[3];
            }
        }
        bool CheckTriangle(CollisionTriangle t)
        {
            return
                Vector3.DistanceSquared(t.v[0], camera.position) < 30f;
        }
        List<PointLight> minilights = new List<PointLight>();

        static Vector3 CalculateBarycentricCoordinates(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 p)
        {
            Vector3 v2v1 = v2 - v1;
            Vector3 v3v1 = v3 - v1;
            Vector3 v3p = p - v3;

            float d00 = Vector3.Dot(v2v1, v2v1);
            float d01 = Vector3.Dot(v2v1, v3v1);
            float d11 = Vector3.Dot(v3v1, v3v1);
            float d20 = Vector3.Dot(v3p, v2v1);
            float d21 = Vector3.Dot(v3p, v3v1);

            float invDenom = 1.0f / (d00 * d11 - d01 * d01);
            float u = (d11 * d20 - d01 * d21) * invDenom;
            float v = (d00 * d21 - d01 * d20) * invDenom;
            float w = 1.0f - u - v;

            return new Vector3(u, v, w);
        }
        static float InterpolateY(Vector3 barycentric, float y1, float y2, float y3)
        {
            return barycentric.X * y1 + barycentric.Y * y2 + barycentric.Z * y3;
        }
        public int hitCount = 0;
        public void DistToGround()
        {
            var closeEnough = mapTriangles.FindAll(t => CheckTriangle(t));
            hitCount = 0;
            Ray rayDown = new Ray(camera.position, Vector3.Down);
            //Ray rayFront = new Ray(camera.position, camera.frontDirection);

            foreach (var l in minilights)
                lightsManager.Destroy(l);
            minilights.Clear();
            
            foreach (var collisionTriangle in closeEnough)
            {

                bool hit = BoundingVolumesExtensions.IntersectRayTriangle(collisionTriangle.v[0], collisionTriangle.v[1], collisionTriangle.v[2], rayDown);

                var pl = new PointLight(collisionTriangle.v[0], 2f, hit ? Color.Red.ToVector3(): Color.White.ToVector3(), Color.Magenta.ToVector3());
                pl.hasLightGeo = true;
                pl.skipDraw = true;
                minilights.Add(pl);
                pl = new PointLight(collisionTriangle.v[1], 2f, hit ? Color.Red.ToVector3() : Color.White.ToVector3(), Color.Magenta.ToVector3());
                pl.hasLightGeo = true;
                pl.skipDraw = true;
                minilights.Add(pl);
                pl = new PointLight(collisionTriangle.v[2], 2f, hit ? Color.Red.ToVector3() : Color.White.ToVector3(), Color.Magenta.ToVector3());
                pl.hasLightGeo = true;
                pl.skipDraw = true;
                minilights.Add(pl);

                if(hit)
                {
                    hitCount++;
                    //var bar = CalculateBarycentricCoordinates(collisionTriangle.v[0], collisionTriangle.v[1], collisionTriangle.v[2], camera.position);
                    //float interpolatedY = InterpolateY(bar, collisionTriangle.v[0].Y, collisionTriangle.v[1].Y, collisionTriangle.v[2].Y);
                    var avgY = (collisionTriangle.v[0].Y + collisionTriangle.v[1].Y + collisionTriangle.v[2].Y) / 3;
                    camera.position.Y = avgY + 2.5f;
                    break;
                }

            }
            //foreach (var collisionTriangle in closeEnough)
            //{

            //    bool hit = BoundingVolumesExtensions.IntersectRayTriangle(collisionTriangle.v[0], collisionTriangle.v[1], collisionTriangle.v[2], rayFront);

            //    var pl = new PointLight(collisionTriangle.v[0], 2f, hit ? Color.Green.ToVector3() : Color.White.ToVector3(), Color.Magenta.ToVector3());
            //    pl.hasLightGeo = true;
            //    pl.skipDraw = true;
            //    minilights.Add(pl);
            //    pl = new PointLight(collisionTriangle.v[1], 2f, hit ? Color.Green.ToVector3() : Color.White.ToVector3(), Color.Magenta.ToVector3());
            //    pl.hasLightGeo = true;
            //    pl.skipDraw = true;
            //    minilights.Add(pl);
            //    pl = new PointLight(collisionTriangle.v[2], 2f, hit ? Color.Green.ToVector3() : Color.White.ToVector3(), Color.Magenta.ToVector3());
            //    pl.hasLightGeo = true;
            //    pl.skipDraw = true;
            //    minilights.Add(pl);

            //    if (hit)
            //        break;

            //}

            //Debug.WriteLine("count " + c + " (" + closeEnough.Count+")");
            foreach (var l in minilights)
                lightsManager.Register(l);


        }

        bool InsideTriangle(Vector3 pos, CollisionTriangle tri)
        {
            Vector3 vec1 = Vector3.Subtract(tri.v[0], pos);
            Vector3 vec2 = Vector3.Subtract(tri.v[1], pos);

            vec1.Normalize();
            vec2.Normalize();

            double dotV = Vector3.Dot(vec1, vec2);
            double angle = (float)Math.Acos(dotV);

            vec1 = Vector3.Subtract(tri.v[1], pos);
            vec2 = Vector3.Subtract(tri.v[2], pos);

            vec1.Normalize();
            vec2.Normalize();

            dotV = Vector3.Dot(vec1, vec2);
            angle += (float)Math.Acos(dotV);

            vec1 = Vector3.Subtract(tri.v[2], pos);
            vec2 = Vector3.Subtract(tri.v[0], pos);

            vec1.Normalize();
            vec2.Normalize();

            dotV = Vector3.Dot(vec1, vec2);
            angle += (float)Math.Acos(dotV);

            float tolerance = 0.001f;

            if (angle > (Math.PI * 2) - tolerance)
                return true;

            return false;
        }
        List<CollisionTriangle> mapTriangles = new List<CollisionTriangle>();
        private void BuildMapCollider()
        {
            foreach (ModelMesh mesh in aztec.Meshes)
            {

                Matrix transform = CreateTransform(mesh.ParentBone);
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    ExtractMeshPart(meshPart, transform);
                }
                aztecMPColliders = aztecMeshPartColliders.ToArray();
                aztecMeshPartColliders.Clear();
            }
        }
        public Matrix CreateTransform(ModelBone bone)
        {
            if (bone == null)
                return Matrix.Identity;

            return bone.Transform * CreateTransform(bone.Parent);
        }
        BoundingSphere[] aztecMPColliders;
        List<BoundingSphere> aztecMeshPartColliders = new List<BoundingSphere>();
        public void ExtractMeshPart(ModelMeshPart meshPart, Matrix transform)
        {
    
            VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexElements = declaration.GetVertexElements();
            VertexElement vertexPosition = new VertexElement();

            foreach (VertexElement vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position && vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;

                    Vector3[] allVertex = new Vector3[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData(
                                    meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                                    allVertex,
                                    0,
                                    meshPart.NumVertices,
                                    declaration.VertexStride);

                    short[] indices = new short[meshPart.PrimitiveCount * 3];
                    meshPart.IndexBuffer.GetData(meshPart.StartIndex * 2, indices, 0, meshPart.PrimitiveCount * 3);

                    for (int i = 0; i != allVertex.Length; ++i)
                    {
                        Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);
                    }

                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        CollisionTriangle triangle = new CollisionTriangle();
                        triangle.v[0] = allVertex[indices[i]];
                        triangle.v[1] = allVertex[indices[i + 1]];
                        triangle.v[2] = allVertex[indices[i + 2]];

                        if(i==0)
                            aztecMeshPartColliders.Add(new BoundingSphere(allVertex[indices[i]], 25f));
                        
                        mapTriangles.Add(triangle);
                    }
                }
            }
            

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

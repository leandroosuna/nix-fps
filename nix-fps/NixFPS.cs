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
using nixfps.Components.Collisions;
using nixfps.Components.GUI;
using nixfps.Components.States;

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
        public SpriteFont fontSmall;
        public SpriteFont fontMedium;
        public SpriteFont fontLarge;

        public Skybox skybox;
        public static Texture2D Pixel { get; set; }

        //public IConfigurationRoot CFG;
        public Camera camera;
        public Point screenCenter;

        public Model plane;
        public Model cube;
        public BoundingBox boundingBox;
        public Model aztec;
        public Model dust2;
        public Texture2D[] aztecTex;
        public Texture2D[] numTex;
        public Texture2D[] dust2Tex;
        public Texture2D[] dust2NormalTex;


        public Texture2D floorTex;

        public LightsManager lightsManager;
        public RenderTarget2D colorTarget;
        public RenderTarget2D normalTarget;
        public RenderTarget2D positionTarget;
        public RenderTarget2D bloomFilterTarget;
        public RenderTarget2D blurHTarget;
        public RenderTarget2D blurVTarget;
        public RenderTarget2D lightTarget;

        public AnimationManager animationManager;
        public Player localPlayer;
        public JObject CFG;
        public string graphicsPreset;
        //public GState gameState;
        public GameState gameState;
        public GunManager gunManager;
        Mutex updateMutex;
        public Mutex playerCacheMutex = new Mutex(false, "player-data-cache");

        public Hud hud;
        public Gizmos gizmos;
        public int selectedVertexIndex = 3000;
        public float boneIndex = 0f;
        public List<LightVolume> testLights = new List<LightVolume>();
        //public GUI GUI;
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
            Window.IsBorderless = true;
            Graphics.IsFullScreen = CFG["Fullscreen"].Value<bool>();
            graphicsPreset = CFG["GraphicsPreset"].Value<string>();
            int dw = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int dh = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            Window.Position = new Point((dw - screenWidth) / 2, (dh - screenHeight) / 2);
            screenCenter = new Point(screenWidth / 2 + Window.Position.X, screenHeight / 2 + Window.Position.Y);

            //var framerateLimit = CFG["FramerateLimit"].Value<int>();
            //IsFixedTimeStep = framerateLimit != 0;
            //if(framerateLimit > 0)
            //    TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0/framerateLimit);
            IsFixedTimeStep = false;
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
                mainStopwatch.Stop();

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

            InputManager.Init();
            
            camera = new Camera(Graphics.GraphicsDevice.Viewport.AspectRatio);
            hud = new Hud();

            Gui.Init();
            GameStateManager.Init();
            //GameStateManager.SwitchTo(State.MAIN);
            GameStateManager.SwitchTo(State.RUN);

            NetworkManager.Connect();
            localPlayer = NetworkManager.localPlayer;

            base.Initialize();
        }
        public Texture2D boxTex;
        protected override void LoadContent()
        {
            gizmos.LoadContent(GraphicsDevice);
            basicModelEffect = new BasicModelEffect("basic");
            deferredEffect = new DeferredEffect("deferred");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            hud.spriteBatch = spriteBatch;
            hud.crosshair.spriteBatch = spriteBatch;
            fullScreenQuad = new FullScreenQuad(GraphicsDevice);
            plane = Content.Load<Model>(ContentFolder3D + "basic/plane");
            cube = Content.Load<Model>(ContentFolder3D + "basic/cube");
            floorTex = Content.Load<Texture2D>(ContentFolder3D + "basic/tex/metalfloor");

            fontSmall = Content.Load<SpriteFont>(ContentFolderFonts + "unispace/15");
            fontMedium = Content.Load<SpriteFont>(ContentFolderFonts + "unispace/20");
            fontLarge = Content.Load<SpriteFont>(ContentFolderFonts + "unispace/25");


            boxTex = Content.Load<Texture2D>(ContentFolder3D + "basic/tex/wood");

            aztec = Content.Load<Model>(ContentFolder3D + "aztec/de_aztec");
            dust2 = Content.Load<Model>(ContentFolder3D + "dust2/dust2");
            
            //Content.Load<Texture2D>(ContentFolder3D + "aztec/de_aztec_texture_0"),
            
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
            dust2Tex = new Texture2D[34];
            dust2NormalTex = new Texture2D[34];

            LoadDust2Tex();
            //for (int i = 0; i < 34; i++)
            //{
            //    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/de_dust2_material_" + i);
            //}

            LightVolume.Init();

            AssignEffectToModel(plane, basicModelEffect.effect);
            AssignEffectToModel(aztec, basicModelEffect.effect);
            AssignEffectToModel(dust2, basicModelEffect.effect);


            lightsManager = new LightsManager();
            lightsManager.ambientLight = new AmbientLight(new Vector3(-50, 50, 50), new Vector3(1f, 1f, 1f), Vector3.One, Vector3.One);
            animationManager = new AnimationManager();
            
            BuildMapCollider();

            // Create many point lights
            GeneratePointLights();

            // Create the render targets we are going to use
            SetupRenderTargets();

            
            skybox = new Skybox();
            InputManager.camera = camera;

            
            gunManager = new GunManager();
            mainStopwatch.Start();
            
        }

        private void LoadDust2Tex()
        {
            for (int i = 0; i < 34; i++)
            {
                if (i == 0)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/brick-wall-detail");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/brick-wall-detail-normal");
                }
                else if (i == 1 || i == 8 || i == 13 || i == 14 || i == 16 || i == 4)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/brick-wall");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/brick-wall-normal");
                }
                else if (i == 7 || i == 26 || i == 28 || i == 29 || i == 30)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/dd");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/dd-normal");
                }
                else if (i == 2 || i == 9 || i == 17 || i == 17 || i == 17 || i == 17 || i == 21 )
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/wood-box");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/wood-box-normal");
                }
                else if( i == 12 || i == 20 || i == 31)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/wood-box-side");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/wood-box-side-normal");
                }
                else if (i == 5 || i == 6 || i == 22)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/ground-sand");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/ground-sand-normal");
                }
                else if (i == 10 || i == 15 || i == 25)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/tile-floor");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/tile-floor-normal");
                }
                else if(i == 11)
                {
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/rock-wall");
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/rock-wall-normal");
                }
                else
                {
                    dust2NormalTex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/hdtex/default-normal");
                    dust2Tex[i] = Content.Load<Texture2D>(ContentFolder3D + "dust2/de_dust2_material_" + i);

                }
            }
        }

        public void StopGame()
        {
            Exit();
        }
        

        public Stopwatch mainStopwatch = new Stopwatch();
        Matrix gunWorld;
        bool firstUpdate = true;
        protected override void Update(GameTime gameTime)
        {
            
            
            gameState.Update(gameTime);

            
            base.Update(gameTime);
        }
        

        protected override void Draw(GameTime gameTime)
        {
            testLights.ForEach(l => lightsManager.Destroy(l));
            testLights.Clear();
            gameState.Draw(gameTime);
            
            base.Draw(gameTime);
        }
               
        
        public void SetupRenderTargets()
        {
            var surfaceFormat = SurfaceFormat.HalfVector4;
            var lightResMultiplier = 1f;
            
            switch (graphicsPreset)
            {
                case "ultra": surfaceFormat = SurfaceFormat.Vector4; lightResMultiplier = 1f; break;
                case "high": surfaceFormat = SurfaceFormat.HalfVector4; lightResMultiplier = 1f; break;
                case "medium": surfaceFormat = SurfaceFormat.HalfVector4; lightResMultiplier = 0.5f; break;
                case "low": surfaceFormat = SurfaceFormat.HalfVector4; lightResMultiplier = 0.25f; break;
            }

            colorTarget = new RenderTarget2D(GraphicsDevice, 
                screenWidth, screenHeight, false, surfaceFormat, DepthFormat.Depth24Stencil8);
            normalTarget = new RenderTarget2D(GraphicsDevice, 
                screenWidth, screenHeight, false, surfaceFormat, DepthFormat.Depth24Stencil8);
            positionTarget = new RenderTarget2D(GraphicsDevice, 
                screenWidth, screenHeight, false, surfaceFormat, DepthFormat.Depth24Stencil8);
            lightTarget = new RenderTarget2D(GraphicsDevice, 
                (int)(screenWidth * lightResMultiplier), (int)(screenHeight * lightResMultiplier), false, surfaceFormat, DepthFormat.Depth24Stencil8);
            bloomFilterTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, surfaceFormat, DepthFormat.Depth24Stencil8);
            blurHTarget= new RenderTarget2D(GraphicsDevice, 
                (int)(screenWidth * lightResMultiplier), (int)(screenHeight * lightResMultiplier), false, surfaceFormat, DepthFormat.Depth24Stencil8);
            blurVTarget = new RenderTarget2D(GraphicsDevice, 
                (int)(screenWidth * lightResMultiplier), (int)(screenHeight * lightResMultiplier), false, surfaceFormat, DepthFormat.Depth24Stencil8);
        }
        float timeL = 0f;
        int maxPointLights = 1;
        public void UpdatePointLights(float deltaTime)
        {
            timeL += deltaTime * 2f;
            for (int i = 0; i < maxPointLights; i++)
            {
                if(i%2 == 0)
                    lights[i].position = new Vector3(74, 10, -170) + new Vector3(MathF.Sin(timeL) , 0, MathF.Cos(timeL ));
                else
                    lights[i].position = new Vector3(74, 10, -170) + new Vector3(MathF.Sin(-timeL), 0, MathF.Cos(-timeL ));
            }
        }

        List<int> offsetR = new List<int>();
        List<float> offsetT = new List<float>();
        List<LightVolume> lights = new List<LightVolume>();
        void GeneratePointLights()
        {
            var random = new Random();
            var light = new PointLight(new Vector3(74, 10, -170), 15f, new Vector3(1,0,1), new Vector3(1, 0, 1));
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
        public class CollisionTriangle
        {
            public Vector3[] v;
            public CollisionTriangle()
            {
                v = new Vector3[3];
            }
            public Vector3 GetNormal()
            {
                Vector3 edge1 = v[1] - v[0];
                Vector3 edge2 = v[2] - v[0];
                return Vector3.Cross(edge1, edge2);
            }
        }
        public bool CheckTriangle(CollisionTriangle t)
        {
            return
                Vector3.DistanceSquared(t.v[0], camera.position) < 500f;
        }
        List<PointLight> minilights = new List<PointLight>();

        public List<CollisionTriangle> mapTriangles = new List<CollisionTriangle>();
        public List<BoundingSphere>[] boundingSpheresMP; 
        private void BuildMapCollider()
        {
            foreach (ModelMesh mesh in dust2.Meshes)
            {
                boundingSpheresMP = new List<BoundingSphere>[mesh.MeshParts.Count];
                
                Matrix transform = CreateTransform(mesh.ParentBone);
                int index = 0;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    boundingSpheresMP[index] = ExtractMeshPart(meshPart, transform);
                    index++;
                }
                //aztecMPColliders = aztecMeshPartColliders.ToArray();
                //aztecMeshPartColliders.Clear();
            }
        }
        public Matrix CreateTransform(ModelBone bone)
        {
            if (bone == null)
                return Matrix.Identity;

            return bone.Transform * CreateTransform(bone.Parent);
        }
        //BoundingSphere[] aztecMPColliders;
        //List<BoundingSphere> aztecMeshPartColliders = new List<BoundingSphere>();

        
        public List<BoundingSphere> ExtractMeshPart(ModelMeshPart meshPart, Matrix transform)
        {
            List<BoundingSphere> boundingSpheres = new List<BoundingSphere>();
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

                        //if(i==0)
                        //    aztecMeshPartColliders.Add(new BoundingSphere(allVertex[indices[i]], 25f));
                        //var pl = new PointLight(triangle.v[0], 2f, Color.White.ToVector3(), Color.White.ToVector3());
                        //pl.skipDraw = true;
                        //pl.hasLightGeo = true;
                        //lightsManager.Register(pl);
                        //pl = new PointLight(triangle.v[1], 2f, Color.White.ToVector3(), Color.White.ToVector3());
                        //pl.skipDraw = true;
                        //pl.hasLightGeo = true;
                        //lightsManager.Register(pl);
                        //pl = new PointLight(triangle.v[2], 2f, Color.White.ToVector3(), Color.White.ToVector3());
                        //pl.skipDraw = true;
                        //pl.hasLightGeo = true;
                        //lightsManager.Register(pl);

                        //var pl = new PointLight((triangle.v[0] + triangle.v[1] + triangle.v[2]) /3, 2f, Color.Red.ToVector3(), Color.Red.ToVector3());
                        //pl.skipDraw = true;
                        //pl.hasLightGeo = true;
                        //lightsManager.Register(pl);

                        var center = (triangle.v[0] + triangle.v[1] + triangle.v[2]) / 3;
                        var d0 = Vector3.DistanceSquared(center, triangle.v[0]);
                        var d1 = Vector3.DistanceSquared(center, triangle.v[1]);
                        var d2 = Vector3.DistanceSquared(center, triangle.v[2]);

                        var radius = Math.Max(Math.Max(d0, d1), d2);
                        
                        boundingSpheres.Add(new BoundingSphere(center, radius)); 

                        mapTriangles.Add(triangle);
                        
                    }
                }
            }

            return boundingSpheres;
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

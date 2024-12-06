using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using nixfps.Components.Collisions;
using nixfps.Components.Effects;
using nixfps.Components.Gizmos;
using nixfps.Components.GUI;
using nixfps.Components.Gun;
using nixfps.Components.HUD;
using nixfps.Components.Input;
using nixfps.Components.Lights;
using nixfps.Components.Network;
using nixfps.Components.Skybox;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace nixfps.Components.States
{
    public class StateRun : GameState
    {
        public StateRun() : base()
        {
            inputManager = new InputGameRun();
            gui = new GuiPause(this);
        }
        public override void OnSwitch()
        {
            
            System.Windows.Forms.Cursor.Position = inputManager.center;
            game.IsMouseVisible = false;
            game.hud.miniMapEnabled = true;
            
        }
        List<LightVolume> miniLights = new List<LightVolume>();

        List<LightVolume> mapLights = new List<LightVolume>();

        float jumpForce = 10f * 2;
        float gravity = -9.81f * 4f;
        float verticalVelocity = 0f;

        public float blendFactor = 0f;
        //public bool blendStart = false;

        public int closeEnoughC;
        //public bool onAir = false;
        public float airTime = 0f;

        bool mb2down = false;
        bool mb2click = false;

        Vector3 currentPosHit = Vector3.Zero;
        Player lp;

        bool prevMouseLock;
        bool prevIsActive;
        public override void Update(GameTime gameTime)
        {
            ///TODO: 

            /// footsteps
            

            if (lp == null)
                lp = game.localPlayer;
            
            //game.animationManager.animationPlayer.blendFactor = blendFactor;
            //game.animationManager.animationPlayer.blendStart = blendStart;
            

            base.Update(gameTime);
            
            NetworkManager.UpdatePlayers(uDeltaTimeFloat);

            NetworkManager.InterpolatePlayers(game.mainStopwatch.ElapsedMilliseconds);

            game.gizmos.UpdateViewProjection(game.camera.view, game.camera.projection);

            game.animationManager.Update(uDeltaTimeFloat);
            //game.UpdatePointLights(uDeltaTimeFloat);

            game.lightsManager.Update(uDeltaTimeFloat);
            game.gunManager.Update(gameTime);
            game.hud.Update(uDeltaTimeFloat);

            game.camera.Update(inputManager);

            if (prevIsActive != game.IsActive)
            {
                prevIsActive = game.IsActive;
                if (!game.IsActive)
                {
                    prevMouseLock = game.camera.mouseLocked;
                    game.camera.mouseLocked = false;
                }
                else
                {
                    game.camera.mouseLocked = prevMouseLock;
                }
            }


            var keyState = Keyboard.GetState();
            var changed = false;
            //if(keyState.IsKeyDown(Keys.Q))
            //{
            //    game.Exit();
            //}
            if (keyState.IsKeyDown(Keys.NumPad1) || keyState.IsKeyDown(Keys.D1))
            {
                game.gunManager.ChangeGun(1);
            }
            if (keyState.IsKeyDown(Keys.NumPad2) || keyState.IsKeyDown(Keys.D2))
            {
                game.gunManager.ChangeGun(2);
            }
            if (keyState.IsKeyDown(Keys.R))
            {
                game.gunManager.currentGun.Reload();
            }

           

            if (keyState.IsKeyDown(Keys.Up))
            {
                game.camera.pitch += uDeltaTimeFloat * 70;
                if (game.camera.pitch > 90f)
                    game.camera.pitch = 90;
                changed = true;
            }
            if (keyState.IsKeyDown(Keys.Down))
            {
                game.camera.pitch -= uDeltaTimeFloat * 70;
                if (game.camera.pitch < -90)
                    game.camera.pitch = -90;
                changed = true;
            }
            if (keyState.IsKeyDown(Keys.Left))
            {
                game.camera.yaw -= uDeltaTimeFloat * 70;
                if (game.camera.yaw < 0)
                    game.camera.yaw = 0;
                changed = true;
            }
            if (keyState.IsKeyDown(Keys.Right))
            {
                game.camera.yaw += uDeltaTimeFloat * 70;
                if (game.camera.yaw > 360)
                    game.camera.yaw = 0;
                changed = true;
            }
            if (changed)
            {
                game.camera.UpdateCameraVectors();
            }
            
            
            //if(lp.clipName )

                //NetworkManager.localPlayer.clipName = "run right";
                //NetworkManager.localPlayer.clipName = "idle";
            
            
           
            

            //oneSec += uDeltaTimeFloat;
            //if (oneSec >= 1)
            //{
            //    oneSec = 0f;
            //    packetsIn = NetworkManager.Client.Connection.Metrics.UnreliableIn;
            //    packetsOut = NetworkManager.Client.Connection.Metrics.UnreliableOut;
            //    NetworkManager.Client.Connection.Metrics.Reset();
            //}

            foreach (var l in miniLights)
                game.lightsManager.Destroy(l);
            miniLights.Clear();

            if (!lp.onAir && inputManager.clientInputState.Jump)
            {
                verticalVelocity = jumpForce;
                lp.onAir = true;
            }

            if (lp.onAir)
            {
                verticalVelocity += gravity * uDeltaTimeFloat;

                lp.position.Y += verticalVelocity * uDeltaTimeFloat;

                airTime += uDeltaTimeFloat;
                if (airTime > 2.5f)
                {
                    airTime = 0;
                    lp.onAir = false;

                    lp.position = GetSafeLocation();
                    game.camera.pitch = 0f;
                }
            }
            //game.hud.crosshair.SetColor(onAir?Color.Blue:Color.White);



            //var ms = Mouse.GetState();
            //if (!mb2down)
            //{
            //    if (ms.RightButton == ButtonState.Pressed)
            //    {
            //        mb2down = true;
            //        ShowPointingAt();
            //        var pl = new PointLight(currentPosHit - game.camera.frontDirection * 5f, 20f, new Vector3(1, 1, .8f), new Vector3(1, 1, .8f));
            //        pl.skipDraw = false;
            //        pl.hasLightGeo = false;
            //        game.lightsManager.Register(pl);
            //    }

            //}
            //else
            //{
            //    if (ms.RightButton == ButtonState.Released)
            //    {
            //        mb2down = false;
            //    }
            //}
            MapCollisionInit();
            MapCollisionFloor();
            MapCollisionBox();
            foreach (var l in miniLights)
                game.lightsManager.Register(l);
        }

        float DistanceSqrNoY(Vector3 v1, Vector3 v2)
        {
            Vector2 v21 = new Vector2(v1.X, v1.Z);
            Vector2 v22 = new Vector2(v2.X, v2.Z);

            return Vector2.DistanceSquared(v21, v22);
        }

        Vector3[] spawnPos =
        {
            new Vector3(76, 13.5f, -203.5f),
            new Vector3(20.7f, 11.4f, -181),
            new Vector3(30.4f, 5, -116),
            new Vector3(25, 5, -96),
            new Vector3(31, 11, -128),
            new Vector3(-45, 5, -26),
            new Vector3(-31.6f, 5, 16),
            new Vector3(48.5f, 5, 20.7f),
            new Vector3(22, 5, -26.7f),
            new Vector3(-40, -2.5f, -89),
            new Vector3(-77.7f, -2.5f, -90),
            new Vector3(-93.4f, 7, -82.7f),
            new Vector3(-141, 7, -81),
            new Vector3(-137, 7, -204),
            new Vector3(-96, 5, -185),
            new Vector3(-111, 5, -111),
            new Vector3(-82, 5, -142),
            new Vector3(-20.4f, -2.3f, -164.5f),
            new Vector3(-19, -3.6f, -115.5f),
            new Vector3(-3.8f, -3.6f, -151),
            new Vector3(31, -3.6f, -166),
            new Vector3(8, -3.6f, -166),
            new Vector3(52, 1.5f, -159),
            new Vector3(117.4f, 9.3f, -61),
            new Vector3(116, 9.3f, -31.5f),
            new Vector3(94.8f, -7.86f, -16),
            new Vector3(78, 5, -22.6f),
            new Vector3(44, 5, -76),
            new Vector3(47, 5, -35.5f),
            new Vector3(47, 5, -12.5f),
            new Vector3(29.5f, 5, 56),
            new Vector3(-46, 13.6f, 52),
            new Vector3(-69, 14.7f, 22.5f),
            new Vector3(-142.6f, 13.5f, 43),
            new Vector3(-103.7f, 5, -7.3f),
            new Vector3(-126, 7.2f, -38)
        }; 
        public Vector3 GetSafeLocation()
        {
            Random r = new Random();
            return spawnPos[r.NextInt64(0, spawnPos.Length)];
        }

        Vector3 temp;
        Vector3 Vec3Avg(NixFPS.CollisionTriangle t)
        {
            //return new Vector3(
            //    (t.v[0].X + t.v[1].X + t.v[2].X) / 3,
            //    (t.v[0].Y + t.v[1].Y + t.v[2].Y) / 3,
            //    (t.v[0].Z + t.v[1].Z + t.v[2].Z) / 3);

            temp.X = (t.v[0].X + t.v[1].X + t.v[2].X) / 3;
            temp.Y = (t.v[0].Y + t.v[1].Y + t.v[2].Y) / 3;
            temp.Z = (t.v[0].Z + t.v[1].Z + t.v[2].Z) / 3;

            return temp;
            
        }

        Vector3[] dir = new Vector3[720];
        Vector2[] hitDir = new Vector2[720];
        IOrderedEnumerable<NixFPS.CollisionTriangle> closeEnough;
        void MapCollisionInit()
        {
            var bodyCheckPos = lp.position + new Vector3(0, 1.8f, 0);
            closeEnough = game.mapTriangles
                .FindAll(t => Vector3.DistanceSquared(Vec3Avg(t), bodyCheckPos) < 150f)
                .OrderBy(t => Vector3.DistanceSquared(Vec3Avg(t), bodyCheckPos));
        }
        float hitDown;
        float avgDelta = .5f;
        void MapCollisionFloor()
        {
            hitDown = float.MinValue;
            
            foreach (var triangle in closeEnough)
            {

                var hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    lp.position + new Vector3(0, 2, 0), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);

                if (hitPos.HasValue)
                {
                    if(hitPos.Value.Y > hitDown)
                    {
                        hitDown = hitPos.Value.Y;
                    }
                }
               
            }
            
            var last = lp.position.Y;

            if (last - hitDown > .25f)
            {
                lp.onAir = true;
            }
            if (!lp.onAir)
            {
                lp.position.Y = hitDown;
            }
            else
            {
                if (lp.position.Y - hitDown < 0.05f)
                {
                    airTime = 0;
                    lp.onAir = false;
                }
            }

        }
        int corrections = 0;
        void MapCollisionBox()
        {
            //var lp = game.localPlayer;

            totalChecks = 0;


            bool[] triangleHit = new bool[closeEnough.Count()];

            var closeEnoughArr = closeEnough.Where(c=>Vector3.Dot(c.GetNormal(), Vector3.Up) <= .1f) .ToArray();

            corrections = 0;
          
            do
            {
                //find hits
                for (int i = 0; i < closeEnoughArr.Count(); i++)
                {
                    triangleHit[i] = CollisionHelper.IsTriangleIntersectingAABB(closeEnoughArr[i], lp.boxCollider);
                }
                //attempt to correct one by one, (first correction might correct others)
                for (int i = 0; i < closeEnoughArr.Count(); i++)
                {
                    var t = closeEnoughArr[i];
                    if(triangleHit[i])
                    {
                        Vector3 normal = closeEnoughArr[i].GetNormal();
                        normal.Normalize();
                        corrections = 0;
                        do
                        {
                            lp.position += -normal * uDeltaTimeFloat;

                            lp.UpdateBoxCollider();

                            corrections++;
                        } while (CollisionHelper.IsTriangleIntersectingAABB(closeEnoughArr[i], lp.boxCollider));

                        break;
                    }
                }
            }
            while (triangleHit.Any(h => h)); 

        }


        int totalChecks = 0;
        float triangleDistSqr = 400f;
        float stepDist = 2 * MathF.Sqrt(400f);

        void ShowPointingAt()
        {
            totalChecks = 0;
 
            IOrderedEnumerable<NixFPS.CollisionTriangle> closeEnough;

            var hitList = new List<Vector3>();
            Vector3? hitPos;
            Vector3 fromPos = game.camera.position;
            var camFD = game.camera.frontDirection;
            float posOffset = 0;

            
            while (hitList.Count == 0 && posOffset <= 212f)
            {
                closeEnough = game.mapTriangles
                    .FindAll(t => Vector3.DistanceSquared(t.v[0], fromPos + camFD * posOffset) < triangleDistSqr)
                    .OrderBy(t => Vector3.DistanceSquared(t.v[0], fromPos + camFD * posOffset));

                //foreach (var triangle in closeEnough)
                //{

                //    hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(game.camera.position, camFD, triangle.v[0], triangle.v[1], triangle.v[2]);
                //    totalChecks++;
                //    if (hitPos.HasValue)
                //        hitList.Add(hitPos.Value);
                //}
                Parallel.ForEach(closeEnough, triangle =>
                {

                    var hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(game.camera.position, camFD, triangle.v[0], triangle.v[1], triangle.v[2]);
                    //totalChecks++;
                    if (hitPos.HasValue)
                        hitList.Add(hitPos.Value);
                });
                posOffset += stepDist; //2 * sqrt 500

            }
            if(hitList.Count > 0)
            {
                currentPosHit = hitList.OrderBy(t => Vector3.DistanceSquared(t, game.camera.position)).ToList()[0];
                var pl = new PointLight(currentPosHit - game.camera.frontDirection * 4f, 10f, new Vector3(1, 0, 1), new Vector3(1, 0, 1));
                //var pl = new CylinderLight(currentPosHit - game.camera.frontDirection * 1, 20f, 10f, Color.White.ToVector3(), Matrix.Identity, Color.White.ToVector3());
                pl.hasLightGeo = true;
                pl.skipDraw = false;
                miniLights.Add(pl);

            }

        }

        public Vector3? CameraRayHitMap()
        {
            IOrderedEnumerable<NixFPS.CollisionTriangle> closeEnough;

            var hitList = new List<Vector3>();
            var vertexHitList = new List<Vector3>();

            Vector3 fromPos = game.camera.position;
            var camFD = game.camera.frontDirection;
            float posOffset = 0;

            Vector3[] tempArr = new Vector3[3];

            while (posOffset <= 212f)
            {
                closeEnough = game.mapTriangles
                    .FindAll(t => Vector3.DistanceSquared(t.v[0], fromPos + camFD * posOffset) < triangleDistSqr)
                    .OrderBy(t => Vector3.DistanceSquared(t.v[0], fromPos + camFD * posOffset));

                //foreach (var triangle in closeEnough)
                //{

                //    hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(game.camera.position, camFD, triangle.v[0], triangle.v[1], triangle.v[2]);
                //    totalChecks++;
                //    if (hitPos.HasValue)
                //        hitList.Add(hitPos.Value);
                //}
                //if (closeEnough.Any(t => BoundingVolumesExtensions.IntersectRayWithTriangle(game.camera.position, camFD, t.v[0], t.v[1], t.v[2]).HasValue))
                //    return true;
                Parallel.ForEach(closeEnough, triangle =>
                {

                    var hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(game.camera.position, camFD, triangle.v[0], triangle.v[1], triangle.v[2]);
                    //totalChecks++;
                    if (hitPos.HasValue)
                    {
                        hitList.Add(hitPos.Value);
                    }
                });
                posOffset += stepDist; //2 * sqrt 500

            }

            if(hitList.Count == 1)
                return hitList[0];

            if (hitList.Count >= 2)
                return hitList.OrderBy(t => Vector3.DistanceSquared(t, fromPos)).ToArray()[0];
            
            return null;
        }

        String fpsStr = "";
        string str2 = "";
        string str3 = "";
        string str4 = "";
        float dTime = 0;
        public override void Draw(GameTime gameTime)
        {
            dTime += dDeltaTimeFloat;
            base.Draw(gameTime);

            game.basicModelEffect.SetView(game.camera.view);
            game.basicModelEffect.SetProjection(game.camera.projection);
            game.deferredEffect.SetView(game.camera.view);
            game.deferredEffect.SetProjection(game.camera.projection);
            game.deferredEffect.SetCameraPosition(game.camera.position);
            /// Target 1 (colorTarget) RGB = color, A = KD
            /// Target 2 (normalTarget) RGB = normal(scaled), A = KS
            /// Target 3 (positionTarget) RGB = world position, A = shininess(scale if necessary)
            /// Target 4 (bloomTarget) RGB = filter, A = (not in use) 
            game.GraphicsDevice.SetRenderTargets(game.colorTarget, game.normalTarget, game.positionTarget, game.bloomFilterTarget);
            game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            game.skybox.Draw(game.camera.view, game.camera.projection, game.camera.position);
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw a simple plane, enable lighting on it
            //DrawPlane();
            //DrawAztec();
            //DrawBox();
            DrawDust2();
            

            //gizmos.DrawSphere(localPlayer.zoneCollider.Center, new Vector3(localPlayer.zoneCollider.Radius), Color.White);
            //gizmos.DrawCylinder(localPlayer.headCollider.Center, localPlayer.headCollider.Rotation, 
            //    new Vector3(localPlayer.headCollider.Radius, localPlayer.headCollider.HalfHeight, localPlayer.headCollider.Radius), Color.Red);

            //game.gizmos.DrawCylinder(NetworkManager.localPlayer.bodyCollider.Center, NetworkManager.localPlayer.bodyCollider.Rotation,
            //    new Vector3(NetworkManager.localPlayer.bodyCollider.Radius, NetworkManager.localPlayer.bodyCollider.HalfHeight, NetworkManager.localPlayer.bodyCollider.Radius), Color.Green);

            //game.localPlayer.boxCollider.Min
            //game.gizmos.DrawCube(lp.position + new Vector3(0, 2.5f, 0), new Vector3(lp.boxWidth, lp.boxHeight, lp.boxWidth), corrections>0? Color.Red : Color.Green);
            //game.gizmos.Draw();
            



            game.animationManager.DrawPlayers();
            game.lightsManager.DrawLightGeo();

            game.gunManager.DrawGun(uDeltaTimeFloat);
            
            
            game.gizmos.Draw();

            // Draw the geometry of the lights in the scene, so that we can see where the generators are

            /// Now we calculate the lights. first we start by sending the targets from before as textures
            /// First, we use a fullscreen quad to calculate the ambient light, as a baseline (optional)
            /// Then, we iterate our point lights and render them as spheres in the correct position. 
            /// This will launch pixel shader functions only for the necessary pixels in range of that light.
            /// From the G-Buffer we sample the required information for that pixel, and we compute the color
            /// BlendState should be additive, to correctly sum up the contributions of multiple lights in
            /// the same pixel.
            /// For pixels that shouldnt be lit, for example the light geometry, normals are set to rgb = 0
            /// and we can use that to simply output white in our lightTarget for that pixel.
            game.GraphicsDevice.SetRenderTargets(game.lightTarget, game.blurHTarget, game.blurVTarget);
            game.GraphicsDevice.BlendState = BlendState.Additive;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            game.deferredEffect.SetColorMap(game.colorTarget);
            game.deferredEffect.SetNormalMap(game.normalTarget);
            game.deferredEffect.SetPositionMap(game.positionTarget);
            game.deferredEffect.SetBloomFilter(game.bloomFilterTarget);
            game.lightsManager.Draw();


            game.hud.DrawMiniMapTarget(dDeltaTimeFloat);

            /// Finally, we have our color texture we calculated in step one, and the lights from step two
            /// we combine them here by simply multiplying them, finalColor = color * light, 
            /// using a final fullscreen quad pass.
            game.GraphicsDevice.SetRenderTarget(null);
            //GUI.Draw(gt);
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            game.deferredEffect.SetLightMap(game.lightTarget);


            game.deferredEffect.SetScreenSize(new Vector2(game.lightTarget.Width, game.lightTarget.Height));
            //game.deferredEffect.SetScreenSize(new Vector2(game.screenWidth, game.screenHeight));
            game.deferredEffect.SetTech("integrate");
            game.deferredEffect.SetBlurH(game.blurHTarget);
            game.deferredEffect.SetBlurV(game.blurVTarget);

            game.fullScreenQuad.Draw(game.deferredEffect.effect);
            
            if (dTime >= 0.05f)
            {
                dTime = 0;
                var cam = game.camera.position;
                var ap = game.animationManager.animationPlayer;
                var pos = lp.position;
                
                fpsStr = " FPS " + FPS;
                //fpsStr += $" {lp.position.X:F2} {lp.position.Z:F2} ";
                if (NetworkManager.Client.IsConnected)
                {
                    str2 = " RTT " + NetworkManager.Client.RTT + "ms";
                }
                else
                {
                    str2 = " Reconectando...";
                }
                str3 = $" KD {lp.kills}/{lp.deaths}";
                str4 = $"{game.gunManager.currentGun.pitchDelta:F2} ";
                //if (NetworkManager.players.Count > 0)
                //    str4 = $"{NetworkManager.players[0].footsteps}";

                        //    str4 = $" KD enemigo {NetworkManager.players[0].kills}/{NetworkManager.players[0].deaths}";
                        //str3 = $"{game.gunManager.currentGun.reload} {game.gunManager.currentGun.reloadTimer}";


            }
            game.spriteBatch.Begin();
            var fontSizeY = 15;
            var currentY = game.hud.mapHeight + fontSizeY + 5;
            game.spriteBatch.DrawString(game.fontSmall, fpsStr, new Vector2(0, currentY), Color.White);
            currentY += fontSizeY + 5;
            game.spriteBatch.DrawString(game.fontSmall,  str2, new Vector2(0, currentY), Color.White);
            currentY += fontSizeY + 5;
            game.spriteBatch.DrawString(game.fontSmall, str3, new Vector2(0, currentY), Color.White);
            currentY += fontSizeY + 5; 
            game.spriteBatch.DrawString(game.fontSmall, str4, new Vector2(0, currentY), Color.White);


            //spriteBatch.DrawString(font, str, new Vector2(screenWidth - font.MeasureString(str).X, 0), Color.White);
            game.spriteBatch.End();
            game.hud.DrawRun(dDeltaTimeFloat);
            
            if(GameStateManager.paused)
                gui.Draw(gameTime);
        }
        private void DrawPlane()
        {
            game.basicModelEffect.SetTech("colorTex_lightEn");
            game.basicModelEffect.SetTiling(Vector2.One * 500);
            foreach (var mesh in game.plane.Meshes)
            {
                var w = mesh.ParentBone.Transform * Matrix.CreateScale(10f) * Matrix.CreateTranslation(0, 0, 0);
                game.basicModelEffect.SetColor(Color.DarkGray.ToVector3());
                game.basicModelEffect.SetWorld(w);
                game.basicModelEffect.SetKA(0.3f);
                game.basicModelEffect.SetKD(0.8f);
                game.basicModelEffect.SetKS(0.8f);
                game.basicModelEffect.SetShininess(30f);
                game.basicModelEffect.SetColorTexture(game.floorTex);
                game.basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));

                mesh.Draw();
            }
            game.basicModelEffect.SetTiling(Vector2.One);
        }

        private void DrawBox()
        {
            game.basicModelEffect.SetTech("colorTex_lightEn");
            game.basicModelEffect.SetTiling(new Vector2(4.5f, 4.5f));
            foreach (var mesh in game.cube.Meshes)
            {
                var w = mesh.ParentBone.Transform * Matrix.CreateScale(.01f) * Matrix.CreateTranslation(4, 2, 4);
                game.basicModelEffect.SetWorld(w);
                game.basicModelEffect.SetKA(0.3f);
                game.basicModelEffect.SetKD(0.8f);
                game.basicModelEffect.SetKS(0.8f);
                game.basicModelEffect.SetShininess(30f);
                game.basicModelEffect.SetColorTexture(game.boxTex);
                game.basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));

                mesh.Draw();
            }
            game.basicModelEffect.SetTiling(Vector2.One);

            var min = new Vector3(4, 2, 4) - new Vector3(1, 1f, 1);
            var max = new Vector3(4, 2, 4) + new Vector3(1, 1f, 1);


            game.gizmos.DrawCube(new Vector3(4, 2, 4), new Vector3(2, 2, 2), Color.Magenta);

        }
        int meshPartDrawCount = 0;


        void DrawDust2()
        {
            meshPartDrawCount = 0;
            
            game.basicModelEffect.SetTech("colorTexNormal_lightEn");
            game.basicModelEffect.SetTiling(new Vector2(1f));

            //ceramic
            game.basicModelEffect.SetKA(.3f);
            game.basicModelEffect.SetKD(.8f);
            game.basicModelEffect.SetKS(.8f);
            game.basicModelEffect.SetShininess(10f);

            //game.basicModelEffect.SetKA(.3f);
            //game.basicModelEffect.SetKD(.6f);
            //game.basicModelEffect.SetKS(.1f);
            //game.basicModelEffect.SetShininess(2f);


            foreach (var mesh in game.dust2.Meshes)
            {
                var w = mesh.ParentBone.Transform * Matrix.CreateScale(1);
                game.basicModelEffect.SetWorld(w);
                game.basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                for (int partindex = 0; partindex < mesh.MeshParts.Count; partindex++)
                {
                    if (!game.boundingSpheresMP[partindex].Any(bs => game.camera.FrustumContains(bs)))
                        continue;
                    
                    game.basicModelEffect.SetColor(Color.White.ToVector3());

                    var part = mesh.MeshParts[partindex];
                    //game.basicModelEffect.SetColorTexture(game.numTex[partindex]);
                    var values = Dust2Values(partindex);

                    game.basicModelEffect.SetColorTexture(values.tex);
                    game.basicModelEffect.SetNormalTexture(values.normal);
                    game.basicModelEffect.SetKA(values.ka);
                    game.basicModelEffect.SetKD(values.kd);
                    game.basicModelEffect.SetKS(values.ks);
                    game.basicModelEffect.SetShininess(values.sh);

                    foreach (var pass in game.basicModelEffect.effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        game.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        game.GraphicsDevice.Indices = part.IndexBuffer;
                        game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                    meshPartDrawCount++;
                }

            }
        }
        void DrawAztec()
        {
            meshPartDrawCount = 0;
            //basicModelEffect.SetTech("colorTex_lightEn");
            //basicModelEffect.SetTech("basic_color");
            game.basicModelEffect.SetTech("number");
            game.basicModelEffect.SetTiling(new Vector2(2f));
            game.basicModelEffect.SetKA(.3f);
            game.basicModelEffect.SetKD(.8f);
            game.basicModelEffect.SetKS(.8f);
            game.basicModelEffect.SetShininess(10f);

            foreach (var mesh in game.aztec.Meshes)
            {
                meshPartDrawCount = 0;


                var w = mesh.ParentBone.Transform * Matrix.CreateScale(1f);
                game.basicModelEffect.SetWorld(w);

                game.basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));

                for (int partindex = 0; partindex < mesh.MeshParts.Count; partindex++)
                {
                    //if (camera.FrustumContains(aztecMPColliders[partindex]))
                    //    continue;
                    meshPartDrawCount++;
                    var (found, tex) = AztecTexFor(partindex);
                    game.basicModelEffect.SetColorTexture(tex);
                    game.basicModelEffect.SetColor(Color.White.ToVector3());
                    game.basicModelEffect.SetTiling(new Vector2(1f));
                    if (!found)
                    {
                        game.basicModelEffect.SetTiling(new Vector2(2f));
                        if (partindex < 100)
                        {
                            game.basicModelEffect.SetColor(Color.Red.ToVector3());
                        }
                        else if (partindex < 200)
                        {
                            game.basicModelEffect.SetColor(Color.Green.ToVector3());
                        }
                        else
                        {
                            game.basicModelEffect.SetColor(Color.Blue.ToVector3());
                        }
                    }


                    var part = mesh.MeshParts[partindex];

                    foreach (var pass in game.basicModelEffect.effect.CurrentTechnique.Passes)
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

        public (bool, Texture2D) AztecTexFor(int index)
        {
            switch (index)
            {
                case 0: return (true, game.aztecTex[0]);
                case 1: return (true, game.aztecTex[22]);
                case 2: return (true, game.aztecTex[3]);
                case 3: return (true, game.aztecTex[3]);

                case 4: return (true, game.aztecTex[52]);
                case 5: return (true, game.aztecTex[53]);
                case 6: return (true, game.aztecTex[49]);
                case 10: return (true, game.aztecTex[10]);
                case 17: return (true, game.aztecTex[22]);

                case 26: return (true, game.aztecTex[31]);
                case 27: return (true, game.aztecTex[32]);

                case 30: return (true, game.aztecTex[35]);
                case 46: return (true, game.aztecTex[46]);
                case 200: return (true, game.aztecTex[60]);
                case 201: return (true, game.aztecTex[58]);

                case 214: return (true, game.aztecTex[35]);


                default:
                    return (false, game.numTex[index % 100]);
            }
        }

        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) brick_detail;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) brick_wall;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) dd;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) box;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) box_side;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) sand;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) tile;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) rock;
        (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) light;
        public void InitDust2Values()
        {
            brick_detail = (game.dust2Tex[0], game.dust2NormalTex[0], .3f, .6f, .1f, 2);
            brick_wall = (game.dust2Tex[1], game.dust2NormalTex[1], .3f, .6f, .1f, 2);
            dd = (game.dust2Tex[2], game.dust2NormalTex[2], .3f, .6f, .1f, 2);
            box = (game.dust2Tex[3], game.dust2NormalTex[3], .3f, .6f, .1f, 2);
            box_side = (game.dust2Tex[4], game.dust2NormalTex[4], .3f, .6f, .1f, 2);
            sand = (game.dust2Tex[5], game.dust2NormalTex[5], .3f, .6f, .1f, 2);
            tile = (game.dust2Tex[6], game.dust2NormalTex[6], .3f, .8f, .8f, 10);
            rock = (game.dust2Tex[7], game.dust2NormalTex[7], .3f, .8f, .8f, 10);
            light = (game.dust2Tex[8], game.dust2NormalTex[1], .3f, .8f, .8f, 10);
        }

        public (Texture2D tex, Texture2D normal, float ka, float kd, float ks, float sh) Dust2Values(int index)
        {
            
            switch (index)
            {
                case 0: return brick_detail;
                case 1: return brick_wall;
                case 2: return box;
                //3 white
                case 4: return brick_wall;
                case 5: return sand;
                case 6: return sand;
                case 7: return dd;
                case 8: return brick_wall;
                case 9: return box;
                case 10: return tile;
                case 11: return rock;
                case 12: return box_side;
                case 13: return brick_wall;
                case 14: return brick_wall;
                case 15: return tile;
                case 16: return brick_wall;
                case 17: return box_side;

                case 20: return box_side;
                case 21: return box;
                case 22: return sand;
                case 23: return dd;

                case 25: return tile;
                case 26: return dd;
                case 27: return brick_wall;
                case 28: return dd;
                case 29: return dd;
                case 30: return dd;
                case 31: return box_side;
                case 32: return rock;
                case 33: return light;
                default: return brick_detail;
            }
        }
    }
}

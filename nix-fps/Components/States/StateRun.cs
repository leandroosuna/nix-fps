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
            gui = new GuiPause();
        }
        public override void OnSwitch()
        {
            
            System.Windows.Forms.Cursor.Position = inputManager.center;
            game.IsMouseVisible = false;
        }
        List<LightVolume> miniLights = new List<LightVolume>();

        List<LightVolume> mapLights = new List<LightVolume>();

        float jumpForce = 10f * 2;
        float gravity = -9.81f * 4f;
        float verticalVelocity = 0f;


        public int closeEnoughC;
        public bool onAir = false;
        public float airTime = 0f;

        bool mb2down = false;
        bool mb2click = false;

        Vector3 currentPosHit = Vector3.Zero;
        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

            NetworkManager.InterpolatePlayers(game.mainStopwatch.ElapsedMilliseconds);

            game.gizmos.UpdateViewProjection(game.camera.view, game.camera.projection);

            game.animationManager.Update(gameTime);
            game.UpdatePointLights(uDeltaTimeFloat);

            game.lightsManager.Update(uDeltaTimeFloat);
            game.gunManager.Update(gameTime);
            game.hud.Update(uDeltaTimeFloat);

            game.camera.Update(inputManager);

            var keyState = Keyboard.GetState();
            var changed = false;
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

            NetworkManager.localPlayer.clipName = "idle";
            if (keyState.IsKeyDown(Keys.L))
            {
                NetworkManager.localPlayer.clipName = "run right";
            }
            if (keyState.IsKeyDown(Keys.J))
            {
                NetworkManager.localPlayer.clipName = "run left";
            }
            if (keyState.IsKeyDown(Keys.I))
            {
                if (keyState.IsKeyDown(Keys.U))
                {
                    NetworkManager.localPlayer.clipName = "sprint forward";

                    if (keyState.IsKeyDown(Keys.L))
                        NetworkManager.localPlayer.clipName = "sprint forward right";
                    if (keyState.IsKeyDown(Keys.J))
                        NetworkManager.localPlayer.clipName = "sprint forward left";
                }
                else
                {
                    NetworkManager.localPlayer.clipName = "run forward";

                    if (keyState.IsKeyDown(Keys.L))
                        NetworkManager.localPlayer.clipName = "run forward right";
                    if (keyState.IsKeyDown(Keys.J))
                        NetworkManager.localPlayer.clipName = "run forward left";
                }
            }
            if (keyState.IsKeyDown(Keys.K))
            {
                NetworkManager.localPlayer.clipName = "run backward";
                
                if (keyState.IsKeyDown(Keys.L))
                    NetworkManager.localPlayer.clipName = "run backward right";
                if (keyState.IsKeyDown(Keys.J))
                    NetworkManager.localPlayer.clipName = "run backward left";

            }
            

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

            if (!onAir && inputManager.clientInputState.Jump)
            {
                verticalVelocity = jumpForce;
                onAir = true;
            }

            if (onAir)
            {
                verticalVelocity += gravity * uDeltaTimeFloat;

                game.localPlayer.position.Y += verticalVelocity * uDeltaTimeFloat;

                airTime += uDeltaTimeFloat;
                if (airTime > 2.5f)
                {
                    airTime = 0;
                    onAir = false;

                    game.localPlayer.position = GetSafeLocation();
                }
            }
            //game.hud.crosshair.SetColor(onAir?Color.Blue:Color.White);

            //ShowPointingAt();

            
            var ms = Mouse.GetState();
            if (!mb2down)
            {
                if (ms.RightButton == ButtonState.Pressed)
                {
                    mb2down = true;
                    ShowPointingAt();
                    var pl = new PointLight(currentPosHit - game.camera.frontDirection * 5f, 20f, new Vector3(1, 1, .8f), new Vector3(1, 1, .8f));
                    pl.skipDraw = false;
                    pl.hasLightGeo = false;
                    game.lightsManager.Register(pl);
                }

            }
            else
            {
                if (ms.RightButton == ButtonState.Released)
                {
                    mb2down = false;
                }
            }

            MapCollision();

            //foreach (var l in miniLights)
            //    game.lightsManager.Register(l);
        }

        

        float DistanceSqrNoY(Vector3 v1, Vector3 v2)
        {
            Vector2 v21 = new Vector2(v1.X, v1.Z);
            Vector2 v22 = new Vector2(v2.X, v2.Z);

            return Vector2.DistanceSquared(v21, v22);
        }
        Vector3 GetSafeLocation()
        {
            var safe = game.mapTriangles
                      .FindAll(t => DistanceSqrNoY(t.v[0], game.localPlayer.position) < 100f);

            if(safe.Count == 0)
                return new Vector3(97, 8, -205);

            var ordered = 
                      safe.OrderByDescending(t => DistanceSqrNoY(t.v[0], game.localPlayer.position)).ToList();

            return ordered[0].v[0];

           
            
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

        void MapCollision()
        {
            var bodyCheckPos = game.localPlayer.position + new Vector3(0, 1.8f, 0);

            var closeEnough = game.mapTriangles
                .FindAll(t => Vector3.DistanceSquared(Vec3Avg(t), bodyCheckPos) < 250f)
                .OrderBy(t => Vector3.DistanceSquared(Vec3Avg(t), bodyCheckPos));
                
            closeEnoughC = closeEnough.Count();

            var flatFrontDir = game.localPlayer.frontDirection;
            flatFrontDir.Y = 0;
            flatFrontDir.Normalize();
            
            var flatBackDir = -flatFrontDir;

            var flatRightDir = game.localPlayer.rightDirection;
            flatRightDir.Y = 0;
            flatRightDir.Normalize();

            var flatLeftDir = -flatRightDir;
            
            var avgDelta = .5f;

            var cis = inputManager.clientInputState;

            Vector3[] dir = new Vector3[9];

            dir[0] = cis.Forward? flatFrontDir: Vector3.Zero;
            dir[0] += cis.Backward? -flatFrontDir : Vector3.Zero;
            dir[0] += cis.Right ? flatRightDir: Vector3.Zero;
            dir[0] += cis.Left? -flatRightDir : Vector3.Zero;

            if (dir[0] == Vector3.Zero)
                dir[0] = flatFrontDir;

            var rightFromDir = Vector3.Cross(dir[0], Vector3.Up);
            dir[1] = dir[0] * 3.5f + rightFromDir;
            dir[2] = dir[0] * 3.5f - rightFromDir;
            dir[3] = dir[0] * 2 + rightFromDir;
            dir[4] = dir[0] * 2 - rightFromDir;
            dir[5] = dir[0] * 2 + rightFromDir * 1.2f;
            dir[6] = dir[0] * 2 - rightFromDir * 1.2f;
            dir[7] = dir[0] + rightFromDir * 2;
            dir[8] = dir[0] - rightFromDir * 2;
            

            foreach (var d in dir)
            {
                d.Normalize();
            }


            float[] hitDown = new float[5];

            for(int i = 0; i < hitDown.Length; i++)
            {
                hitDown[i] = float.MinValue;
            }

            Vector2[] hitDir = new Vector2[7];
            for (int i = 0; i < hitDir.Length; i++)
            {
                hitDir[i] = Vector2.Zero;
            }
            
            foreach (var triangle in closeEnough)
            {

                var hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(0, 2, 0), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);

                if(hitPos.HasValue)
                {
                    if (hitPos.Value.Y > hitDown[0])
                    {
                        hitDown[0] = hitPos.Value.Y;

                        var pl = new PointLight(hitPos.Value, 5, Color.Blue.ToVector3(), Color.Blue.ToVector3());
                        pl.skipDraw = true;
                        pl.hasLightGeo = true;
                        miniLights.Add(pl);
                    }
                    
                }
                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(avgDelta, 2, avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);

                if (hitPos.HasValue)
                {
                    if (hitPos.Value.Y > hitDown[1])
                    {
                        hitDown[1] = hitPos.Value.Y;
                    }
                }
                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(-avgDelta, 2, avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    if (hitPos.Value.Y > hitDown[2])
                    {
                        hitDown[2] = hitPos.Value.Y;
                    }
                }

                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(avgDelta, 2, -avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    if (hitPos.Value.Y > hitDown[3])
                    {
                        hitDown[3] = hitPos.Value.Y;
                    }
                }
                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(-avgDelta, 2, -avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);

                if (hitPos.HasValue)
                {
                    if (hitPos.Value.Y > hitDown[4])
                    {
                        hitDown[4] = hitPos.Value.Y;
                    }
                }


                for (int i = 0; i < hitDir.Count(); i++)
                {
                    if (hitDir[i] == Vector2.Zero)
                    {
                        hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                            bodyCheckPos, dir[i], triangle.v[0], triangle.v[1], triangle.v[2]);

                        if(hitPos.HasValue)
                        {
                            hitDir[i] = new Vector2(hitPos.Value.X, hitPos.Value.Z); 
                        }
                    }
                }



            }
            
            var last = game.localPlayer.position.Y;

            

            //var hitCount = 0;
            //var acc = 0f;    
            //foreach(var h  in hitDown) 
            //{
            //    if (h == float.MinValue)
            //        continue;
            //    acc += h;
            //    hitCount++;
            //}

            //var newY = acc/hitCount;
            var newY = hitDown.Average();

            if (last - hitDown[0] > .5f)
            {
                onAir = true;
                
            }
            if (!onAir)
            {
                game.localPlayer.position.Y = newY;
            }
            else
            {
                if (game.localPlayer.position.Y - hitDown[0] < 0.05f)
                {
                    airTime = 0;
                    onAir = false;
                }
            }
            
            var correction = 1.55f;
            var playerNoY = new Vector2(game.localPlayer.position.X, game.localPlayer.position.Z);

            
            foreach(var hit in hitDir) 
            {
                if (hit == Vector2.Zero)
                    continue;
                var v2 = playerNoY - hit;

                var pl = new PointLight(new Vector3(hit.X, game.localPlayer.position.Y + 1.8f, hit.Y), 20f, Color.Green.ToVector3(), Color.Green.ToVector3());
                pl.hasLightGeo = true;
                pl.skipDraw = true;
                miniLights.Add(pl);

                if (v2.LengthSquared() < correction * correction) 
                {
                    v2.Normalize();
                    game.localPlayer.position.X = hit.X + v2.X * correction * 1.05f;
                    game.localPlayer.position.Z = hit.Y + v2.Y * correction * 1.05f;
                    pl.color = Color.Red.ToVector3();
                }
            }
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
        String fpsStr = "";
        
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
            game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            game.skybox.Draw();
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw a simple plane, enable lighting on it
            //DrawPlane();
            //DrawAztec();
            //DrawBox();
            DrawDust2();
            game.localPlayer.UpdateColliders();

            //gizmos.DrawSphere(localPlayer.zoneCollider.Center, new Vector3(localPlayer.zoneCollider.Radius), Color.White);
            //gizmos.DrawCylinder(localPlayer.headCollider.Center, localPlayer.headCollider.Rotation, 
            //    new Vector3(localPlayer.headCollider.Radius, localPlayer.headCollider.HalfHeight, localPlayer.headCollider.Radius), Color.Red);
            //gizmos.DrawCylinder(localPlayer.bodyCollider.Center, localPlayer.bodyCollider.Rotation, 
            //    new Vector3(localPlayer.bodyCollider.Radius, localPlayer.bodyCollider.HalfHeight, localPlayer.bodyCollider.Radius), Color.Green);
            //gizmos.Draw();
            game.gunManager.DrawGun(dDeltaTimeFloat);


            game.animationManager.DrawPlayers();
            game.gizmos.Draw();

            // Draw the geometry of the lights in the scene, so that we can see where the generators are
            game.lightsManager.DrawLightGeo();

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
            game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            //var rec = new Rectangle(0, 0, game.screenWidth, game.screenHeight);

            /// In this example, by hitting key 0 you can see the targets in the corners of the screen
            //if (debugRTs)
            //{
            //    spriteBatch.Begin(blendState: BlendState.Opaque);

            //    spriteBatch.Draw(colorTarget, Vector2.Zero, rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
            //    spriteBatch.Draw(normalTarget, new Vector2(0, screenHeight - screenHeight / 4), rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
            //    spriteBatch.Draw(positionTarget, new Vector2(screenWidth - screenWidth / 4, 0), rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
            //    spriteBatch.Draw(lightTarget, new Vector2(screenWidth - screenWidth / 4, screenHeight - screenHeight / 4), rec, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);

            //    spriteBatch.End();
            //}

            //string ft = (frameTime * 1000).ToString("0,####");
            //string fpsStr = "FPS " + fps + "|" + updatefps;
            //var pc = " PC " + NetworkManager.players.Count;
            //var p = NetworkManager.localPlayer;
            //var camStr = string.Format("({0:F2}, {1:F2}, {2:F2})", p.position.X, p.position.Y, p.position.Z);
            //var pos = " camlocal " + camStr;
            //var pNetStr = "";
            //var pos2 = "";
            //if (NetworkManager.players.Count > 0)
            //{
            //    var player2 = NetworkManager.players[0];
            //    var pNet = player2.position;

            //    //pNetStr += string.Format("({0:F2}, {1:F2}, {2:F2})", pNet.X, pNet.Y, pNet.Z);

            //    //pos2 = " player2 " + pNetStr;
            //    pos2 = " player2Cache " + player2.netDataCache.Count + " ";
            //}

            //fpsStr += pos + pos2 + NetworkManager.Client.RTT + " ms, in " + packetsIn;
            //fpsStr += " out " + packetsOut;

            //fpsStr += " diff " + string.Format("({0:F2}, {1:F2}, {2:F2})", NetworkManager.posDiff.X, NetworkManager.posDiff.Y, NetworkManager.posDiff.Z);
            ////fpsStr += "MP " + meshPartDrawCount;
            ////fpsStr += lightsManager.lights.Count + " " + lightsManager.lightsToDraw.Count;
            //fpsStr += "selected " + selectedVertexIndex;
            if (dTime >= 0.1f)
            {
                dTime = 0;
                var cam = game.camera.position;
                var pos = NetworkManager.localPlayer.position;
                fpsStr = "FPS " + FPS + " RTT " + NetworkManager.Client.RTT + " ms ";
                //fpsStr += string.Format(" ({0:F2}, {1:F2}, {2:F2})", cam.X, cam.Y, cam.Z);
                //fpsStr += string.Format(" ({0:F2}, {1:F2}, {2:F2})", pos.X, pos.Y, pos.Z);
                //fpsStr += string.Format(" ({0:F2}, {1:F2}, {2:F2})", dist.X, dist.Y, dist.Z);
                //fpsStr += $" tri {closeEnoughC}";
                //fpsStr += $" mp {meshPartDrawCount}";

            }
            game.spriteBatch.Begin();
            game.spriteBatch.DrawString(game.fontSmall, fpsStr, Vector2.Zero, Color.White);
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

            game.boundingBox = new BoundingBox(min, max);
            //gizmos.DrawCube(Matrix.CreateScale(2f) * , Color.Magenta);
            game.gizmos.DrawCube(new Vector3(4, 2, 4), new Vector3(2, 2, 2), Color.Magenta);

        }
        int meshPartDrawCount = 0;


        void DrawDust2()
        {
            meshPartDrawCount = 0;
            //game.basicModelEffect.SetTech("basic_color");
            //game.basicModelEffect.SetTech("number");
            game.basicModelEffect.SetTech("colorTex_lightEn");
            game.basicModelEffect.SetTiling(new Vector2(1f));
            game.basicModelEffect.SetKA(.3f);
            game.basicModelEffect.SetKD(.8f);
            game.basicModelEffect.SetKS(.8f);
            game.basicModelEffect.SetShininess(10f);

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
                    game.basicModelEffect.SetColorTexture(game.dust2Tex[partindex]);

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
    }
}

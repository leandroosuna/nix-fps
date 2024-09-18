using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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
        float oneSec = 0f;
        public StateRun() : base()
        {
            inputManager = new InputGameRun();
        }
        public override void OnSwitch()
        {
            System.Windows.Forms.Cursor.Position = inputManager.center;
            game.camera.pitch = 0;
            game.IsMouseVisible = game.CFG["MouseVisible"].Value<bool>();
        }
        List<PointLight> miniLights = new List<PointLight>();
        float jumpForce = 10f * 2;
        float gravity = -9.81f * 4f;
        float verticalVelocity = 0f;


        List<float> hitBuffer0 = new List<float>();
        List<float> hitBuffer1 = new List<float>();
        List<float> hitBuffer2 = new List<float>();
        List<float> hitBuffer3 = new List<float>();
        List<float> hitBuffer4 = new List<float>();
        public int closeEnoughC;

        public bool onAir = false;
        public float airTime = 0f;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            NetworkManager.InterpolatePlayers(game.mainStopwatch.ElapsedMilliseconds);

            game.gizmos.UpdateViewProjection(game.camera.view, game.camera.projection);


            //camera.Update(inputManager);
            game.animationManager.Update(gameTime);
            game.UpdatePointLights(uDeltaTimeFloat);

            game.lightsManager.Update(uDeltaTimeFloat);
            game.gunManager.Update(gameTime);
            game.hud.Update(uDeltaTimeFloat);

            game.camera.Update(inputManager);
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
            //if (onAir && !jumping)
            //{
            //    game.localPlayer.position.Y -= uDeltaTimeFloat * 15f;
            //    airTime += uDeltaTimeFloat;

            //    if(airTime > 3f)
            //    {
            //        airTime = 0;
            //        onAir = false;

            //        game.localPlayer.position = GetSafeLocation();


            //    }
            //}

            //if(!onAir && inputManager.clientInputState.Jump)
            //{
            //    jumping = true;
            //    onAir = true;
            //}
            //if(jumping && jumpTimer <= jumpDuration)
            //{
            //    jumpTimer += uDeltaTimeFloat;
            //    game.localPlayer.position.Y += uDeltaTimeFloat * 20f * 1.5f/(jumpDuration/jumpTimer);
            //    if(jumpTimer >= jumpDuration)
            //    {
            //        jumpTimer = 0f;
            //        jumping = false;
            //    }
            //}

            if (!onAir && inputManager.clientInputState.Jump)
            {
                // Apply jump force
                verticalVelocity = jumpForce;
                onAir = true;  // Player is now in the air
            }

            // Apply gravity if the player is in the air
            if (onAir)
            {
                // Reduce the vertical velocity by gravity
                verticalVelocity += gravity * uDeltaTimeFloat;

                // Update the player's Y position
                game.localPlayer.position.Y += verticalVelocity * uDeltaTimeFloat;

                airTime += uDeltaTimeFloat;
                if (airTime > 5f)
                {
                    airTime = 0;
                    onAir = false;

                    game.localPlayer.position = GetSafeLocation();
                }
                // Check if the player has landed (collision with the ground)
                //if (game.localPlayer.position.Y <= groundHeight)  // Assuming groundHeight is the Y level of the ground
                //{
                //    // Reset player's position to the ground level
                //    game.localPlayer.position.Y = groundHeight;

                //    // Player is no longer in the air
                //    onAir = false;

                //    // Reset vertical velocity
                //    verticalVelocity = 0f;
                //}
            }
            ShowPointingAt();
            MapCollision();

            foreach (var l in miniLights)
                game.lightsManager.Register(l);
        }
        
        
        float DistanceSqrNoY(Vector3 v1, Vector3 v2)
        {
            Vector2 v21 = new Vector2(v1.X, v1.Z);
            Vector2 v22 = new Vector2(v2.X, v2.Z);

            return Vector2.DistanceSquared(v21, v22);
        }
        Vector3 GetSafeLocation()
        {
            return game.mapTriangles
                      .FindAll(t => DistanceSqrNoY(t.v[0],game.localPlayer.position) < 100f)
                      .OrderByDescending(t => DistanceSqrNoY(t.v[0], game.localPlayer.position)).ToList()[0].v[0];
        }
       
        

        void MapCollision()
        {
            var closeEnough = game.mapTriangles
                .FindAll(t => Vector3.DistanceSquared(t.v[0], game.localPlayer.position) < 500f)
                .OrderByDescending(t => Vector3.DistanceSquared(t.v[0], game.localPlayer.position));
            closeEnoughC = closeEnough.Count();

            var flatFrontDir = game.localPlayer.frontDirection;
            flatFrontDir.Y = 0;
            flatFrontDir.Normalize();

            var avgDelta = 1f;

            hitBuffer0.Clear();
            hitBuffer1.Clear();
            hitBuffer2.Clear();
            hitBuffer3.Clear();
            hitBuffer4.Clear();


            //Ray forwardRay = new Ray(game.localPlayer.position, -Vector3.Up);
            foreach (var triangle in closeEnough)
            {
                

                //bool hit = BoundingVolumesExtensions.IntersectRayTriangle(triangle.v[0], triangle.v[1], triangle.v[2], camRay);

                Vector3? hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(0, 4, 0), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    hitBuffer0.Add(hitPos.Value.Y);
                    var pl = new PointLight(triangle.v[0], 20f, Color.White.ToVector3(), Color.White.ToVector3());
                    pl.skipDraw = true;
                    pl.hasLightGeo = true;
                    miniLights.Add(pl);
                }


                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(avgDelta, 4, avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    hitBuffer1.Add(hitPos.Value.Y);
                    var pl = new PointLight(hitPos.Value, 20f, Color.White.ToVector3(), Color.White.ToVector3());
                    pl.skipDraw = true;
                    pl.hasLightGeo = true;
                    miniLights.Add(pl);
                }

                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(avgDelta, 4, -avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    hitBuffer2.Add(hitPos.Value.Y);
                    var pl = new PointLight(hitPos.Value, 20f, Color.White.ToVector3(), Color.White.ToVector3());
                    pl.skipDraw = true;
                    pl.hasLightGeo = true;
                    miniLights.Add(pl);
                }

                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(-avgDelta, 4, avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    hitBuffer3.Add(hitPos.Value.Y);
                    var pl = new PointLight(hitPos.Value, 20f, Color.White.ToVector3(), Color.White.ToVector3());
                    pl.skipDraw = true;
                    pl.hasLightGeo = true;
                    miniLights.Add(pl);
                }

                hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(
                    game.localPlayer.position + new Vector3(-avgDelta, 4, -avgDelta), -Vector3.Up, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    hitBuffer4.Add(hitPos.Value.Y);
                    var pl = new PointLight(hitPos.Value, 20f, Color.White.ToVector3(), Color.White.ToVector3());
                    pl.skipDraw = true;
                    pl.hasLightGeo = true;
                    miniLights.Add(pl);
                }

            }

            if (hitBuffer0.Count > 0 && hitBuffer1.Count > 0 && hitBuffer2.Count > 0 && hitBuffer3.Count > 0 && hitBuffer4.Count > 0)
            {
                var pH = hitBuffer0.OrderByDescending(h => h).ToList()[0];
                float[] H = { 
                    hitBuffer1.OrderByDescending(h => h).ToList()[0], 
                    hitBuffer2.OrderByDescending(h => h).ToList()[0], 
                    hitBuffer3.OrderByDescending(h => h).ToList()[0], 
                    hitBuffer4.OrderByDescending(h => h).ToList()[0] };
                

                
                if(inputManager.clientInputState.Ability1)
                {
                    inputManager.clientInputState.Ability1 = inputManager.clientInputState.Ability1;
                }
                //foreach (var h in H)
                //{
                //    if(h < pH && Math.Abs(h-pH) > 2f)
                //    {
                //        onAir = true;
                //    }
                //}
                var last = game.localPlayer.position.Y;
                var newY = (pH + H[0] + H[1] + H[2] + H[3]) / 5;
                if (last - newY >2f)
                {
                    onAir = true;
                }
                if(!onAir)
                {
                    game.localPlayer.position.Y = newY;
                }
                else
                {
                    if (game.localPlayer.position.Y - pH < 0.05f)
                    {
                        airTime = 0;
                        onAir = false;
                    }
                }
                //var pl = new PointLight(hitPos.Value, 20f, Color.Green.ToVector3(), Color.Green.ToVector3());
                //pl.hasLightGeo = true;
                //pl.skipDraw = true;
            }
        }


        void ShowPointingAt()
        {
            var closeEnough = game.mapTriangles.FindAll(t => game.CheckTriangle(t)).OrderByDescending(t => Vector3.DistanceSquared(t.v[0], game.camera.position));

            Ray camRay = new Ray(game.camera.position, game.camera.frontDirection);
            

            foreach(var triangle in closeEnough)
            {
                //bool hit = BoundingVolumesExtensions.IntersectRayTriangle(triangle.v[0], triangle.v[1], triangle.v[2], camRay);

                Vector3? hitPos = BoundingVolumesExtensions.IntersectRayWithTriangle(game.camera.position, game.camera.frontDirection, triangle.v[0], triangle.v[1], triangle.v[2]);
                if (hitPos.HasValue)
                {
                    var pl = new PointLight(hitPos.Value - game.camera.frontDirection * 1, 20f, Color.White.ToVector3(), Color.White.ToVector3());
                    pl.hasLightGeo = true;
                    //pl.skipDraw = true;
                    miniLights.Add(pl);
                    break;
                }


            }
            
        }
        public override void Draw(GameTime gameTime)
        {
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
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            game.deferredEffect.SetLightMap(game.lightTarget);
            game.deferredEffect.SetScreenSize(new Vector2(game.screenWidth, game.screenHeight));
            game.deferredEffect.SetTech("integrate");
            game.deferredEffect.SetBlurH(game.blurHTarget);
            game.deferredEffect.SetBlurV(game.blurVTarget);

            game.fullScreenQuad.Draw(game.deferredEffect.effect);

            var rec = new Rectangle(0, 0, game.screenWidth, game.screenHeight);

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
            var cam = game.camera.position;
            var pos = NetworkManager.localPlayer.position;
            var fpsStr = "FPS " + FPS + " RTT " + NetworkManager.Client.RTT + " ms";
            //fpsStr += string.Format(" ({0:F2}, {1:F2}, {2:F2})", cam.X, cam.Y, cam.Z);
            fpsStr += string.Format(" ({0:F2}, {1:F2}, {2:F2})", pos.X, pos.Y, pos.Z);
            fpsStr += $" tri {closeEnoughC}";
            fpsStr += $" air {onAir}";
            game.spriteBatch.Begin();
            game.spriteBatch.DrawString(game.font, fpsStr, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(font, str, new Vector2(screenWidth - font.MeasureString(str).X, 0), Color.White);
            game.spriteBatch.End();
            //Gui.Draw(gameTime);
            game.hud.DrawRun(dDeltaTimeFloat);
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

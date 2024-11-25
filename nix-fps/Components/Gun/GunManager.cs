using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Cameras;
using nixfps.Components.Effects;
using nixfps.Components.Input;
using nixfps.Components.Network;
using nixfps.Components.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Gun
{
    public class GunManager
    {
        NixFPS game;
        Camera camera;
        Model gun;
        Matrix gunWorld;
        Texture2D gunTex1;
        BasicModelEffect basicModelEffect;
        List<Tracer> tracers = new List<Tracer>();
        public GunManager()
        {
            game = NixFPS.GameInstance();
            camera = game.camera;
            basicModelEffect = game.basicModelEffect;

            gun = game.Content.Load<Model>(NixFPS.ContentFolder3D + "gun/m16/m16");
            Tracer.model = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/cube");
            Tracer.game = game;
            NixFPS.AssignEffectToModel(gun, basicModelEffect.effect);
            NixFPS.AssignEffectToModel(Tracer.model, basicModelEffect.effect);

            gunTex1 = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "gun/m16/tex/baseColor");
        }

        //float fireRate = .1f;
        float fireRate = .1f;
        float releaseTime = 0;
        float fireRateTimer = 0;

        bool firingAnim = false;
        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

            if(InputManager.keyMappings.Fire.IsDown())
            //if (game.inputManager.clientInputState.Fire)
            {
                if (fireRateTimer == 0)
                {
                    Fire();
                }
                fireRateTimer += elapsedTime;

                if (fireRateTimer >= fireRate)
                {
                    fireRateTimer = 0;
                }
                releaseTime = 0;
            }
            else
            {
                releaseTime += elapsedTime;

                if (releaseTime >= fireRate - fireRateTimer)
                    fireRateTimer = 0;

                firingAnim = false;


            }
            foreach (var tracer in tracers)
            {
                tracer.Update(elapsedTime);
            }
            tracers.RemoveAll(tracer => tracer.life >= tracer.maxLife);


        }
        public (byte location, uint enemyId) hit = (0, uint.MaxValue);
        public void Fire()
        {
            firingAnim = true;
            var pos = camera.position
                        - camera.upDirection * .5f
                        + camera.rightDirection * .6f
                        + camera.frontDirection * 1.6f;

            var dir = camera.frontDirection;
            tracers.Add(new Tracer(Color.Green.ToVector3(), pos, dir));
            Ray ray = new Ray(camera.position, camera.frontDirection);
            
            //check environment 

            //check players
            foreach(var p in NetworkManager.playersToDraw)
            {
                var h = p.Hit(ray);
                if(h > 0)
                {
                    var mapHit = GameStateManager.stateRun.CameraRayHitMap();
                    if (mapHit.HasValue)
                    {
                        var distWall = Vector3.DistanceSquared(mapHit.Value, game.localPlayer.position);
                        var distP = Vector3.DistanceSquared(p.position, game.localPlayer.position);

                        if (distWall < distP)
                        {
                            //Debug.WriteLine($"hit wall {distWall}, {distP}");
                            break;
                        }
                    }
                    hit = (h, p.id);
                    break;
                }
            }

        }
        public void DrawGun(float deltaTime)
        {
            if (camera.isFree)
                return;
            tracers.ForEach(tracers => tracers.Draw());

            var pos = camera.position
                        - camera.upDirection * .5f
                        + camera.rightDirection * .6f
                        + camera.frontDirection * 1f;

            var yaw = camera.yaw;
            var pitch = camera.pitch;

            Random r = new Random();
            if (firingAnim)
            {
                yaw += (float)(r.NextDouble() * 2 - 1) * 1.15f;
                pitch += (float)(r.NextDouble() * 2 - 1) * 1.15f;
                pos += camera.frontDirection * (float)(r.NextDouble() * 2 - 1) * .05f;
            }
            var rot = Matrix.CreateFromYawPitchRoll(-MathHelper.ToRadians(yaw) + MathHelper.PiOver2, -MathHelper.ToRadians(pitch), 0);
            gunWorld = Matrix.CreateScale(.8f) * rot * Matrix.CreateTranslation(pos);

            basicModelEffect.SetTech("colorTex_lightEn");
            basicModelEffect.SetKA(0.3f);
            basicModelEffect.SetKD(0.8f);
            basicModelEffect.SetKS(0.8f);
            basicModelEffect.SetShininess(30f);
            basicModelEffect.SetColor(Color.White.ToVector3());

            basicModelEffect.SetColorTexture(gunTex1);
            basicModelEffect.SetTiling(Vector2.One);
            foreach (var mesh in gun.Meshes)
            {
                if (mesh.Name == "BARREL")//body
                {
                    var w = mesh.ParentBone.Transform * gunWorld;

                    basicModelEffect.SetWorld(w);
                    basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                }
                else if (mesh.Name == "BARREL.001")//mag
                {
                    var w = mesh.ParentBone.Transform * gunWorld;

                    basicModelEffect.SetWorld(w);
                    basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                }
                else if (mesh.Name == "BARREL.002")
                {
                    var w = mesh.ParentBone.Transform * gunWorld;

                    basicModelEffect.SetWorld(w);
                    basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                }


                mesh.Draw();
            }
        }

        public (byte location, byte id) GetEnemyHit()
        {

            return (0, 0);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Audio;
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
        Model[] gunModels;
        Texture2D[] gunsTex;
        Matrix gunWorld;
        BasicModelEffect basicModelEffect;
        List<Tracer> tracers = new List<Tracer>();
        public Gun currentGun;
        public Gun rifle;
        public Gun pistol;
        public GunManager()
        {
            game = NixFPS.GameInstance();
            camera = game.camera;
            basicModelEffect = game.basicModelEffect;

            gunModels = new Model[] {
                game.Content.Load<Model>(NixFPS.ContentFolder3D + "gun/m16/m16"),
                game.Content.Load<Model>(NixFPS.ContentFolder3D + "gun/beretta/BerettaPistol")};

            Tracer.model = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/cube");
            Tracer.game = game;
            foreach (var g in gunModels)
                NixFPS.AssignEffectToModel(g, basicModelEffect.effect);

            NixFPS.AssignEffectToModel(Tracer.model, basicModelEffect.effect);

            gunsTex = new Texture2D[] { 
                game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "gun/m16/tex/baseColor"),
                game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "gun/beretta/BerretaM9_Material_BaseColor"),
            };
            

            var recoilPattern = new List<(float, float)>
            {
                (2.5f, 0.2f), 
                (2.5f, -0.1f),
                (1.6f, 0.3f),
                (3f, -0.2f), 
                (2.5f, -0.2f),
                (2.5f, -0.1f),
                (1.6f, 0.3f),
                (3f, -0.5f),
                (2.5f, -0.7f),
                (2.5f, -.8f),
                (1.6f, 1.4f),
                (3f, -1.5f),
                (2.5f, -0.6f),
                (1.6f, 0.6f),
                (3f, -0.2f), 
                (4.2f, 0.2f),
                (4.3f, -0.1f),
                (4.5f, 0.3f),
                (6f, -0.2f), 
            };
            rifle = new Gun(1,"rifle", 150, 40, 25, true, .1f, 25, recoilPattern);

            recoilPattern = new List<(float, float)>
            {
                (6f, 0.4f), 
                (6f, -0.5f),
                (6f, 0.6f), 
                (6f, -0.7f), 
            };
            pistol = new Gun(2,"pistol", 100, 20, 10, false, .2f, 6, recoilPattern);

            currentGun = rifle;
        }

        
        public bool firedThisFrame = false;
        bool firingAnim;
        
        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

            var fireKey = InputManager.keyMappings.Fire.IsDown();
            var ignoreFireKey = game.camera.isFree || !game.IsActive || GameStateManager.paused;
            
            currentGun.Update(elapsedTime, ignoreFireKey ? false : fireKey);
            firingAnim = false;

            firedThisFrame = currentGun.IsFiring();
            if (firedThisFrame)
                Fire();
            
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

            SoundManager.FireGun(currentGun.name, game.localPlayer);
            //check players
            foreach (var p in NetworkManager.playersToDraw)
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
                    SoundManager.PlayHit();
                    break;
                }
            }
            
        }
        public void ChangeGun(int id)
        {
            switch (id)
            {
                case 1: currentGun = rifle; break;
                case 2: currentGun = pistol; break;
            }
        }

        public void EnemyFire(Player ep, byte id)
        {
            if (id == 0)
                return;
            var gun = game.gunManager.rifle;

            switch(id)
            {
                case 1: gun = game.gunManager.rifle; break;
                case 2: gun = game.gunManager.pistol; break;
            }
            ep.SetFireLight(false);
            if (ep.soundFireTimer == 0)
            {
                SoundManager.FireGun(gun.name, ep);
                ep.SetFireLight(true);
            }
            ep.soundFireTimer += game.gameState.uDeltaTimeFloat;
            if(ep.soundFireTimer >= gun.fireRate)
                ep.soundFireTimer = 0;
            

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

            basicModelEffect.SetTiling(Vector2.One);

            if (currentGun.id == 1)
            {
                basicModelEffect.SetColorTexture(gunsTex[0]);
                foreach (var mesh in gunModels[0].Meshes)
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
            else
            {
                basicModelEffect.SetColorTexture(gunsTex[1]);
                foreach (var mesh in gunModels[1].Meshes)
                {
                    var w = mesh.ParentBone.Transform * gunWorld;
                    basicModelEffect.SetWorld(w);
                    basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                    mesh.Draw();
                }
            }
        }
        public (byte location, byte id) GetEnemyHit()
        {

            return (0, 0);
        }
    }
}

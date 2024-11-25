using Microsoft.Xna.Framework;
using nixfps.Components.Animations.Models;
using nixfps.Components.Collisions;
using nixfps.Components.Effects;
using nixfps.Components.Input;
using nixfps.Components.Network;
using Riptide;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace nixfps
{ 
    public class Player
    {
        public uint id;
        public string name;
        public bool connected = false;

        public Vector3 position = Vector3.Zero;
        public Vector3 positionPrev = Vector3.Zero;
        public Vector3 frontDirection = Vector3.Zero;
        public Vector3 rightDirection = Vector3.Zero;
        public bool onAir = false;
        public float yaw;
        public float pitch = 0f;

        public byte health = 150;
        public byte hitLocation;
        public uint damagerId;

        public Matrix scale = Matrix.CreateScale(0.025f);
        public Matrix world;

        public string clipName;
        public byte clipId;
        
        public string clipPrevName = "idle";
        public byte clipPrevId = 0;

        public string clipNextName;
        public string clipNamePrev;
        public string clipNextNamePrev;

        public byte clipNextId;
        public float timeOffset;
        public List<PlayerCache> netDataCache = new List<PlayerCache>();
        public Vector3 teamColor;
        NixFPS game;

        public BoundingSphere zoneCollider;
        public BoundingCylinder[] bodyCollider;

        public BoundingBox boxCollider;
        public float boxWidth = 2;
        public float boxHeight = 4;

        public bool animBlending;
        public bool animBlendStart;
        public float animBlendFactor;
        public float animBlendTime = .25f;

        public Player(uint id)
        {
            this.id = id;
            name = "noname";
            position = new Vector3(97,8,-205);

            timeOffset = (float)new Random().NextDouble() * 5;
            clipName = "idle";
            clipNamePrev = "idle";
            clipId = 0;
            clipNextName = "";
            clipNextNamePrev = "";
            clipNextId = 1;
            world = scale;
            game = NixFPS.GameInstance();
            zoneCollider = new BoundingSphere(Vector3.Zero, 2.5f);
            
            bodyCollider = new BoundingCylinder[]{
                new BoundingCylinder(Vector3.Zero, .25f, .3f),
                new BoundingCylinder(Vector3.Zero, .5f, .9f),
                new BoundingCylinder(Vector3.Zero, .2f, .45f),
                new BoundingCylinder(Vector3.Zero, .2f, .45f),
                new BoundingCylinder(Vector3.Zero, .2f, .75f),
                new BoundingCylinder(Vector3.Zero, .2f, .45f),
                new BoundingCylinder(Vector3.Zero, .2f, .45f),
                new BoundingCylinder(Vector3.Zero, .2f, .75f),
            };


            boxCollider = new BoundingBox(
                position + new Vector3(-boxWidth / 2, 2 - boxHeight / 2, -boxWidth / 2), 
                position + new Vector3(boxWidth / 2, 2 + boxHeight / 2, boxWidth / 2));
        }
        public void UpdateZoneCollider()
        {
            zoneCollider.Center = position;
            UpdateBoxCollider();
        }
        public void UpdateBoxCollider()
        {
            boxCollider.Min = position + new Vector3(-boxWidth / 2, 3.5f - boxHeight / 2, -boxWidth / 2);
            boxCollider.Max = position + new Vector3(boxWidth / 2, 3.5f + boxHeight / 2, boxWidth / 2);
        }
        public void UpdateBodyColliders(Matrix[] col)
        {
            Vector3 scale;
            Quaternion quat;
            Vector3 translation;
            Matrix rot;

            for (int i = 0; i < col.Length; i++)
            {
                col[i].Decompose(out scale, out quat, out translation);
                rot = Matrix.CreateFromQuaternion(quat);
                
                var color = Color.White;
                switch(i)
                {
                    case 0: 
                        translation += rot.Up * .25f - rot.Forward * 0.1f; //head
                        color = Color.White; break;
                    case 1:
                        translation += rot.Up * .25f - rot.Forward * 0.15f;//body
                        color = Color.Yellow; break;
                    case 2:
                        translation += rot.Up * .25f; //left arm
                        color = Color.Green; break;
                    case 3:
                        translation += rot.Up * .25f; //right arm
                        color = Color.Blue; break;
                    case 4:
                        translation += rot.Up * .6f; //left leg
                        color = Color.Cyan; break; 
                    case 5:
                        translation += rot.Up * .6f; //right leg
                        color = Color.Magenta; break;
                    case 6:
                        translation += rot.Up * .6f; //left leg
                        color = Color.Cyan; break;
                    case 7:
                        translation += rot.Up * .6f; //right leg
                        color = Color.Magenta; break;
                }
                if (lastHit == i)
                    color = Color.Red;
                
                bodyCollider[i].MoveRot(translation, rot);
                game.gizmos.DrawCylinder(translation, rot, new Vector3(bodyCollider[i].Radius, bodyCollider[i].HalfHeight, bodyCollider[i].Radius), color);
            }
        }
        
        public int lastHit = -1;
        public byte Hit(Ray ray)
        {
            for(int i = 0; i < bodyCollider.Length; i++)
            {
                if (bodyCollider[i].Intersects(ray))
                {
                    lastHit = i;
                    switch(i)
                    {
                        case 0: return 1;
                        case 1: return 2;
                        case 2: return 2;
                        case 3: return 2;
                        case 4: return 3;
                        case 5: return 3;
                        case 6: return 3;
                        case 7: return 3;
                    }
                }
            }
            lastHit = -1;
            return 0;
        }
        public Matrix GetWorld()
        {
            world = scale * 
                //Matrix.CreateRotationX(MathF.PI / 2) *
                Matrix.CreateFromYawPitchRoll(-MathHelper.ToRadians(yaw) + MathHelper.PiOver2, 0, 0) *
                Matrix.CreateTranslation(position);

            return world;
        }
        Vector3 tempFront;
        public Vector3 FrontDir()
        {

            tempFront.X = MathF.Cos(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));
            tempFront.Y = MathF.Sin(MathHelper.ToRadians(pitch));
            tempFront.Z = MathF.Sin(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));
            frontDirection = Vector3.Normalize(tempFront);

            return frontDirection;
        }

        

        public void Interpolate(long now)
        {
            game.playerCacheMutex.WaitOne();
            if (netDataCache.Count < 2)
            {
                game.playerCacheMutex.ReleaseMutex();
                return;
            } 
            
            netDataCache = netDataCache.OrderBy(pc => pc.timeStamp).ToList();

            //rendering 4 server ticks behind, 20ms
            
            long renderTimeStamp = now - 20;
            
            ////check if there is at least one timestamp after renderTimeStamp to interpolate to
            if (netDataCache.All(pc => pc.timeStamp <= renderTimeStamp))
            {
                game.playerCacheMutex.ReleaseMutex();
                return;
            }
            //if (InputManager.keyMappings.TAB.IsDown())
            //{
            //    var a = 0;
            //}
            int indexFound = 0;
            for (int i = 0; i < netDataCache.Count - 1; i++)
            {
                var t0 = netDataCache[i].timeStamp;
                var t1 = netDataCache[i + 1].timeStamp;
                if (t0 <= renderTimeStamp && renderTimeStamp < t1)
                {
                    indexFound = i;
                    var x0 = netDataCache[i].position;
                    var x1 = netDataCache[i + 1].position;

                    var y0 = netDataCache[i].yaw;
                    var y1 = netDataCache[i + 1].yaw;

                    var p0 = netDataCache[i].pitch;
                    var p1 = netDataCache[i + 1].pitch;

                    position = x0 + (x1 - x0) * (renderTimeStamp - t0) / (t1 - t0);
                    
                    yaw = y0 + (y1 - y0) * (renderTimeStamp - t0) / (t1 - t0);
                    pitch = p0 + (p1 - p0) * (renderTimeStamp - t0) / (t1 - t0);
                }
            }

            //game.animationManager.SetClipName(this, netDataCache[indexFound].clipId);
            SetClip(netDataCache[indexFound].clipId);

            //we delete elements that are old
            if (netDataCache.Count > 2)
            {
                netDataCache.RemoveRange(0, indexFound);
            }
            game.playerCacheMutex.ReleaseMutex();
        }
        public void UpdateBlend(float deltaTime)
        {
            if (animBlending)
            {
                animBlendFactor += deltaTime / animBlendTime;
                if (animBlendFactor >= 1.0f)
                {
                    animBlendFactor = 0;
                    animBlending = false;


                    clipNamePrev = clipName;
                    clipPrevId = clipId;

                }
            }
        }
        void SetClip(byte newClipId)
        {
            (byte id, String name) = game.animationManager.GetClip(newClipId);

            if (id != clipId) //change detected
            {
                if(!animBlending) //not blending yet
                {
                    animBlending = true;
                    animBlendFactor = 0;
                }
                else //already blending
                {
                    if (id == clipPrevId) //returning to last
                    {
                        animBlendFactor = 1 - animBlendFactor;
                    }
                    else //switching to another
                    {
                        animBlendFactor = 0; 
                    }
                }
                
                clipPrevName = clipName;
                clipPrevId = clipId;

                clipName = name;
                clipId = id;
            }
        }
    }
    
    public class PlayerCache
    {
        public Vector3 position;
        public float yaw, pitch;
        public byte clipId;
        public long timeStamp;
        public byte health;
        public PlayerCache(Vector3 position, float yaw, float pitch, byte clipId, byte health, long timeStamp)
        {
            this.position = position;
            this.yaw = yaw;
            this.pitch = pitch;
            this.clipId = clipId;
            this.timeStamp = timeStamp;
            this.health = health;
        }
    }
}

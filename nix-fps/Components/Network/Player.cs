using Microsoft.Xna.Framework;
using nixfps.Components.Animations.Models;
using nixfps.Components.Collisions;
using nixfps.Components.Effects;
using nixfps.Components.Input;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


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
        
        public Matrix scale = Matrix.CreateScale(0.025f);
        public Matrix world;
        public string clipName;
        public byte clipId;
        public string clipNextName;
        public string clipNamePrev;
        public string clipNextNamePrev;

        public byte clipNextId;
        public float timeOffset;
        public List<PlayerCache> netDataCache = new List<PlayerCache>();
        public Vector3 teamColor;
        NixFPS game;

        public BoundingSphere zoneCollider;
        public BoundingCylinder headCollider;
        public BoundingCylinder bodyCollider;
        public BoundingBox boxCollider;
        public float boxWidth = 2;
        public float boxHeight = 4;
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
            headCollider = new BoundingCylinder(Vector3.Zero, .25f, .34f);
            bodyCollider = new BoundingCylinder(Vector3.Zero, .5f, .75f);


            boxCollider = new BoundingBox(
                position + new Vector3(-boxWidth / 2, 2 - boxHeight / 2, -boxWidth / 2), 
                position + new Vector3(boxWidth / 2, 2 + boxHeight / 2, boxWidth / 2));
        }
        public void UpdateBoxCollider()
        {
            boxCollider.Min = position + new Vector3(-boxWidth / 2, 3.5f - boxHeight / 2, -boxWidth / 2);
            boxCollider.Max = position + new Vector3(boxWidth / 2, 3.5f + boxHeight / 2, boxWidth / 2);
        }
        public void UpdateColliders() 
        {
            zoneCollider.Center = position + new Vector3(0, 2.4f, 0); 
            
            headCollider.Center = position + new Vector3(0, 4.3f, 0);
            bodyCollider.Center = position + new Vector3(0, 3.15f, 0);
            boxCollider.Min = position + new Vector3(-boxWidth / 2, 2.5f - boxHeight / 2, -boxWidth / 2);
            boxCollider.Max = position + new Vector3(boxWidth / 2, 2.5f + boxHeight / 2, boxWidth / 2);

            Matrix headRotation = Matrix.Identity;
            Matrix bodyRotation = Matrix.Identity;
            
            var correctedYaw = -MathHelper.ToRadians(yaw) + MathHelper.PiOver2;
            var colliderPitch = 0f;

            var dir = Vector3.Zero;
                
            switch (clipId)
            {
                case (byte)PlayerAnimation.idle:break;
                case (byte)PlayerAnimation.runForward: 
                    colliderPitch -= 20;
                    dir = Vector3.Normalize(frontDirection + rightDirection * 0.2f);
                    headCollider.Center += dir * 0.6f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);
                    break;
                
                case (byte)PlayerAnimation.runRight: 
                    correctedYaw -= (MathHelper.PiOver4 + .001f);  colliderPitch -= 20;
                    dir = Vector3.Normalize(frontDirection * 0.8f + rightDirection);
                    headCollider.Center += dir * 0.6f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);
                    break;

                case (byte)PlayerAnimation.runLeft:
                    correctedYaw -= (MathHelper.PiOver4 + .001f); colliderPitch -= 20;
                    dir = Vector3.Normalize(frontDirection * 0.8f + rightDirection);
                    headCollider.Center += dir * 0.6f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);

                    break;
                case (byte)PlayerAnimation.runForwardRight: 
                    correctedYaw -= MathHelper.PiOver4; colliderPitch -= 20;
                    dir = Vector3.Normalize(frontDirection + rightDirection);
                    headCollider.Center += dir * 0.6f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);
                    break;
                case (byte)PlayerAnimation.runForwardLeft:
                    correctedYaw += MathHelper.PiOver4;
                    colliderPitch -= 7;
                    dir = Vector3.Normalize(frontDirection - rightDirection * 0.15f);
                    headCollider.Center += dir * 0.4f + new Vector3(0, -.3f, 0);
                    bodyCollider.Center += dir * 0.20f + new Vector3(0, -.15f, 0);
                    break;
                case (byte)PlayerAnimation.sprintForward:
                    colliderPitch -= 20;
                    dir = frontDirection;
                    headCollider.Center += dir * 0.9f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);
                    
                    break;
                case (byte)PlayerAnimation.sprintForwardRight:
                    correctedYaw -= MathHelper.PiOver4; colliderPitch -= 20;
                    dir = Vector3.Normalize(frontDirection + rightDirection * .35f);
                    headCollider.Center += dir * 0.9f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);
                    break;
                case (byte)PlayerAnimation.sprintForwardLeft:
                    correctedYaw += MathHelper.PiOver4; colliderPitch -= 20;
                    dir = Vector3.Normalize(frontDirection - rightDirection * .35f);
                    headCollider.Center += dir * 0.9f + new Vector3(0, -.45f, 0);
                    bodyCollider.Center += dir * 0.25f + new Vector3(0, -.15f, 0);
                    break;
                case (byte)PlayerAnimation.runBackward:
                    dir = Vector3.Normalize(frontDirection + rightDirection * 0.9f);
                    headCollider.Center += dir * .3f +new Vector3(0, -.15f, 0);
                    correctedYaw -= MathHelper.PiOver4; colliderPitch -= 15;
                    break;
                case (byte)PlayerAnimation.runBackwardLeft:
                    headCollider.Center += new Vector3(0, -.10f, 0);
                    correctedYaw -= MathHelper.PiOver4; colliderPitch -= 15;
                    break;
                case (byte)PlayerAnimation.runBackwardRight:
                    headCollider.Center += new Vector3(0, -.1f, 0);
                    correctedYaw -= MathHelper.PiOver4; colliderPitch -= 15;
                    break;
            }
            

            colliderPitch = -MathHelper.ToRadians(colliderPitch);
            headRotation = Matrix.CreateFromYawPitchRoll(correctedYaw, colliderPitch, 0);
            bodyRotation = Matrix.CreateFromYawPitchRoll(correctedYaw, colliderPitch, 0);

            headCollider.Rotation = headRotation;
            bodyCollider.Rotation = bodyRotation;

        }
        public int lastHit = -1;
        public bool Hit(Ray ray)
        {
            if(headCollider.Intersects(ray))
            {
                lastHit = 1;
                return true;
            }
            if(bodyCollider.Intersects(ray))
            {
                lastHit = 2;
                return true;
            }
            lastHit = -1;
            return false;
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
                return;
            
            netDataCache = netDataCache.OrderBy(pc => pc.timeStamp).ToList();

            //rendering one server tick behind, 5ms
            long renderTimeStamp = now - 50;

            ////rendering 20ms behind
            //long renderTimeStamp = now - 100;


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

            game.animationManager.SetClipName(this, netDataCache[indexFound].clipId);
            
            //we delete elements that are old
            if (netDataCache.Count > 2)
            {
                netDataCache.RemoveRange(0, indexFound);
            }
            game.playerCacheMutex.ReleaseMutex();
        }
    }

    public class PlayerCache
    {
        public Vector3 position;
        public float yaw, pitch;
        public byte clipId;
        public long timeStamp;
        public PlayerCache(Vector3 position, float yaw, float pitch, byte clipId, long timeStamp)
        {
            this.position = position;
            this.yaw = yaw;
            this.pitch = pitch;
            this.clipId = clipId;
            this.timeStamp = timeStamp;
        }
    }
}

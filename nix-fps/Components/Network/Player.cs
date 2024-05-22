using Microsoft.Xna.Framework;
using nixfps.Components.Input;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Assimp.Metadata;

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
        public float yaw;
        public float pitch = 0f;

        public Matrix scale = Matrix.CreateScale(0.025f);
        public Matrix world;
        public string clipName;
        public float timeOffset;
        public List<PlayerCache> netDataCache = new List<PlayerCache>();
        public Vector3 teamColor;
        NixFPS game;
        public Player(uint id)
        {
            this.id = id;
            name = "noname";
            position = Vector3.Zero;

            timeOffset = (float)new Random().NextDouble() * 5;
            clipName = "idle";

            world = scale;
            game = NixFPS.GameInstance();
        }
        public Matrix GetWorld()
        {
            world = scale * Matrix.CreateRotationX(MathF.PI / 2) *
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

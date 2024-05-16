using Microsoft.Xna.Framework;
using Riptide;
using System;

namespace nixfps
{ 
    public class Player
    {
        public uint id;
        public string name;
        public bool connected = false;

        public Vector3 position = Vector3.Zero;
        public Vector3 frontDirection = Vector3.Zero;
        public float yaw;
        public float pitch = 0f;

        public Matrix scale = Matrix.CreateScale(0.025f);
        public Matrix world;
        public string clipName;
        public float timeOffset;
        public Player(uint id)
        {
            this.id = id;
            name = "noname";
            position = Vector3.Zero;

            timeOffset = (float)new Random().NextDouble() * 5;
            clipName = "idle";

            world = scale;

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
        
    }
}

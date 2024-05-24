using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using nixfps.Components.Lights;

namespace nixfps.Components.Gun
{
    public class Tracer
    {
        public static Model model;
        public static NixFPS game;
        public Vector3 position;
        public Vector3 dir;
        public Vector3 color;
        public float life = 0f;
        public float maxLife;
        PointLight lightEmitter;
        public Tracer(Vector3 color, Vector3 pos, Vector3 dir)
        {
            position = pos;
            this.color = color;
            this.dir = dir;
            maxLife = 2f;
            lightEmitter = new PointLight(position, 15f, color, color);

            game.lightsManager.Register(lightEmitter);
        }
        public void Update(float time)
        {
            position += dir * time * 1200;
            lightEmitter.position = position;

            life += time;
            if (life >= maxLife)
                game.lightsManager.Destroy(lightEmitter);
        }

        public void Draw()
        {
            float pitch = MathF.Atan2(MathF.Sqrt(dir.X * dir.X + dir.Z * dir.Z), dir.Y);
            float yaw = -MathF.Atan2(dir.Z, dir.X) + MathHelper.PiOver2;

            var scale = Matrix.CreateScale(new Vector3(0.0002f, 0.07f, 0.0002f));

            var rot = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);
            var trans = Matrix.CreateTranslation(position);

            game.basicModelEffect.SetTech("color_lightDis");
            game.basicModelEffect.SetColor(color);

            foreach (var mesh in model.Meshes)
            {
                var w = mesh.ParentBone.Transform * scale * rot * trans;
                game.basicModelEffect.SetWorld(w);
                game.basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                mesh.Draw();
            }
        }
    }
}

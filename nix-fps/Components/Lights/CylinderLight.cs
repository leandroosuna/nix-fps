using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nixfps.Components.Effects;

namespace nixfps.Components.Lights
{
    public class CylinderLight : LightVolume
    {
        float radius;
        float length;
        public Matrix world;
        Matrix rotation;
        public CylinderLight(Vector3 position, float radius, float length, Vector3 color, Matrix rot, Vector3 specularColor ) : base(position, color, Vector3.Zero, specularColor)
        {
            this.radius = radius;
            this.length = length;
            this.position = position;
            collider = new BoundingSphere(position, radius);
            rotation = rot;

            world = Matrix.CreateScale(0.001f * radius, 0.001f * length, 0.001f * radius) * rotation * Matrix.CreateTranslation(position);

        }
        
        public override void Draw()
        {

            deferredEffect.SetLightDiffuseColor(color);
            deferredEffect.SetLightSpecularColor(specularColor);
            deferredEffect.SetLightPosition(position);
            deferredEffect.SetRadius(radius);
            deferredEffect.SetLength(length);

            foreach (var mesh in lightCylinder.Meshes)
            {
                deferredEffect.SetWorld(mesh.ParentBone.Transform * world);

                mesh.Draw();
            }
        }
        

        public override void Update()
        {
            collider.Center = position;

            world = Matrix.CreateScale(0.01f * radius, 0.01f * length, 0.01f * radius) * rotation * Matrix.CreateTranslation(position);

        }
    }
}

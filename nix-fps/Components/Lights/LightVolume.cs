using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nixfps.Components.Effects;
using System.Reflection.Metadata;

namespace nixfps.Components.Lights
{
    public abstract class LightVolume
    {
        public static Model sphere;
        public static Model lightSphere;
        public static Model lightCone;
        public static Model cube;
        public static DeferredEffect deferredEffect;
        public static BasicModelEffect basicModelEffect;

        //public static Model cone;

        public Vector3 position;
        public BoundingSphere collider;
        public Vector3 color;
        public Vector3 ambientColor;
        public Vector3 specularColor;

        public bool enabled;
        public bool hasLightGeo;
        public bool skipDraw;
        static NixFPS game;
        public LightVolume(Vector3 position, Vector3 color, Vector3 ambientColor, Vector3 specularColor)
        {
            this.position = position;
            this.color = color;
            this.ambientColor = ambientColor;
            this.specularColor = specularColor;
            enabled = true;
            hasLightGeo = false;
        }
        public static void Init()
        {
            game = NixFPS.GameInstance();
            
            sphere = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/sphere");
            cube = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/cube");
            lightSphere = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/lightSphere");
            lightCone = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/cone");
            
            deferredEffect = game.deferredEffect;
            basicModelEffect = game.basicModelEffect;

            NixFPS.AssignEffectToModel(sphere, basicModelEffect.effect);
            NixFPS.AssignEffectToModel(cube, basicModelEffect.effect);

            NixFPS.AssignEffectToModel(lightSphere, deferredEffect.effect);
            NixFPS.AssignEffectToModel(lightCone, deferredEffect.effect);

            
        }
        
        public abstract void Update();
        public abstract void Draw();

        public void DrawLightGeo()
        {
            if(hasLightGeo)
            {
                basicModelEffect.SetTech("color_lightDis");
                basicModelEffect.SetColor(color);
            
                foreach (var mesh in sphere.Meshes)
                {
                    var w = mesh.ParentBone.Transform * Matrix.CreateScale(0.001f) * Matrix.CreateTranslation(position);
                    basicModelEffect.SetWorld(w);
                    basicModelEffect.SetInverseTransposeWorld(Matrix.Invert(Matrix.Transpose(w)));
                    mesh.Draw();
                }
            }
        }
        

    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Cameras;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Skybox
{
    internal class Skybox
    {
        Model model;
        TextureCube texture;
        Effect effect;
        float size;
        public Skybox()
        {
            
            NixFPS game = NixFPS.GameInstance();
            model = game.Content.Load<Model>(NixFPS.ContentFolder3D + "skybox/cube");
            texture = game.Content.Load<TextureCube>(NixFPS.ContentFolder3D + "skybox/skybox");
            var ef = NixFPS.ContentFolderEffects + "SkyBox";
            effect = game.Content.Load<Effect>(ef);
            size = 50f;
            NixFPS.AssignEffectToModel(model, effect);
        }
        public void Draw(Camera camera)
        {

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        
                        part.Effect.Parameters["world"]?.SetValue(
                            Matrix.CreateScale(size) * Matrix.CreateTranslation(camera.position));
                        part.Effect.Parameters["view"]?.SetValue(camera.view);
                        part.Effect.Parameters["projection"]?.SetValue(camera.projection);
                        part.Effect.Parameters["skyBoxTexture"]?.SetValue(texture);
                        part.Effect.Parameters["cameraPosition"]?.SetValue(camera.position);
                    }
                    mesh.Draw();
                }
            }
        }
    }
}

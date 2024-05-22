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
        Texture2D texture2D;
        Effect effect;
        float size;
        NixFPS game;
        public Skybox()
        {
            
            game = NixFPS.GameInstance();
            model = game.Content.Load<Model>(NixFPS.ContentFolder3D + "basic/skyboxSphere");
            //texture = game.Content.Load<TextureCube>(NixFPS.ContentFolder3D + "skybox/space");
            texture2D = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "skybox/milkyway");

            var ef = NixFPS.ContentFolderEffects + "SkyBox";
            effect = game.Content.Load<Effect>(ef);
            size = 400;
            NixFPS.AssignEffectToModel(model, effect);
            effect.CurrentTechnique = effect.Techniques["sphere2d"];
        }
        public void Draw()
        {
            //effect.Parameters["skyBoxTexture"]?.SetValue(texture);
            effect.Parameters["skyBox2DTexture"]?.SetValue(texture2D);

            effect.Parameters["cameraPosition"].SetValue(game.camera.position);

            foreach (var mesh in model.Meshes)
            {
                effect.Parameters["world"].SetValue(Matrix.CreateScale(size) * Matrix.CreateTranslation(game.camera.position));
                effect.Parameters["view"].SetValue(game.camera.view);
                effect.Parameters["projection"].SetValue(game.camera.projection);
                
                mesh.Draw();
            }
            
        }
    }
}

﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nixfps.Components.Skybox
{
    /// <summary>
    ///     Handles all of the aspects of working with a SkyBox.
    /// </summary>
    public class SkyBox
    {
        /// <summary>
        ///     Creates a new SkyBox
        /// </summary>
        /// <param name="model">The geometry to use for SkyBox.</param>
        /// <param name="texture">The SkyBox texture to use.</param>
        /// <param name="effect">The size of the cube.</param>
        public SkyBox(Model model, TextureCube texture, Effect effect) : this(model, texture, effect, 600)
        {
        }

        /// <summary>
        ///     Creates a new SkyBox
        /// </summary>
        /// <param name="model">The geometry to use for SkyBox.</param>
        /// <param name="texture">The SkyBox texture to use.</param>
        /// <param name="effect">The SkyBox fx to use.</param>
        /// <param name="size">The SkyBox fx to use.</param>
        public SkyBox(Model model, TextureCube texture, Effect effect, float size)
        {
            Model = model;
            Texture = texture;
            Effect = effect;
            Size = size;


        }

        /// <summary>
        ///     The size of the cube, used so that we can resize the box
        ///     for different sized environments.
        /// </summary>
        private float Size { get; }

        /// <summary>
        ///     The effect file that the SkyBox will use to render
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        ///     The actual SkyBox texture
        /// </summary>
        private TextureCube Texture { get; }

        /// <summary>
        ///     The SkyBox model, which will just be a cube
        /// </summary>
        public Model Model { get; set; }

        /// <summary>
        ///     Does the actual drawing of the SkyBox with our SkyBox effect.
        ///     There is no world matrix, because we're assuming the SkyBox won't
        ///     be moved around.  The size of the SkyBox can be changed with the size
        ///     variable.
        /// </summary>
        /// <param name="view">The view matrix for the effect</param>
        /// <param name="projection">The projection matrix for the effect</param>
        /// <param name="cameraPosition">The position of the camera</param>
        /// 
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            Draw(view, projection, cameraPosition, true);
        }
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, bool isDeferred)
        {
            var game = NixFPS.GameInstance();
            Effect.Parameters["isDeferred"].SetValue(isDeferred);
            Effect.Parameters["SkyBoxTexture"].SetValue(Texture);
            Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            foreach (var mesh in Model.Meshes)
            {
                var world = Matrix.CreateScale(Size) * Matrix.CreateTranslation(cameraPosition);
                var wvp = world * view * projection;
                Effect.Parameters["World"].SetValue(world);
                Effect.Parameters["WorldViewProjection"].SetValue(wvp);

                mesh.Draw();
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Lights;
using System;
using System.Collections.Generic;
using System.Text;

namespace nixfps.Components.Effects
{
    public class SkinnedEffect
    {
        public Effect effect;
        EffectParameter world;
        EffectParameter view;
        EffectParameter projection;
        EffectParameter boneTransforms;
        EffectParameter texture;
        EffectParameter normalTexture;
        EffectParameter inverseTransposeWorld;
        EffectParameter cameraPosition;
        EffectParameter lightAmbientColor;
        EffectParameter lightDiffuseColor;
        EffectParameter lightSpecularColor;
        EffectParameter lightPosition;
        EffectParameter KA;
        EffectParameter KD;
        EffectParameter KS;
        EffectParameter shininess;
        public SkinnedEffect(Effect effect)
        {
            this.effect = effect;

            cacheParams();
            
        }
        void cacheParams()
        {
            world = effect.Parameters["world"];
            view = effect.Parameters["view"];
            projection = effect.Parameters["projection"];
            boneTransforms = effect.Parameters["Bones"];
            texture = effect.Parameters["colorTexture"];
            normalTexture = effect.Parameters["normalTexture"];
            inverseTransposeWorld = effect.Parameters["inverseTransposeWorld"];

            lightPosition = effect.Parameters["lightPosition"];
            cameraPosition = effect.Parameters["cameraPosition"];
            lightAmbientColor = effect.Parameters["lightAmbientColor"];
            lightDiffuseColor = effect.Parameters["lightDiffuseColor"];
            lightSpecularColor = effect.Parameters["lightSpecularColor"];
            KA = effect.Parameters["KA"];
            KD = effect.Parameters["KD"];
            KS = effect.Parameters["KS"];
            shininess = effect.Parameters["shininess"];
        }
        public void SetWorld(Matrix world)
        {
            this.world.SetValue(world);
        }
        public void SetView(Matrix view)
        {
            this.view.SetValue(view);
        }
        public void SetProjection(Matrix projection)
        {
            this.projection.SetValue(projection);
        }
        public void SetBoneTransforms(Matrix[] skeleton)
        {
            this.boneTransforms.SetValue(skeleton);
        }
        public void SetTexture(Texture2D texture)
        {
            this.texture.SetValue(texture);
        }
        public void SetNormalTexture(Texture2D texture)
        {
            this.normalTexture.SetValue(texture);
        }
        public void SetTech(string tech)
        {
            effect.CurrentTechnique = effect.Techniques[tech];
        }
        public void SetInverseTransposeWorld(Matrix itw)
        {
            inverseTransposeWorld.SetValue(itw);
        }
        public void SetCameraPosition(Vector3 position)
        {
            cameraPosition.SetValue(position);
        }
        public void SetLightAmbientColor(Vector3 color)
        {
            lightAmbientColor.SetValue(color);
        }
        public void SetLightDiffuseColor(Vector3 color)
        {
            lightDiffuseColor.SetValue(color);
        }
        public void SetLightSpecularColor(Vector3 color)
        {
            lightSpecularColor.SetValue(color);
        }
        public void SetLightPosition(Vector3 position)
        {
            lightPosition.SetValue(position);
        }
        public void SetKA(float value)
        {
            KA.SetValue(value);
        }
        public void SetKD(float value)
        {
            KD.SetValue(value);
        }
        public void SetKS(float value)
        {
            KS.SetValue(value);
        }
        public void SetShininess(float value)
        {
            shininess.SetValue(value);
        }
        public void SetAmbientLight(AmbientLight ambient)
        {
            SetLightDiffuseColor(ambient.color);
            SetLightAmbientColor(ambient.ambientColor);
            SetLightPosition(ambient.position);
            SetLightSpecularColor(ambient.specularColor);
        }
    }
}

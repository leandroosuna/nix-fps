using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nixfps;
using nixfps.Components.Effects;

namespace nixfps.Components.Lights
{
    public class LightsManager
    {
        NixFPS game;
        DeferredEffect effect;

        public List<LightVolume> lights = new List<LightVolume>();

        public List<LightVolume> lightsToDraw = new List<LightVolume>();

        public AmbientLight ambientLight;
        public LightsManager()
        {
            game = NixFPS.GameInstance();
            effect = game.deferredEffect;
            effect.SetScreenSize(new Vector2(game.screenWidth, game.screenHeight));
        }
        //float ang = 0;
        public void Update(float deltaTime)
        {
            lightsToDraw.Clear();
            
            foreach (var l in lights)
            {
                l.Update();

                if(l.enabled && game.camera.FrustumContains(l.collider))
                    lightsToDraw.Add(l);
            }
        }
        public void Draw()
        {

            effect.SetCameraPosition(game.camera.position);
            effect.SetView(game.camera.view);
            effect.SetProjection(game.camera.projection);

            effect.SetAmbientLight(ambientLight);
            effect.SetKA(.25f);
            effect.SetKD(.5f);
            effect.SetKS(.2f);
            effect.SetShininess(5f);

            effect.SetTech("ambient_light");

            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise; 

            game.fullScreenQuad.Draw(effect.effect);

            
            game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise; //remove front side of spheres to be drawn

            lightsToDraw.ForEach(l => {
                if (!l.skipDraw) 
                { 
                    if(l is PointLight)
                        effect.SetTech("point_light");
                    if (l is CylinderLight)
                        effect.SetTech("cylinder_light");
                    l.Draw(); 
                } 
                    
            });
            
        }
        public void DrawLightGeo()
        {
            lightsToDraw.ForEach(l => l.DrawLightGeo());            
        }
        public void Register(LightVolume volume)
        { 
            lights.Add(volume);
        }
        public void Destroy(LightVolume volume)
        {
            lights.Remove(volume);
        }
    }
}

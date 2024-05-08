using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Animations.DataTypes;
using nixfps.Components.Animations.Models;
using nixfps.Components.Animations.PipelineExtension;
using System.Collections.Generic;
using System.Diagnostics;

namespace nixfps.Components.Effects
{
    public class AnimationManager
    {
        public SkinnedEffect effect;
        
        AnimatedModel animatedModel;
        public Texture2D playerModelTex;
        public Texture2D playerModelTex2;

        NixFPS game;

        Dictionary<string, AnimationPlayer> animations;
        CustomPipelineManager manager;
        
        public AnimationManager()
        {
            animations = new Dictionary<string, AnimationPlayer>();
            game = NixFPS.GameInstance();

            var e = game.Content.Load<Effect>(NixFPS.ContentFolderEffects + "skinning");
            effect = new SkinnedEffect(e);
            effect.SetTech("SkinMRT");

            playerModelTex = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1001_Diffuse");
            playerModelTex2 = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1002_Diffuse");

            //custom model build
            var modelName = NixFPS.ContentFolder3D + "soldier/t-pose";

            manager = CustomPipelineManager.CreateCustomPipelineManager(game.CFG);

            animationNames = new List<string> {
                soldierPath  + "idle",
                soldierPath  + "run forward",
                soldierPath  + "run forward right",
                soldierPath  + "run forward left",
                soldierPath + "run backward",
                soldierPath + "run backward right",
                soldierPath + "run backward left",
                soldierPath + "run right",
                soldierPath + "run left",
                soldierPath + "sprint forward",
                soldierPath + "sprint forward right",
                soldierPath + "sprint forward left",

            };
            if (bool.Parse(game.CFG["rebuild-animations"]))
            {
                manager.BuildAnimationContent(modelName);

                BuildAnimations();
            }
            animatedModel = new AnimatedModel(modelName);
            var model = animatedModel.LoadContent(game.Content);
            NixFPS.AssignEffectToModel(model, e);

            CacheAnimations();

        }
        List<string> animationNames;
        string soldierPath = NixFPS.ContentFolder3D + "soldier/";

        void BuildAnimations()
        {
            foreach (var name in animationNames)
                manager.BuildAnimationContent(name);
        }
        void CacheAnimations()
        {
            
            foreach(var name in animationNames)
            {
                var anim = new AnimatedModel(name);
                anim.LoadContent(game.Content);
                var clip = anim.Clips[0];
                var ap = new AnimationPlayer(clip, animatedModel);
                ap.Looping = true;
                animations.Add(name, ap);
            }    
             
        }
        string lastSet = "last";
        public void PlayAnimation(string name)
        {
            if(name != lastSet)
            {
                animatedModel.SetPlayer(animations[soldierPath + name]);
                lastSet = name;
            }

        }
        public void DrawPlayer(Matrix SRT)
        {
            animatedModel.DrawCustom(game,SRT);
        }
        public void Update(GameTime gameTime)
        {
            float pos = animatedModel.Update(gameTime);
            //Debug.WriteLine(pos + " - "+ animatedModel._player.Duration);
        }
    }
}

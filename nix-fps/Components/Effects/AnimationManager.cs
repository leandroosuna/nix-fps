using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using nixfps.Components.Animations.DataTypes;
using nixfps.Components.Animations.Models;
using nixfps.Components.Animations.PipelineExtension;
using nixfps.Components.Network;
using System;
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
        public Texture2D playerEmissiveTex;
        public Texture2D playerEmissiveTex2;
        public Texture2D playerSpecTex;
        public Texture2D playerSpecTex2;

        NixFPS game;

        CustomPipelineManager manager;
        public AnimationPlayer animationPlayer;
        public List<string> animationNames;
        
        public string soldierPath = NixFPS.ContentFolder3D + "soldier/";

        
        public enum PlayerAnimation{
            idle,
            runForward,
            runForwardRight,
            runForwardLeft,
            runBackward,
            runBackwardRight,
            runBackwardLeft,
            runRight,
            runLeft,
            sprintForward,
            sprintForwardRight,
            sprintForwardLeft
        }
        public AnimationManager()
        {
            
            game = NixFPS.GameInstance();

            var e = game.Content.Load<Effect>(NixFPS.ContentFolderEffects + "skinning");
            effect = new SkinnedEffect(e);
            effect.SetTech("SkinMRT");

            playerModelTex = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1001_Diffuse");
            playerModelTex2 = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1002_Diffuse");

            playerEmissiveTex = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1001_Emissive");
            playerEmissiveTex2 = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1002_Emissive");
            playerSpecTex = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1001_Specular");
            playerSpecTex2 = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "soldier/t-pose.fbm/Ch44_1002_Specular");

            //custom model build
            var modelName = NixFPS.ContentFolder3D + "soldier/t-pose";

            manager = CustomPipelineManager.CreateCustomPipelineManager(game.CFG);

            animationNames = new List<string> {
                "idle",
                "run forward",
                "run forward right",
                "run forward left",
                "run backward",
                "run backward right",
                "run backward left",
                "run right",
                "run left",
                "sprint forward",
                "sprint forward right",
                "sprint forward left",
            };
            var rebuild = game.CFG["rebuild-animations"].Value<bool>();
            if (rebuild)
            {
                manager.BuildAnimationContent(modelName);

                BuildAnimations();
            }
            animatedModel = new AnimatedModel(modelName);
            var model = animatedModel.LoadContent();
            NixFPS.AssignEffectToModel(model, e);

            CreateAnimationPlayer();

        }
        

        void BuildAnimations()
        {
            foreach (var name in animationNames)
                manager.BuildAnimationContent(soldierPath + name);
        }
        void CreateAnimationPlayer()
        {
            Dictionary<string, AnimationClip> clipsDic = new Dictionary<string, AnimationClip>();


            foreach (var name in animationNames)
            {
                var anim = new AnimatedModel(soldierPath + name);
                anim.LoadContent();
                clipsDic.Add(name, anim.Clips[0]);
            }

            animationPlayer = new AnimationPlayer(animatedModel, clipsDic);

        }

        public void DrawPlayers()
        {
            
            animatedModel.DrawPlayer(NetworkManager.localPlayer);
            foreach (var p in NetworkManager.players)
                animatedModel.DrawPlayer(p);
            
        }
        public void SetClipName(Player p, byte clipId)
        {


            if (clipId < 0 || clipId > animationNames.Count - 1)
            {
                p.clipName = "idle";
                p.clipId = 0;
                return;
            }
            p.clipName = animationNames[clipId];
            p.clipId = clipId;
        }
        public (byte id, String name) GetClip(byte clipId)
        {
            if (clipId < 0 || clipId > animationNames.Count - 1)
            {
                return (0, "idle");
            }
            return (clipId, animationNames[clipId]);
        }
        public void SetClipName(Player p, PlayerAnimation anim)
        {
            p.clipName = animationNames[(byte)anim];
        }
        public void Update(float deltaTime)
        {
            animationPlayer.Update(deltaTime);
            effect.SetTime(deltaTime);
        }

        
    }
    
    public enum PlayerAnimation
    {
        idle,
        runForward,
        runForwardRight,
        runForwardLeft,
        runBackward,
        runBackwardRight,
        runBackwardLeft,
        runRight,
        runLeft,
        sprintForward,
        sprintForwardRight,
        sprintForwardLeft
    }
}

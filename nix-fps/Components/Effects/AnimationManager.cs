using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using nixfps.Components.Animations.DataTypes;
using nixfps.Components.Animations.Models;
using nixfps.Components.Animations.PipelineExtension;
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

        NixFPS game;

        CustomPipelineManager manager;
        public AnimationPlayer animationPlayer;
        public List<string> animationNames;
        public string soldierPath = NixFPS.ContentFolder3D + "soldier/";

        List<PlayerDrawData> playerDrawData = new List<PlayerDrawData>();

        public AnimationManager()
        {
            
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
            foreach(var p in playerDrawData)
                animatedModel.Draw(p);
        }

        public void SetPlayerData(int id, string clipName)
        {
            var pdd = GetPlayerData(id);
            pdd.clipName = clipName;
        }
        public void SetPlayerData(int id, Matrix SRT)
        {
            var pdd = GetPlayerData(id);
            pdd.SRT = SRT;
        }
        public void SetPlayerData(int id, Matrix SRT, string clipName)
        {
            var pdd = GetPlayerData(id);
            pdd.SRT = SRT;
            pdd.clipName = clipName;
        }
        public PlayerDrawData GetPlayerData(int id)
        {
            if (id >= playerDrawData.Count)
            {
                //TODO: handle edge case where id > count, to not get ghost players and missaligned indeces
                ///      or just dont use this function incorrectly lmao
                //for(int i = 0; i < id;i++)
                //{
                //    //will create ghosts
                //    playerDrawData.Add(new PlayerDrawData(Matrix.Identity, ""));
                //}
                var pdd = new PlayerDrawData(Matrix.Identity, "");
                playerDrawData.Add(pdd);
                return pdd;
            }
            return playerDrawData[id];
        }
        public void Update(float deltaTime)
        {
           animationPlayer.Update(deltaTime);
        }

        
    }
    public class PlayerDrawData
    {
        public Matrix SRT;
        public string clipName;
        public float timeOffset;

        public PlayerDrawData(Matrix SRT, string clipName)
        { 
            this.SRT = SRT; 
            this.clipName = clipName;
            timeOffset = (float)new Random().NextDouble() * 5;
        }
    }
}

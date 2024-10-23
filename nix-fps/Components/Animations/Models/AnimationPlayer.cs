using Microsoft.Xna.Framework;
using nixfps.Components.Animations.DataTypes;
using nixfps.Components.Effects;
using nixfps.Components.Lights;
using nixfps.Components.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace nixfps.Components.Animations.Models;

public class Clip
{
    public AnimationClip clip;
    public float position;
    public string name;
    public BoneInfo[] boneInfo;
    public Clip(string name, AnimationClip clip)
    {
        this.clip = clip;
        this.position = 0f;
        this.name = name;
        
    }
    public void Update(float deltaTime)
    {
        position += deltaTime;
        if (position >= clip.Duration)
        //if (position >= .5)
            position = 0;
    }
}

public class AnimationPlayer
{
    Clip[] clips;
    Clip activeClip, targetClip;
    float blendTime = 0.5f; // Time to blend between animations
    float blendProgress = 0f;
    public AnimationPlayer(AnimatedModel model, Dictionary<string, AnimationClip> clps )
    {
        
        clips = new Clip[clps.Keys.Count];
        int i = 0;
        foreach (var key in clps.Keys) {
            
            var actualClip = clps[key];
            clips[i] = new Clip(key, actualClip);
            clips[i].name = key;
            var boneCount = actualClip.Bones.Count;
            clips[i].boneInfo = new BoneInfo[boneCount];

            for (var b = 0; b < clips[i].boneInfo.Length; b++)
            {
                clips[i].boneInfo[b] = new BoneInfo(actualClip.Bones[b]);
                clips[i].boneInfo[b].SetModel(model);
            }
            i++;
        }
    }
    public void Update(float deltaTime)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].Update(deltaTime);
        }
        if(blending)
        {
            blendFactor += deltaTime * 2f;
            if (blendFactor >= 1f )
            {
                blendFactor = 0;
                blending = false;
                NetworkManager.localPlayer.clipName = NetworkManager.localPlayer.clipNextName;
                NetworkManager.localPlayer.clipNextName = "";
                NetworkManager.localPlayer.clipNextNamePrev = "";

                for (int i = 0; i < 400; i++)
                {
                    kf1[i] = 0;
                    kf2[i] = 0;
                }
            }
        }
        
    }
    public float blendFactor = 0f;
    public bool blendStart = false;
    public bool blending = false;

    int[] kf2 = new int[400];
    int[] kf1 = new int[400];

    public Clip sclip, sclipnext;
    public float pos;
    public (Quaternion r, Vector3 t, int kf) rp;
    public (Quaternion r, Vector3 t, int kf) rp2;

    public void SetActiveClip(Player player)
    {
        Clip clip = FindClip(player.clipName);
        if (clip == null)
        {
            return;
        }
        Clip clipNext = FindClip(player.clipNextName);

        sclip = clip;
        sclipnext = clipNext;

        // clip idle, next run, nextprev e
        // start blend, nextprev = run
        // clip idle, next run, nextprev run (blend = .5)
        // clip idle, next idle, nextprev run
        // clip run, next idle, nextprev run -> start blend
        // clip run, next idle, nextprev idle -> continue
        // 

        if (clipNext != null)
        {
            if (player.clipName != player.clipNextName)
            {
                if (player.clipNextNamePrev != player.clipNextName)
                {
                   
                    blendStart = true;
                    blendFactor = 0;
                    player.clipNextNamePrev = player.clipNextName;
                }
            }
            else
            {
                if (player.clipNextNamePrev != player.clipNextName && blending)
                {
                    player.clipName = player.clipNextNamePrev;
                    player.clipNextNamePrev = player.clipNextName;

                    blendStart = true;
                    blendFactor = 1 - blendFactor;
                }
            }
        }

        //apply an offset for different models, random at their creation
        pos = (clip.position + player.timeOffset) % (float) clip.clip.Duration;
        var pos2 = 0f;

         

        int len = clip.boneInfo.Length;

        if (blendStart) //sync keyframes and pos
        {
            for (int i = 0; i < 400; i++)
            {
                kf1[i] = 0;
                kf2[i] = 0;
            }
            blendStart = false;
            blending = true;
            //clip.position = 0;
            
            //pos = 0;


        }
        if(blending)
        {
            len = Math.Min(clip.boneInfo.Length, clipNext.boneInfo.Length);

            pos2 = (clipNext.position + player.timeOffset) % (float)clipNext.clip.Duration;
        }
        

   

        //if (blending)
        //    pos %= 0.5f;
        //else
        //    pos %= (float)clip.clip.Duration;

        //string str;
        //for (int i = 0; i < 328; i++)
        //{
        //    str = ""+i+" "+clips[8].clip.Bones[i].Name;

        //    if(i <= 302)
        //        str += " | " + i + " " + clips[9].clip.Bones[i].Name;

        //    Debug.WriteLine(str);
        //}

        for (int i = 0; i < len; i++)
        {
            var bone1 = clip.boneInfo[i];
            if (bone1.ClipAnimationBone.Keyframes.Count < 2)
            {
                continue;
            }
            rp = bone1.CalculateRotPos(pos, 0);
            kf1[i] = rp.kf;
            
            if (blending)
            {
                
                var bone2 = clipNext.boneInfo[i];
                if (bone2.ClipAnimationBone.Keyframes.Count < 2)
                {
                    continue;
                }
                rp2 = bone2.CalculateRotPos(pos2, 0);
                kf2[i] = rp2.kf;

                rp.r = Quaternion.Slerp(rp.r, rp2.r, blendFactor);
                rp.t = Vector3.Lerp(rp.t, rp2.t, blendFactor);

            }

            bone1.SetRotPos(rp.r, rp.t);
            
            //var game = NixFPS.GameInstance();
            //if (i == (int)game.boneIndex)
            //{
            //    var ppos = NetworkManager.localPlayer.position;
            //    var pl = new PointLight(ppos + bone1._assignedBone.GetAbsoluteTransform().Translation * 0.025f, 5f, Vector3.One, Vector3.One);
            //    pl.skipDraw = true;
            //    pl.hasLightGeo = true;
            //    game.testLights.Add(pl);
            //    game.testLights.ForEach(l => game.lightsManager.Register(l));
            //}
        }

        //foreach (var bone in clip.boneInfo)
        //{
        //    if (bone.ClipAnimationBone.Keyframes.Count > 1)
        //    {
        //        bone.SetPosition(pos);
        //    }
        //}
    }

    Clip FindClip(string name)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == name)
            {
                return clips[i];
            }
        }
        return null;
    }
    
}
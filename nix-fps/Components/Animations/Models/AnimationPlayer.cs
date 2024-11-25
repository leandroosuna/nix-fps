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

        //testing, delete fps only or leave in for 3rd person
        //NetworkManager.localPlayer.UpdateBlend(deltaTime);

        foreach (var p in NetworkManager.players)
            p.UpdateBlend(deltaTime);
        
    }
    //public float blendFactor = 0f;
    //public bool blendStart = false;
    //public bool blending = false;

    //public Clip sclip, sclipnext;
    public float pos;
    public (Quaternion r, Vector3 t, int kf) rp;
    public (Quaternion r, Vector3 t, int kf) rp2;

    //public void SetActiveClip(Player player)
    //{
    //    Clip clip = FindClip(player.clipName);
    //    if (clip == null)
    //    {
    //        return;
    //    }
    //    Clip clipNext = FindClip(player.clipNextName);


    //    if (clipNext != null)
    //    {
    //        if (player.clipName != player.clipNextName)
    //        {
    //            if (player.clipNextNamePrev != player.clipNextName)
    //            {
                   
    //                player.animBlendStart= true;
    //                player.animBlendFactor = 0;
    //                player.clipNextNamePrev = player.clipNextName;
    //            }
    //        }
    //        else
    //        {
    //            if (player.clipNextNamePrev != player.clipNextName && player.animBlending)
    //            {
    //                player.clipName = player.clipNextNamePrev;
    //                player.clipNextNamePrev = player.clipNextName;

    //                player.animBlendStart = true;
    //                player.animBlendFactor = 1 - player.animBlendFactor;
    //            }
    //        }
    //    }

    //    //apply an offset for different models, random at their creation
    //    pos = (clip.position + player.timeOffset) % (float) clip.clip.Duration;
    //    var pos2 = 0f;

         

    //    int len = clip.boneInfo.Length;

    //    if (player.animBlendStart) //sync keyframes and pos
    //    {
    //        //for (int i = 0; i < 400; i++)
    //        //{
    //        //    kf1[i] = 0;
    //        //    kf2[i] = 0;
    //        //}
    //        player.animBlendStart = false;
    //        player.animBlending = true;
    //        //clip.position = 0;
            
    //        //pos = 0;


    //    }
    //    if(player.animBlending)
    //    {
    //        len = Math.Min(clip.boneInfo.Length, clipNext.boneInfo.Length);

    //        pos2 = (clipNext.position + player.timeOffset) % (float)clipNext.clip.Duration;
    //    }
        

    //    for (int i = 0; i < len; i++)
    //    {
    //        var bone1 = clip.boneInfo[i];
    //        if (bone1.ClipAnimationBone.Keyframes.Count < 2)
    //        {
    //            continue;
    //        }
    //        rp = bone1.CalculateRotPos(pos, 0);
    //        //kf1[i] = rp.kf;
            
    //        if (player.animBlending)
    //        {
                
    //            var bone2 = clipNext.boneInfo[i];
    //            if (bone2.ClipAnimationBone.Keyframes.Count < 2)
    //            {
    //                continue;
    //            }
    //            rp2 = bone2.CalculateRotPos(pos2, 0);
    //            //kf2[i] = rp2.kf;

    //            rp.r = Quaternion.Slerp(rp.r, rp2.r, player.animBlendFactor);
    //            rp.t = Vector3.Lerp(rp.t, rp2.t, player.animBlendFactor);

    //        }

    //        bone1.SetRotPos(rp.r, rp.t);
            
    //    }
    //}

    public void SetActiveClip(Player p)
    {
        Clip clipTo = FindClip(p.clipName);
        Clip clipFrom = FindClip(p.clipPrevName);

        var posTo = 0f;
        int len = clipTo.boneInfo.Length;
        posTo = (clipTo.position + p.timeOffset) % (float)clipTo.clip.Duration;


        if (p.animBlending)
        {
            len = Math.Min(clipFrom.boneInfo.Length, clipTo.boneInfo.Length);
            var posFrom = (clipFrom.position + p.timeOffset) % (float)clipFrom.clip.Duration;

        }

        for (int i = 0; i < len; i++)
        {
            var boneTo = clipTo.boneInfo[i];
            if (boneTo.ClipAnimationBone.Keyframes.Count < 2)
            {
                continue;
            }
            rp2 = boneTo.CalculateRotPos(posTo, 0);

            if (p.animBlending)
            {
                var bone1 = clipFrom.boneInfo[i];
                if (bone1.ClipAnimationBone.Keyframes.Count < 2)
                {
                    continue;
                }
                rp = bone1.CalculateRotPos(pos, 0);


                rp2.r = Quaternion.Slerp(rp.r, rp2.r, p.animBlendFactor);
                rp2.t = Vector3.Lerp(rp.t, rp2.t, p.animBlendFactor);
            }

            boneTo.SetRotPos(rp2.r, rp2.t);
        }

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
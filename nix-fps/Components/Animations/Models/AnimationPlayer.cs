using Microsoft.Xna.Framework;
using nixfps.Components.Animations.DataTypes;
using nixfps.Components.Effects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            position = 0;
    }
}

public class AnimationPlayer
{
    Clip[] clips;

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
    }

    public void SetActiveClip(Player player)
    {
        Clip clip = null;
        float pos;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == player.clipName) { 
                clip = clips[i];
                break;
            }
        }
        if (clip != null)
        {
            //apply an offset for different models, random at their creation
            pos = clip.position;
            pos += player.timeOffset;
            pos %= (float) clip.clip.Duration;

            foreach (var bone in clip.boneInfo)
            {
                bone.SetPosition(pos);
            }
        }
    }
}
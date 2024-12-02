using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using nixfps.Components.Network;
using System;
using System.Collections.Generic;

namespace nixfps.Components.Audio
{
    public static class SoundManager
    {
        static NixFPS game = NixFPS.GameInstance();
        //static audio
        static SoundEffect soundRifle;
        static SoundEffect soundPistol;
        static SoundEffect soundFootsteps;
        static SoundEffectInstance hit;

        static AudioListener listener;
        static SoundEffectInstance[] footsteps; 
        static AudioEmitter[] soundEmitters;
        static List<(SoundEffectInstance, AudioEmitter, uint)> effectsBeingPlayed = new List<(SoundEffectInstance, AudioEmitter, uint)>();

        static SoundEffectInstance soundKill;
        static SoundEffectInstance soundDeath;
        static SoundEffectInstance soundStreak;
        static SoundEffectInstance soundDamaged;
        public static void LoadContent()
        {
            soundRifle = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "rifle");
            soundPistol = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "pistol");
            soundFootsteps = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "step-sand");
            listener = new AudioListener();
            listener.Up = Vector3.UnitY;
            
            var kill = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "kill");
            var death = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "death");
            var streak = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "streak");
            var damaged = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "minecraft-hit");

            soundKill = kill.CreateInstance();
            soundKill.IsLooped = false;
            soundKill.Volume = .3f;
            soundDeath = death.CreateInstance();
            soundDeath.IsLooped = false;
            soundDeath.Volume = .3f;
            soundStreak = streak.CreateInstance();
            soundStreak.IsLooped = false;
            soundStreak.Volume = .3f;
            soundDamaged = damaged.CreateInstance();
            soundDamaged.IsLooped = false;
            soundDamaged.Volume = .3f;


            var hitef = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "hit");
            hit = hitef.CreateInstance();
            hit.Volume = .5f;

            footsteps = new SoundEffectInstance[64];
            soundEmitters = new AudioEmitter[64];
            for(int i = 0; i < footsteps.Length; i++)
            {
                var instance = soundFootsteps.CreateInstance();
                instance.IsLooped = true;
                footsteps[i] = instance;
                var emitter = new AudioEmitter();
                soundEmitters[i] = emitter;
            }

            
        }
        public static void PlayKill(uint kills)
        {
            if(soundStreak == null || soundKill == null)
            {
                return;
            }
            if (kills % 10 == 0)
                soundStreak.Play();
            else
                soundKill.Play();
        }
        public static void PlayDamaged(uint hp)
        {
            if (soundDeath == null || soundDamaged == null)
            {
                return;
            }
            if (hp == 0)
                soundDeath.Play();
            else
            {
                if(soundDamaged.State == SoundState.Playing)
                {
                    soundDamaged.Stop();
                }
                soundDamaged.Play();
            }
        }
        

        public static void PlayHit()
        {
            hit.Play();
        }
        
        public static void Update()
        {
            var lp = NetworkManager.localPlayer;
            listener.Position = lp.position;
            listener.Forward = lp.frontDirection;

            int i = 0;
            //foreach(var p in NetworkManager.players) 
            //{
            //    if (!p.connected)
            //    {
            //        i++;
            //        continue;
            //    }
            //    if(p.footsteps)
            //    {
            //        if(footsteps[i].State != SoundState.Playing)
            //            footsteps[i].Play();
            //        soundEmitters[i].Position = p.position;
            //        effectsBeingPlayed.Add((footsteps[i], soundEmitters[i], p.id));
            //    }
            //    else
            //    {
            //        footsteps[i].Stop();
            //    }
            //    i++;
            //}


            effectsBeingPlayed.RemoveAll(fx => fx.Item1.State == SoundState.Stopped);
            foreach((var fx, var em, var id) in effectsBeingPlayed)
            {
                if (id == lp.id)
                
                    em.Position = lp.position;
                    
                
                else
                    em.Position = NetworkManager.GetPlayerFromId(id).position;
                //em.Position = lp.position + new Vector3(100,0,0);
                
                fx.Apply3D(listener, em);
            }


            

        }


        public static void FireGun(string name, Player p)
        {
            var emitter = new AudioEmitter();
            emitter.Position = p.position;

            var instance = soundRifle.CreateInstance();

            switch(name)
            {
                case "pistol": instance = soundPistol.CreateInstance(); break;
                
            }
            effectsBeingPlayed.Add((instance, emitter, p.id));
            instance.IsLooped = false;
            instance.Apply3D(listener, emitter);
            if (p.id == NetworkManager.localPlayer.id)
                instance.Volume = .5f;
            else
            {
                instance.Volume = 1f;
            }
            instance.Play();
        }

    }
}

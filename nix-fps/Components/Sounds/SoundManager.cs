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


        static SoundEffectInstance hit;

        static AudioListener listener;

        static List<(SoundEffectInstance, AudioEmitter, uint)> effectsBeingPlayed = new List<(SoundEffectInstance, AudioEmitter, uint)>();

        public static void LoadContent()
        {
            soundRifle = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "rifle");
            soundPistol = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "pistol");

            listener = new AudioListener();
            listener.Up = Vector3.UnitY;

            var hitef = game.Content.Load<SoundEffect>(NixFPS.ContentFolderSounds + "hit");
            hit = hitef.CreateInstance();
            hit.Volume = .5f;
            
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

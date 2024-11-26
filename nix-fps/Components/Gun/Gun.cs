using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Gun
{
    public class Gun
    {
        byte damageHead;
        byte damageBody;
        byte damageLeg;

        public bool fullAuto;
        public float fireRate;
        public float fireRateTimer = 0f;
        public bool waitToFire;
        public bool stopAtReset;
        public bool fire = false;

        // Recoil settings
        private int currentRecoilStep = 0;
        private List<(float basePitch, float baseYaw)> fixedRecoilPattern;
        private float accumulatedPitch = 0f;
        private float accumulatedYaw = 0f;
        private bool isFiring = false;

        // Smoothing settings
        private float recoilReturnSpeed = 20f; // Speed of return to the original position
        private float recoilSmoothingSpeed = 60f; // Higher values mean faster smoothing
        // Game instance
        NixFPS game;
        public string name;
        public byte id;

        public uint shotsFired;
        public uint magSize;
        bool lastHeld;

        public bool reload = false;
        public float reloadTimer = 0;
        float reloadTime = 1.5f;

        public Gun(byte id, string name, byte damageHead, byte damageBody, byte damageLeg, bool fullAuto, float fireRate, uint magSize, List<(float, float)> recoil)
        {
            this.id = id;
            this.name = name;
            this.damageHead = damageHead;
            this.damageBody = damageBody;
            this.damageLeg = damageLeg;
            this.fullAuto = fullAuto;
            this.fireRate = fireRate;
            this.magSize = magSize;

            // Get game instance
            game = NixFPS.GameInstance();

            fixedRecoilPattern = recoil;
            
            
        }
        public void Update(float deltaTime, bool fireKeyDown)
        {
            fire = false;

            if (fireKeyDown && !waitToFire && !reload)
            {
                isFiring = true;

                if (fireRateTimer == 0)
                {
                    if(fullAuto || (!fullAuto && !lastHeld))
                    {
                        if(shotsFired < magSize)
                        {
                            shotsFired++;
                            fire = true;
                            waitToFire = true;
                            ApplyRecoil(ref game.camera.pitch, ref game.camera.yaw);
                        
                            lastHeld = true;
                            if(shotsFired == magSize)
                            {
                                reload = true;
                            }
                        }
                        else
                        {
                            reload = true;
                        }

                    }                    
                }
                
            }
            else
            {
                isFiring = false;
                if(!fireKeyDown)
                    lastHeld = false;
                SmoothReturn(deltaTime, ref game.camera.pitch, ref game.camera.yaw);
            }

            if (waitToFire)
            {
                fireRateTimer += deltaTime;
                if (fireRateTimer >= fireRate)
                {
                    fireRateTimer = 0;

                    waitToFire = false;

                }
            }
            if(reload)
            {
                reloadTimer += deltaTime;
                if(reloadTimer >= reloadTime)
                {
                    reload = false;
                    reloadTimer = 0;
                    shotsFired = 0;
                }
            }    
        }
        

        private void ApplyRecoil(ref float cameraPitch, ref float cameraYaw)
        {
            if (currentRecoilStep < fixedRecoilPattern.Count)
            {
                // Get the base recoil values for this step
                var (basePitch, baseYaw) = fixedRecoilPattern[currentRecoilStep];

                // Adjust camera pitch and yaw
                cameraPitch += basePitch;
                cameraYaw += baseYaw;

                // Accumulate total recoil
                accumulatedPitch += basePitch;
                accumulatedYaw += baseYaw;

                currentRecoilStep++;
            }
            else
            {
                // Reset to start the pattern again
                currentRecoilStep = 0;
            }
        }

        private void SmoothReturn(float deltaTime, ref float cameraPitch, ref float cameraYaw)
        {
            // If there's recoil to correct, gradually return to the original position
            if (Math.Abs(accumulatedPitch) > 0.01f || Math.Abs(accumulatedYaw) > 0.01f)
            {
                float pitchReturn = recoilReturnSpeed * deltaTime;
                float yawReturn = recoilReturnSpeed * deltaTime;

                // Smoothly reduce the accumulated recoil
                float pitchAdjustment = Math.Min(Math.Abs(accumulatedPitch), pitchReturn) * Math.Sign(accumulatedPitch);
                float yawAdjustment = Math.Min(Math.Abs(accumulatedYaw), yawReturn) * Math.Sign(accumulatedYaw);

                cameraPitch -= pitchAdjustment;
                cameraYaw -= yawAdjustment;

                accumulatedPitch -= pitchAdjustment;
                accumulatedYaw -= yawAdjustment;
            }
            else
            {
                currentRecoilStep = 0;
            }
        }

        public bool Fire()
        {
            return fire;
        }
    }

}

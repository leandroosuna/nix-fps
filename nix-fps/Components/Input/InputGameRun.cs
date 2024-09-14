using Microsoft.Xna.Framework;
using nixfps.Components.Cameras;
using nixfps.Components.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    public class InputGameRun : InputManager
    {
        public InputGameRun() : base() 
        { 
            mouseLocked = true;
        }
        public new void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
        Player localPlayer;
        public override void ProcessInput(float deltaTime)
        {
            localPlayer = game.localPlayer;
            camera = game.camera;
            
            if (keyMappings.Escape.IsDown())
            {
                if(!keysDown.Contains(keyMappings.Escape))
                {
                    keysDown.Add(keyMappings.Escape);
                    game.SwitchGameState(NixFPS.GState.MAIN_MENU);
                }
            }
            clientInputState = keyMappings.GetClientState();

            if (!camera.isFree)
            {
                localPlayer.yaw = camera.yaw;
                localPlayer.pitch = camera.pitch;
                localPlayer.frontDirection = camera.frontDirection;
                localPlayer.rightDirection = camera.rightDirection;

                clientInputState.deltaTime = deltaTime;
                ApplyInput(clientInputState);
            }
            else
            {
                var speed = 8;
                if (keyMappings.Sprint.IsDown())
                {
                    speed *= 2;
                }
                if (keyMappings.Forward.IsDown())
                {
                    camera.position += camera.frontDirection * speed * deltaTime;
                }
                if (keyMappings.Backward.IsDown())
                {
                    camera.position -= camera.frontDirection * speed * deltaTime;
                }
                if (keyMappings.Left.IsDown())
                {
                    camera.position -= camera.rightDirection * speed * deltaTime;
                }
                if (keyMappings.Right.IsDown())
                {
                    camera.position += camera.rightDirection * speed * deltaTime;
                }
                if (keyMappings.Jump.IsDown())
                {
                    camera.position.Y += speed * deltaTime;
                }
                if (keyMappings.Crouch.IsDown())
                {
                    camera.position.Y -= speed * deltaTime;
                }
            }

            if (keyMappings.CAPS.IsDown())
            {
                if (!keysDown.Contains(keyMappings.CAPS))
                {
                    keysDown.Add(keyMappings.CAPS);
                    camera.SetFreeToggle();
                }
            }
            if (keyMappings.Debug0.IsDown())
            {
                if (!keysDown.Contains(keyMappings.Debug0))
                {
                    keysDown.Add(keyMappings.Debug0);
                    if(game.selectedVertexIndex < 14500)
                        game.selectedVertexIndex+=20;
                }
            }
            if (keyMappings.Debug9.IsDown())
            {
                if (!keysDown.Contains(keyMappings.Debug9))
                {
                    keysDown.Add(keyMappings.Debug9);
                    if (game.selectedVertexIndex >= 10)
                        game.selectedVertexIndex -= 20;
                }
            }
            //if(keyMappings.Debug0.IsDown())
            //{
            //    if(!keysDown.Contains(keyMappings.Debug0))
            //    {
            //        keysDown.Add(keyMappings.Debug0);
            //        game.hud.crosshair.thickness++;
            //        game.hud.crosshair.modified = true;
            //    }
            //}
            //if (keyMappings.Debug1.IsDown())
            //{
            //    if (!keysDown.Contains(keyMappings.Debug1))
            //    {
            //        keysDown.Add(keyMappings.Debug1);
            //        game.hud.crosshair.thickness--;
            //        game.hud.crosshair.modified = true;
            //    }
            //}
            //if (keyMappings.Debug2.IsDown())
            //{
            //    if (!keysDown.Contains(keyMappings.Debug2))
            //    {
            //        keysDown.Add(keyMappings.Debug2);
            //        game.hud.crosshair.offset++;
            //        game.hud.crosshair.modified = true;
            //    }
            //}
            //if (keyMappings.Debug3.IsDown())
            //{
            //    if (!keysDown.Contains(keyMappings.Debug3))
            //    {
            //        keysDown.Add(keyMappings.Debug3);
            //        game.hud.crosshair.offset--;
            //        game.hud.crosshair.modified = true;
            //    }
            //}
        }

        public override void ApplyInput(ClientInputState state)
        {
            var frontFlat = Vector3.Normalize(new Vector3(localPlayer.frontDirection.X, 0, localPlayer.frontDirection.Z));
            var rightFlat = Vector3.Cross(Vector3.Up, frontFlat);

            Vector3 dir = Vector3.Zero;
            int dz = 0;
            int dx = 0;

            if (state.Forward)
                dz++;
            if (state.Backward)
                dz--;
            if (state.Left)
                dx++;
            if (state.Right)
                dx--;

            dir += (dz * frontFlat + dx * rightFlat);
            speed = 9.5f;
            if (dz > 0 && state.Sprint)
                speed = 18;

            if (dir != Vector3.Zero)
                dir = Vector3.Normalize(dir);

            localPlayer.position += dir * speed * state.deltaTime;
            camera.position = localPlayer.position + new Vector3(0, 4, 0);

        }
    }
}

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
            clientInputState.deltaTime = deltaTime;
            clientInputState.accDeltaTime += deltaTime;
            
            

            if (!camera.isFree)
            {
                localPlayer.yaw = camera.yaw;
                localPlayer.pitch = camera.pitch;
                localPlayer.frontDirection = camera.frontDirection;
                ApplyInput(clientInputState);
            }
            else
            {
                if (keyMappings.Forward.IsDown())
                {
                    camera.position += camera.frontDirection * 5 * deltaTime;
                }
                if (keyMappings.Backward.IsDown())
                {
                    camera.position -= camera.frontDirection * 5 * deltaTime;
                }
                if (keyMappings.Left.IsDown())
                {
                    camera.position -= camera.rightDirection * 5 * deltaTime;
                }
                if (keyMappings.Right.IsDown())
                {
                    camera.position += camera.rightDirection * 5 * deltaTime;
                }
                if (keyMappings.Jump.IsDown())
                {
                    camera.position.Y += 5 * deltaTime;
                }
                if (keyMappings.Crouch.IsDown())
                {
                    camera.position.Y -= 5 * deltaTime;
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
            //animationManager.SetPlayerData(p);

            //var dz = (keyState.IsKeyDown(Keys.Up) ? 1 : 0) - (keyState.IsKeyDown(Keys.Down) ? 1 : 0);
            //var dx = (keyState.IsKeyDown(Keys.Right) ? 1 : 0) - (keyState.IsKeyDown(Keys.Left) ? 1 : 0);

            //if (dz > 0 && dx == 0)
            //{
            //    if (!keyState.IsKeyDown(Keys.RightShift))
            //        animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runForward);
            //    else
            //        animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.sprintForward);
            //}
            //else if (dz > 0 && dx > 0)
            //{
            //    if (!keyState.IsKeyDown(Keys.RightShift))
            //        animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runForwardRight);
            //    else
            //        animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.sprintForwardRight);
            //}
            //else if (dz > 0 && dx < 0)
            //{
            //    if (!keyState.IsKeyDown(Keys.RightShift))
            //        animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runForwardLeft);
            //    else
            //        animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.sprintForwardLeft);
            //}
            //else if (dz < 0 && dx == 0)
            //{
            //    animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runBackward);
            //}
            //else if (dz < 0 && dx > 0)
            //{
            //    animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runBackwardRight);
            //}
            //else if (dz < 0 && dx < 0)
            //{
            //    animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runBackwardLeft);
            //}
            //else if (dz == 0 && dx > 0)
            //{
            //    animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runRight);
            //}
            //else if (dz == 0 && dx < 0)
            //{
            //    animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.runLeft);
            //}

            //else
            //    animationManager.SetClipName(localPlayer, AnimationManager.PlayerAnimation.idle);

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

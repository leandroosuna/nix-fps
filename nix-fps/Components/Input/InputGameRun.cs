using Microsoft.Xna.Framework;
using nixfps.Components.Audio;
using nixfps.Components.Cameras;
using nixfps.Components.Effects;
using nixfps.Components.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoGame.Framework.Content.Pipeline.Builder.PipelineBuildEvent;

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
                if (!keysDown.Contains(keyMappings.Escape))
                {
                    keysDown.Add(keyMappings.Escape);

                    GameStateManager.TogglePause();
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
                var speed = 30;
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
            
        }

        
        private float groundAcceleration = 10f; // acceleration rate on ground
        private float airAcceleration = 4f; // acceleration rate in air
        private float maxSpeed = 18f; // regular speed
        private float sprintSpeed = 25f; // sprint speed
        private Vector3 velocity = Vector3.Zero; // stores current player velocity

        public override void ApplyInput(ClientInputState state)
        {
            var frontFlat = Vector3.Normalize(new Vector3(localPlayer.frontDirection.X, 0, localPlayer.frontDirection.Z));
            var rightFlat = Vector3.Cross(Vector3.Up, frontFlat);

            Vector3 targetDirection = Vector3.Zero;
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

            targetDirection += (dz * frontFlat + dx * rightFlat);

            // Determine target speed
            float targetSpeed = maxSpeed;
            if (dz > 0 && state.Sprint)
                targetSpeed = sprintSpeed;

            // Choose acceleration based on whether the player is in the air
            float acceleration = localPlayer.onAir ? airAcceleration : groundAcceleration;

            if (targetDirection != Vector3.Zero)
                targetDirection = Vector3.Normalize(targetDirection);
            else
                acceleration *= 1.25f; //Slow down faster

            // Calculate desired velocity with acceleration
            Vector3 desiredVelocity = targetDirection * targetSpeed;
            
            velocity = Vector3.Lerp(velocity, desiredVelocity, acceleration * game.gameState.uDeltaTimeFloat);

            localPlayer.position += velocity * game.gameState.uDeltaTimeFloat;
            camera.position = localPlayer.position + new Vector3(0, 4, 0);
        }
    }
}

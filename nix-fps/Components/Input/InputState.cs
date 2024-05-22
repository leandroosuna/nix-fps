using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    public class ClientInputState
    {
        public Vector3 positionDelta;
        public bool Forward;
        public bool Backward;
        public bool Left;
        public bool Right;
        public bool Fire;
        public bool ADS;
        public bool Reload;

        public bool Jump;
        public bool Crouch;
        public bool Sprint;
        public bool Ability1;
        public bool Ability2;
        public bool Ability3;
        public bool Ability4;
        public float deltaTime = 0f;
        public float accDeltaTime = 0f;


        public uint messageId;
        public ClientInputState(bool forward, bool backward, bool left, bool right, bool fire, 
            bool ads, bool reload, bool jump, bool crouch, bool sprint, bool ability1, bool ability2, bool ability3, bool ability4)
        {
            Forward = forward;
            Backward = backward;
            Left = left;
            Right = right;
            Fire = fire;
            ADS = ads;
            Reload = reload;
            Jump = jump;
            Crouch = crouch;
            Sprint = sprint;
            Ability1 = ability1;
            Ability2 = ability2;
            Ability3 = ability3;
            Ability4 = ability4;
        }
        public ClientInputState() { }   
    }
}

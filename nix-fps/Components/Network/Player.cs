using Microsoft.Xna.Framework;
using Riptide;

namespace nixfps
{ 
    public class Player
    {
        public uint id;
        public string name;
        public bool connected = false;

        public Vector3 position = Vector3.Zero;
        public Vector3 frontDirection = Vector3.Zero;
        public float yaw;

        public Player(uint id)
        {
            this.id = id;
            name = "noname";

        }
    }
}

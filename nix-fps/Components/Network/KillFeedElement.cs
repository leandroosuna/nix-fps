using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Network
{
    public class KillFeedElement
    {
        public string p1, p2;
        public byte gun;
        float time = 0f;
        float timeToShow = 4f;
        public bool shouldBeDestroyed = false;

        public KillFeedElement(string p1, string p2, byte gun, float timeToShow)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.gun = gun;
            this.timeToShow = timeToShow;
        }

        public void Update(float deltaTime)
        {
            time += deltaTime; 

            if(time >= timeToShow)
            {
                shouldBeDestroyed = true;
            }
        }
    }
}

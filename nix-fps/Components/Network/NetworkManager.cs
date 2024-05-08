using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Network
{
    internal class NetworkManager
    {
        internal static Client Client { get; set; }

        public static void Connect(string ip)
        {
            Client = new Client();
            Client.ClientDisconnected += (s, e) => Player.List.Remove(e.Id);

            Client.Connect(ip);
        }
    }
}

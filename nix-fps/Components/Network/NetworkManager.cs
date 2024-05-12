using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
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
        public static int playerCount;
        public static List<Vector3> positions = new List<Vector3>();
        static NixFPS game;
        public static void Connect()
        {
            game = NixFPS.GameInstance();
            
            Client = new Client();
            //Client.ClientDisconnected += (s, e) => Player.List.Remove(e.Id);

            var serverIP = game.CFG["ServerIP"].Value<string>();
            Client.Connect(serverIP);

            SendPlayerIdentity();
        }


        [MessageHandler((ushort)MessageId.AllPlayerData)]
        private static void HandleAllPlayerData(Message message)
        {
            positions.Clear();
            playerCount = message.GetInt();
            for (int i = 0; i < playerCount; i++)
            { 
                var pos = message.GetVector3();
                positions.Add(pos);
            }
        }
        public static void SendPlayerIdentity()
        {
            var guid = game.CFG["ClientGUID"].Value<string>();
            var playerName = game.CFG["PlayerName"].Value<string>();

            Message msg = Message.Create(MessageSendMode.Unreliable, MessageId.PlayerIdentity);
            msg.AddString(guid);
            msg.AddString(playerName);
            Client.Send(msg);
        }
        public static void SendData(Vector3 position)
        {
            var msg = Message.Create(MessageSendMode.Unreliable, MessageId.PlayerData);
            msg.AddVector3(position);
            Client.Send(msg);
        }

    }
}

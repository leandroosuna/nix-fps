using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Riptide;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static List<Player> players = new List<Player>();
        static NixFPS game;
        public static uint localPlayerId;
        public static Vector3 netPosition;
        public static void Connect()
        {
            game = NixFPS.GameInstance();

            Client = new Client();
            //Client.ClientDisconnected += (s, e) => Player.List.Remove(e.Id);

            var serverIP = game.CFG["ServerIP"].Value<string>();
            Client.Connect(serverIP);

            SendPlayerIdentity();
        }

        public void Update()
        {

        }
        

        [MessageHandler((ushort)MessageId.PlayerConnected)]
        private static void HandlePlayerConnected(Message message)
        {
            var id = message.GetUInt();
            var name = message.GetString();
            if (id == localPlayerId)
                return;
            Debug.WriteLine(name + " (" + id + ") connected");

            var p = GetPlayerFromId(id, true);
            p.name = name;
            p.connected = true;

        }

        [MessageHandler((ushort)MessageId.PlayerDisconnected)]
        private static void HandlePlayerDisconnected(Message message)
        {
            var id = message.GetUInt();
            var name = message.GetString();

            Debug.WriteLine(name + " (" + id + ") disconnected");

            var p = GetPlayerFromId(id);
            if(p.id != uint.MaxValue)
            {
                p.name = name;
                p.connected = false;
                return;
            }
            Debug.WriteLine("player not found");
        }

        public static Player GetPlayerFromId(uint id, bool createIfNull = false)
        {
            foreach (var player in players)
            {
                if (player.id == id)
                {
                    return player;
                }
            }
            if(createIfNull)
            {
                var p = new Player(id);
                players.Add(p);
                return p;
            }

            return new Player(uint.MaxValue);
        }
        public static void SendPlayerIdentity()
        {
            var id = game.CFG["ClientID"].Value<uint>();
            var playerName = game.CFG["PlayerName"].Value<string>();

            Message msg = Message.Create(MessageSendMode.Reliable, MessageId.PlayerIdentity);
            msg.AddUInt(id);
            msg.AddString(playerName);
            Client.Send(msg);

            var p = new Player(id);
            p.name = playerName;
            players.Add(p);
            localPlayerId = p.id;
        }
        public static void SendData()
        {

            var msg = Message.Create(MessageSendMode.Unreliable, MessageId.PlayerData);
            msg.AddUInt(localPlayerId);
            msg.AddVector3(game.camera.position - new Vector3(0,4,0));
            msg.AddVector3(game.camera.frontDirection);
            msg.AddFloat(game.camera.yaw);

            Client.Send(msg);
        }
        public static bool notFound;
        [MessageHandler((ushort)MessageId.AllPlayerData)]
        private static void HandleAllPlayerData(Message message)
        {
            notFound = false;
            playerCount = message.GetInt();
            for (int i = 0; i < playerCount; i++)
            {
                var id = message.GetUInt();
                var netPos = message.GetVector3();
                var netFD = message.GetVector3();
                var netYaw = message.GetFloat();
                if(id != localPlayerId)
                {
                    var p = GetPlayerFromId(id);
                    if (p.id != uint.MaxValue)
                    {
                        if (p.id != localPlayerId)
                        {
                            p.position = netPos;
                            p.frontDirection = netFD;
                            p.yaw = netYaw;
                            game.animationManager.SetPlayerData(p);
                        }
                        else
                            netPosition = netPos;

                        //
                    }
                    else
                    {
                        notFound = true;
                        //Debug.WriteLine("player not found");
                    }
                }
                
            }
        }


    }
}

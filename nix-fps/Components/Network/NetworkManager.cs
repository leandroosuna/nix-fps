using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using nixfps.Components.Input;
using Riptide;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace nixfps.Components.Network
{

    internal class NetworkManager
    {
        public static Client Client { get; set; }
        public static int playerCount;
        public static List<Vector3> positions = new List<Vector3>();

        public static List<Player> players = new List<Player>();
        public static Player localPlayer;
        
        static NixFPS game;
        public static Vector3 netPosition;

        public static void Connect()
        {
            game = NixFPS.GameInstance();

            Client = new Client();

            var server = game.CFG["ServerIP"].Value<string>();
            var serverIP = Dns.GetHostAddresses(server)[0].ToString();
            serverIP +=":7777";
            Client.Connect(serverIP);
            
            SendPlayerIdentity();
        }

        public static void SendPlayerIdentity()
        {
            var id = game.CFG["ClientID"].Value<uint>();
            var playerName = game.CFG["PlayerName"].Value<string>();

            Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.PlayerIdentity);
            msg.AddUInt(id);
            msg.AddString(playerName);
            Client.Send(msg);

            localPlayer = new Player(id);
            localPlayer.name = playerName;

        }
        public static void SendData()
        {
            var inputMan = game.inputManager;

            inputMan.clientInputState.messageId = inputMan.messagesSent;
            inputMan.InputStateCache.Add(inputMan.clientInputState);
            inputMan.messagesSent++;


            var msg = Message.Create(MessageSendMode.Unreliable, ClientToServer.PlayerData);
            msg.AddUInt(localPlayer.id);
            msg.AddVector3(localPlayer.position);

            msg.AddUInt(inputMan.clientInputState.messageId);
            msg.AddBool(inputMan.clientInputState.Forward);
            msg.AddBool(inputMan.clientInputState.Backward);
            msg.AddBool(inputMan.clientInputState.Left);
            msg.AddBool(inputMan.clientInputState.Right);
            msg.AddBool(inputMan.clientInputState.Sprint);
            msg.AddBool(inputMan.clientInputState.Jump);
            msg.AddBool(inputMan.clientInputState.Crouch);
            msg.AddBool(inputMan.clientInputState.Fire);
            msg.AddBool(inputMan.clientInputState.ADS);
            msg.AddBool(inputMan.clientInputState.Ability1);
            msg.AddBool(inputMan.clientInputState.Ability2);
            msg.AddBool(inputMan.clientInputState.Ability3);
            msg.AddBool(inputMan.clientInputState.Ability4);
            msg.AddFloat(localPlayer.yaw);
            msg.AddFloat(localPlayer.pitch);
            
            msg.AddFloat(inputMan.clientInputState.accDeltaTime);
            inputMan.clientInputState.accDeltaTime = 0;
            Client.Send(msg);
        }

        public static Vector3 posDiff;
        [MessageHandler((ushort)ServerToClient.AllPlayerData)]
        private static void HandleAllPlayerData(Message message)
        {
            playerCount = message.GetInt();
            for (int i = 0; i < playerCount; i++)
            {
                var id = message.GetUInt();
                var lastProcessedMessage = message.GetUInt();
                var netPos = message.GetVector3();
                var netYaw = message.GetFloat();
                var netPitch = message.GetFloat();
                var netClip = message.GetByte();

                if (id != localPlayer.id)
                {
                    var p = GetPlayerFromId(id);
                    if (p.id != uint.MaxValue)
                    {
                        var cache = new PlayerCache(netPos, netYaw, netPitch, netClip, game.mainStopwatch.ElapsedMilliseconds); 
                        
                        p.netDataCache.Add(cache);
                        
                    }
                    else
                    {
                        //Debug.WriteLine("player not found");
                    }
                }
                else
                {
                    //server reconciliation
                    //auth position from server
                    var prevPos = localPlayer.position;

                    localPlayer.position = netPos;
                    var inputMan = game.inputManager;
                    var cache = inputMan.InputStateCache;

                    //we remove all processed messages from the cache
                    cache.RemoveAll(c => c.messageId <= lastProcessedMessage);

                    for(int j = 0; j < cache.Count; j++)
                    {
                        inputMan.ApplyInput(cache[j]);
                        if(InputManager.keyMappings.TAB.IsDown())
                        {
                            var a = 0;
                        }
                    }
                    posDiff = localPlayer.position - prevPos;


                }
            }
        }
        [MessageHandler((ushort)ServerToClient.PlayerConnected)]
        private static void HandlePlayerConnected(Message message)
        {
            var id = message.GetUInt();
            var name = message.GetString();
            if (id == localPlayer.id)
                return;
            Debug.WriteLine(name + " (" + id + ") connected");

            var p = GetPlayerFromId(id, true);
            p.name = name;
            p.connected = true;

        }

        [MessageHandler((ushort)ServerToClient.PlayerDisconnected)]
        private static void HandlePlayerDisconnected(Message message)
        {
            var id = message.GetUInt();
            var name = message.GetString();

            Debug.WriteLine(name + " (" + id + ") disconnected");

            var p = GetPlayerFromId(id);
            if (p.id != uint.MaxValue)
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
            if (createIfNull)
            {
                var p = new Player(id);
                players.Add(p);
                return p;
            }

            return new Player(uint.MaxValue);
        }

        public static void InterpolatePlayers(long now)
        {
            foreach (var player in players)
                player.Interpolate(now);
        }
    }
}

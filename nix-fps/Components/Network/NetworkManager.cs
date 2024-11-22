using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using nixfps.Components.Input;
using Riptide;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;


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
        public static int ConnectionAttempts = 1;
        static String serverIP;
        public static int tick;

        private delegate void TimerCallback(uint id, uint msg, IntPtr user, IntPtr param1, IntPtr param2);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeSetEvent(uint msDelay, uint msResolution, TimerCallback callback, IntPtr user, uint eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeKillEvent(uint uTimerId);


        private static uint timerId;
        static uint TargetMS;
        private static TimerCallback callback;
        public static void Connect()
        {
            game = NixFPS.GameInstance();
            
            var id = game.CFG["ClientID"].Value<uint>();
            
            localPlayer = new Player(id);
            localPlayer.name = "name";
            localPlayer.teamColor = new Vector3(0, 1, 1);

            Client = new Client();

            var server = game.CFG["ServerIP"].Value<string>();
            serverIP = Dns.GetHostAddresses(server)[0].ToString();
            serverIP +=":7777";

            Client.Connect(serverIP);

            Client.ConnectionFailed += Client_ConnectionFailed;
            Client.Connected += Client_Connected;
            Client.Disconnected += Client_Disconnected;
            callback = TimerElapsed;

            TargetMS = 1000 / game.CFG["TPS"].Value<uint>();


            timerId = timeSetEvent(TargetMS, 0, callback, IntPtr.Zero, 1);

        }

        private static void TimerElapsed(uint id, uint msg, IntPtr user, IntPtr param1, IntPtr param2)
        {
            Client.Update();
            if (Client.IsConnected)
            {
                SendData();
                tick++;
            }
            else
            {
                tick--;
            }
        }
        static bool isReconnect = false;
        private static void Client_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("CONNECTED");
            if(isReconnect)
            {
                SendPlayerIdentity();
                isReconnect = false;
            }
        }
        
        private static void Client_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            
            Client.Connect(serverIP);
            ConnectionAttempts++;
            
        }

        private static void Client_Disconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("DISCONNECTED");

            //attempt auto reconnect
            Client.Connect(serverIP);
            isReconnect = true; 
        }

        public static void StopNetThread()
        {
            timeKillEvent(timerId);
        }
        public static void SetLocalPlayerName(string name)
        {
            localPlayer.name = name;
        }
        public static void SendPlayerIdentity()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, ClientToServer.PlayerIdentity);
            msg.AddUInt(localPlayer.id);
            msg.AddString(localPlayer.name);
            Client.Send(msg);  
        }
        public static void SendData()
        {
            var inputMan = game.gameState.inputManager;

            inputMan.clientInputState.messageId = inputMan.messagesSent;
            //inputMan.InputStateCache.Add(inputMan.clientInputState);
            inputMan.messagesSent++;

            inputMan.clientInputState.positionDelta = localPlayer.position - localPlayer.positionPrev;
            localPlayer.positionPrev = localPlayer.position;    


            var msg = Message.Create(MessageSendMode.Unreliable, ClientToServer.PlayerData);
            msg.AddUInt(localPlayer.id);
            msg.AddUInt(inputMan.clientInputState.messageId);

            msg.AddVector3(localPlayer.position);
            msg.AddVector3(inputMan.clientInputState.positionDelta);
            msg.AddFloat(localPlayer.yaw);
            msg.AddFloat(localPlayer.pitch);
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
            
            //msg.AddFloat(inputMan.clientInputState.accDeltaTime);
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
                var connected = message.GetBool();
                
                var p = NetworkManager.localPlayer;
                if (id != localPlayer.id)
                {
                    p = GetPlayerFromId(id, true);
                    p.connected = connected;
                }
                if (connected)
                {
                    var lastProcessedMessage = message.GetUInt();
                    var lastMovementValid = message.GetBool();
                    var netPos = message.GetVector3();
                    var netYaw = message.GetFloat();
                    var netPitch = message.GetFloat();
                    var netClip = message.GetByte();

                    if (id != localPlayer.id)
                    {
                        if (p.id != uint.MaxValue)
                        {
                            var cache = new PlayerCache(netPos, netYaw, netPitch, netClip, game.mainStopwatch.ElapsedMilliseconds);
                            game.playerCacheMutex.WaitOne();
                            p.netDataCache.Add(cache);
                            game.playerCacheMutex.ReleaseMutex();
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

                        //localPlayer.position = netPos;
                        var inputMan = game.gameState.inputManager;
                        var cache = inputMan.InputStateCache;

                        //we remove all processed messages from the cache
                        cache.RemoveAll(c => c.messageId <= lastProcessedMessage);

                        for(int j = 0; j < cache.Count; j++)
                        {
                            //inputMan.ApplyInput(cache[j]);
                            if(InputManager.keyMappings.TAB.IsDown())
                            {
                                var a = 0;
                            }
                        }
                        posDiff = localPlayer.position - prevPos;


                    }
                }

            }
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

        internal static void UpdatePlayers()
        {
            localPlayer.UpdateZoneCollider();
            foreach (var player in players)
                player.UpdateZoneCollider();
        }
    }
}

using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using nixfps.Components.Audio;
using nixfps.Components.Gun;
using nixfps.Components.Input;
using nixfps.Components.States;
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
        public static int playerCount = 0;
        public static List<Vector3> positions = new List<Vector3>();

        public static List<Player> players = new List<Player>();
        public static Player localPlayer;
        public static List<Player> playersToDraw = new List<Player>();
        
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
            localPlayer.teamColor = new Vector3(0, 0, 0);
            localPlayer.position = GameStateManager.stateRun.GetSafeLocation();
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
            msg.AddInt(game.CFG["Version"].Value<int>());
            Client.Send(msg);  
        }
        public static void SendData()
        {
            var inputMan = game.gameState.inputManager;
            var gunMan = game.gunManager;

            
            inputMan.clientInputState.messageId = inputMan.messagesSent;
            //inputMan.InputStateCache.Add(inputMan.clientInputState);
            inputMan.messagesSent++;

            inputMan.clientInputState.positionDelta = localPlayer.position - localPlayer.positionPrev;
            localPlayer.positionPrev = localPlayer.position;
            if (gunMan == null) return;
            var (hitLocation, enemyId) = gunMan.hit;


            var msg = Message.Create(MessageSendMode.Unreliable, ClientToServer.PlayerData);
            msg.AddUInt(localPlayer.id);

            msg.AddUInt(inputMan.clientInputState.messageId);
            msg.AddVector3(localPlayer.teamColor);
            msg.AddVector3(localPlayer.position);
            msg.AddVector3(inputMan.clientInputState.positionDelta);
            msg.AddFloat(localPlayer.yaw);
            msg.AddFloat(localPlayer.pitch);
            msg.AddByte(hitLocation);
            msg.AddByte(game.gunManager.currentGun.id);
            msg.AddUInt(enemyId);
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

            if(hitLocation >0)
            {
                gunMan.hit = (0, uint.MaxValue);
            }

            Client.Send(msg);
        }

        public static Vector3 posDiff;
        [MessageHandler((ushort)ServerToClient.Version)]
        private static void HandleVersion(Message message)
        {
            game.correctVersion = game.CFG["Version"].Value<int>() == message.GetInt();
            game.versionReceived = true;
        }

        [MessageHandler((ushort)ServerToClient.PlayerName)]
        private static void HandlePlayerNames(Message message)
        {
            var count = message.GetUInt();
            for(int i = 0; i < count; i++)
            {
                var id = message.GetUInt();
                var name = message.GetString();

                var p = GetPlayerFromId(id, true);
                p.name = name;

            }
        }

        //public static List<(uint p1, uint p2, byte gun, float time)> killFeed = new List<(uint p1, uint p2, byte gun, float time)>();
        public static List<KillFeedElement> killFeed = new List<KillFeedElement>();
        [MessageHandler((ushort)ServerToClient.KillFeed)]
        private static void HandleKillFeed(Message message)
        {
            var p1 = message.GetUInt();
            var p2 = message.GetUInt();
            var gun = message.GetByte();

            killFeed.Add(new KillFeedElement(GetPlayerFromId(p1).name, GetPlayerFromId(p2).name, gun, 10f));
            
        }


        [MessageHandler((ushort)ServerToClient.AllPlayerData)]
        private static void HandleAllPlayerData(Message message)
        {
            playerCount = message.GetInt();
            for (int i = 0; i < playerCount; i++)
            {
                var id = message.GetUInt();
                var connected = message.GetBool();
                
                var p = localPlayer;
                if (id != localPlayer.id)
                {
                    p = GetPlayerFromId(id, true);
                    p.connected = connected;
                }
                if (connected)
                {
                    var lastProcessedMessage = message.GetUInt();
                    var lastMovementValid = message.GetBool();
                    var color = message.GetVector3();
                    var netPos = message.GetVector3();
                    var netYaw = message.GetFloat();
                    var netPitch = message.GetFloat();
                    var netClip = message.GetByte();
                    var hp = message.GetByte();
                    var hitLocation = message.GetByte();
                    var damagerId = message.GetUInt();
                    var enemyGunId = message.GetByte();
                    var kills = message.GetUInt();
                    var deaths = message.GetUInt();

                    if (id != localPlayer.id)
                    {
                        if (p.id != uint.MaxValue)
                        {
                            var cache = new PlayerCache(netPos, netYaw, netPitch, netClip, hp, enemyGunId, kills, deaths, game.mainStopwatch.ElapsedMilliseconds);
                            game.playerCacheMutex.WaitOne();
                            p.netDataCache.Add(cache);
                            game.playerCacheMutex.ReleaseMutex();
                            p.teamColor = color;
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

                        var prevHp = localPlayer.health;
                        
                        localPlayer.health = hp;
                        if(prevHp > localPlayer.health)
                            SoundManager.PlayDamaged(hp);

                        if(hp == 0)
                        {
                            localPlayer.position = GameStateManager.stateRun.GetSafeLocation();
                            game.gunManager.currentGun.InstantReload();
                        }

                        localPlayer.hitLocation = hitLocation;
                        localPlayer.damagerId = damagerId;

                        var prevKills = localPlayer.kills;
                        localPlayer.kills = kills;
                        if (localPlayer.kills > prevKills)
                            SoundManager.PlayKill(localPlayer.kills);
                        localPlayer.deaths = deaths;
                        //p.hp = hp;
                    }
                }

            }
        }
        public static void SetPlayerColor(Color color)
        {
            if (localPlayer != null) 
                localPlayer.SetColor(color);
        }
        public static Player GetPlayerFromId(uint id, bool createIfNull = false)
        {
            if(id == localPlayer.id) return localPlayer;

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

        internal static void UpdatePlayers(float deltaTime)
        {
            killFeed.ForEach(e => e.Update(deltaTime));
            killFeed.RemoveAll(e => e.shouldBeDestroyed);

            localPlayer.UpdateZoneCollider();
            foreach (var player in players)
                player.UpdateZoneCollider();

            playersToDraw.Clear();

            playersToDraw.AddRange(
                players.FindAll(p => game.camera.FrustumContains(p.zoneCollider)));

        }
    }
}

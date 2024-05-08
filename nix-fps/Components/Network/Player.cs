﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Riptide;
using System.Collections.Generic;

namespace nixfps.Components.Network
{
    internal class Player
    {
        internal static readonly Dictionary<ushort, Player> List = new Dictionary<ushort, Player>();

        private Vector2 position;
        private readonly ushort id;

        internal Player(ushort clientId, Vector2 position)
        {
            id = clientId;
            this.position = position;
        }

        internal void Update(float deltaTime)
        {
            //move to input manager
            KeyboardState keyboard = Keyboard.GetState();
            int h = (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left) ? -1 : 0) + (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right) ? 1 : 0);
            int v = (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up) ? -1 : 0) + (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down) ? 1 : 0);

            Vector2 pos = new Vector2(h, v);
            if (h != 0 && v != 0)
                pos = Vector2.Normalize(pos);

            position += pos * deltaTime * 96;

            SendPosition();
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            //leave in for debug for now
            spriteBatch.Draw(NixFPS.Pixel, position, null, Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 0);
        }

        private void SendPosition()
        {
            //change to vector3
            Message message = Message.Create(MessageSendMode.Unreliable, MessageId.PlayerPosition);
            message.AddVector2(position);

            NetworkManager.Client.Send(message);
        }

        [MessageHandler((ushort)MessageId.PlayerPosition)]
        private static void HandlePosition(Message message)
        {
            ushort id = message.GetUShort();
            Vector2 position = message.GetVector2();

            if (List.TryGetValue(id, out Player player))
                player.position = position;
        }

        [MessageHandler((ushort)MessageId.PlayerSpawn)]
        private static void HandleSpawn(Message message)
        {
            ushort id = message.GetUShort();
            Vector2 position = message.GetVector2();

            List.Add(id, new Player(id, position));
        }
        //add player orientation, fire event
    }
}

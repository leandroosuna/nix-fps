using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using nixfps.Components.Network;
using System;


namespace nixfps.Components.HUD
{
    public class Hud
    {
        public Crosshair crosshair;
        NixFPS game;
        public SpriteBatch spriteBatch;
        Texture2D miniMap;
        Effect mmEffect;
        int baseMapWidth = 513;
        int baseMapHeight = 474;


        public int mapWidth;
        public int mapHeight;
        Rectangle mapTexBounds;
        public bool miniMapEnabled;

        Texture2D pistol, rifle;
        Vector2 sizePistol, sizeRifle;
        public Hud()
        {
            game = NixFPS.GameInstance();
            crosshair = new Crosshair(game);
            miniMap = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D+"dust2/hdtex/minimap");
            mmEffect = game.Content.Load<Effect>(NixFPS.ContentFolderEffects + "minimap");
            pistol = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "gun/beretta/icon");
            rifle = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "gun/m16/icon");

            sizeRifle = new Vector2((int)((1000) * .1f), (int)((363) * .1f));
            sizePistol = new Vector2((int)((650) * .06f), (int)((400) * .06f));


            mmEffect.Parameters["Texture"].SetValue(miniMap);
            //mmEffect.Parameters["PI"].SetValue(MathF.PI);
            mmEffect.Parameters["PIOver2"].SetValue(MathHelper.PiOver2);

            

            ChangeMapSize(.7f);
        }
        float time;
        const int maxPlayers = 64;
        Vector2[] playerPositions = new Vector2[maxPlayers];

        public bool rotateWithPlayerYaw = false;
        public void Update(float deltaTime)
        {
            var lp = game.localPlayer;
            crosshair.Update();
            time += deltaTime;
            mmEffect.Parameters["time"].SetValue(time);
            
            mmEffect.Parameters["rotation"].SetValue(rotateWithPlayerYaw? MathHelper.ToRadians(lp.yaw) : MathHelper.PiOver2);
            
            mmEffect.Parameters["localPlayerPos"].SetValue(GetTextureCoord(lp.position));

            //var count = 1;
            //playerPositions[0] = GetTextureCoord(lp.position + new Vector3(20, 0, 0));
            
            int i = 0;
            foreach(var p in NetworkManager.players)
            {
                if(p.connected)
                {
                    playerPositions[i] = GetTextureCoord(p.position);
                    i++;
                }
            }

            mmEffect.Parameters["playerPositions"].SetValue(playerPositions);
            mmEffect.Parameters["numPlayers"].SetValue(i);
        }

        public void DrawRun(float deltaTime)
        {
            var lp = game.localPlayer;

            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            crosshair.Draw();

            var gun = game.gunManager.currentGun;
            var magStr = $"{gun.magSize - gun.shotsFired}/{gun.magSize}";

            if (gun.reload)
                magStr += " R";


            //DrawMiniMap(deltaTime);

            if(miniMapEnabled)
                spriteBatch.Draw(mapTarget, mapTexBounds, Color.White);

            KillFeed(deltaTime);

            spriteBatch.DrawString(game.fontXLarge, "+", new Vector2(game.screenWidth / 3, game.screenHeight - 55), Color.White);
            spriteBatch.DrawString(game.fontLarge, ""+lp.health, new Vector2(game.screenWidth / 3 + 25, game.screenHeight - 50), Color.White);
            spriteBatch.DrawString(game.fontLarge,
               magStr, new Vector2(game.screenWidth  *2 / 3 + 25, game.screenHeight - 50), Color.White);
            spriteBatch.End();

        }
        
        void KillFeed(float deltaTime)
        {
            var kfl = NetworkManager.killFeed;


            var yPos = 0;
            var xPos = 0;

            
            for(int i = 0; i < kfl.Count; i++)
            {
                var kf = kfl[i];
            
                var p1Name = kf.p1;
                var p2Name = kf.p2;
                var gun = kf.gun;
                var name2Size = game.fontSmall.MeasureString(p2Name);
                var name1Size = game.fontSmall.MeasureString(p1Name);

                xPos = game.screenWidth - (int)name2Size.X;

                spriteBatch.DrawString(game.fontSmall, p2Name, new Vector2(xPos, yPos), Color.White);

                var gunSize = Vector2.Zero;
                var gunIcon = rifle;
                switch(gun)
                {
                    case 1:
                        xPos -= (int)(sizeRifle.X) + 5;
                        gunIcon = rifle;
                        gunSize = sizeRifle;
                        break;
                    case 2:
                        xPos -= (int)(sizePistol.X) + 5;
                        gunIcon = pistol;
                        gunSize = sizePistol; 
                        break;

                }

                var gunRec = new Rectangle(xPos, yPos, (int)gunSize.X, (int)gunSize.Y);
                spriteBatch.Draw(gunIcon, gunRec, Color.White);

                xPos -= (int)(name1Size.X) + 5;

                spriteBatch.DrawString(game.fontSmall, p1Name, new Vector2(xPos, yPos), Color.White);

                yPos += 38;

                
            }

            //var name2 = "player 1";
            //var name1 = "player 2";

            //var name2Size = game.fontSmall.MeasureString(name2);
            //var name1Size = game.fontSmall.MeasureString(name1);


            //var yPos = 0;
            //var xPos = game.screenWidth - (int)name2Size.X;
            //spriteBatch.DrawString(game.fontSmall, name2, new Vector2(xPos, yPos), Color.White);
            //xPos -= (int)(sizeRifle.X) + 5;
            //var rifleRec = new Rectangle(xPos, yPos, (int)sizeRifle.X, (int)sizeRifle.Y);
            //spriteBatch.Draw(rifle, rifleRec, Color.White);
            //xPos -= (int)(name1Size.X) + 5;
            //spriteBatch.DrawString(game.fontSmall, name1, new Vector2(xPos , yPos), Color.White);

            //yPos = 40;
            //xPos = game.screenWidth - (int)name2Size.X;
            //spriteBatch.DrawString(game.fontSmall, name2, new Vector2(xPos, yPos), Color.White);
            //xPos -= (int)(sizePistol.X) + 5;
            //var pistolRec = new Rectangle(xPos, yPos, (int)sizePistol.X, (int)sizePistol.Y);
            //spriteBatch.Draw(pistol, pistolRec, Color.White);
            //xPos -= (int)(name1Size.X) + 5;
            //spriteBatch.DrawString(game.fontSmall, name1, new Vector2(xPos, yPos), Color.White);


        }

        RenderTarget2D mapTarget = new RenderTarget2D(NixFPS.GameInstance().GraphicsDevice,
                513, 474, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);

        float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        Vector2 GetTextureCoord(Vector3 pos)
        {
            float texX = Normalize(pos.X, 119, -149 ); 
            float texY = Normalize(pos.Z, -223, 67);
            return new Vector2(1-texX, texY);
        }

        public void CheckRenderTarget()
        {
            
        }
        public void DrawMiniMapTarget(float deltaTime)
        {
            if(!miniMapEnabled)
            {
                return;
            }
            // var miniRec = new Rectangle(0, 10, w, h);

            game.GraphicsDevice.SetRenderTarget(mapTarget);
            game.GraphicsDevice.Clear(ClearOptions.Target, new Vector4(0,0,0,0), 0f, 0);
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


            game.fullScreenQuad.Draw(mmEffect);


        }
        public void EnableRotation(bool enable)
        {
            rotateWithPlayerYaw = enable;
        }
        public void ChangeMapSize(float scale)
        {
            if(scale <= 0.01)
            {
                miniMapEnabled = false;
                return;
            }
            miniMapEnabled = true;

            mapWidth = (int)(baseMapWidth * scale);
            mapHeight = (int)(baseMapHeight * scale);
            mapTexBounds = new Rectangle(0,0,mapWidth,mapHeight);

            mmEffect.Parameters["width"]?.SetValue(mapWidth);
            mmEffect.Parameters["height"]?.SetValue(mapHeight);

            if (mapTarget.Bounds.Width != mapWidth || mapTarget.Bounds.Height != mapHeight)
            {
                mapTarget = new RenderTarget2D(game.GraphicsDevice,
                    mapWidth, mapHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
            }

            
        }

    }
}

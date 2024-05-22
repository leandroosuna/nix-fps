using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace nixfps.Components.HUD
{
    public class Crosshair
    {
        bool centerDot = true;
        bool enabled = true;
        bool mainLines = true;
        bool outline = true;
        bool centerDotOutline = true;
        bool hit = false;

        Color color = Color.White;
        Color centerDotColor = Color.White;
        Color outlineColor = Color.Black;

        public int centerDotSize = 2;
        public int length = 10;
        public int thickness = 4;
        public int offset = 10;
        public int correction = 0;
        SpriteBatch spriteBatch;
        Point c;
        List<Rectangle> lines = new List<Rectangle>();
        List<Rectangle> outlines = new List<Rectangle>();

        Texture2D pixelTex;
        NixFPS game;
        public bool modified = false;
        public Crosshair(NixFPS game)
        {
            this.game = game;
            pixelTex = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "basic/Tex/white-pixel");
            spriteBatch = game.spriteBatch;
            c = new Point(game.screenWidth / 2 , game.screenHeight / 2 );

            modified = true;


        }
        public void Update()
        {
            if(!modified) return;
            modified = false;
            lines.Clear();
            outlines.Clear();
            
            var thicknessCorrection = (int)Math.Ceiling(thickness / 2.0) - 1;
            var even = (thickness % 2);
            var posCorrection = (1 - even) * 2 + (even) * 1;

            lines.Add(new Rectangle(c - new Point(thicknessCorrection, offset + length - posCorrection), new Point(thickness, length)));
            lines.Add(new Rectangle(c + new Point(-thicknessCorrection, offset), new Point(thickness, length)));
            lines.Add(new Rectangle(c - new Point(offset + length - posCorrection, thicknessCorrection), new Point(length, thickness)));
            lines.Add(new Rectangle(c + new Point(offset, -thicknessCorrection), new Point(length, thickness)));

            var outThick = thickness + 2;
            var outLength = length + 2;
            var outOffset = offset - 1;
            thicknessCorrection = (int)Math.Ceiling(outThick / 2.0) - 1;
            even = (thickness % 2);
            posCorrection = (1 - even) * 2 + (even) * 1;

            outlines.Add(new Rectangle(c - new Point(thicknessCorrection, outOffset + outLength - posCorrection), new Point(outThick, outLength)));
            outlines.Add(new Rectangle(c + new Point(-thicknessCorrection, outOffset), new Point(outThick, outLength)));
            outlines.Add(new Rectangle(c - new Point(outOffset + outLength - posCorrection, thicknessCorrection), new Point(outLength, outThick)));
            outlines.Add(new Rectangle(c + new Point(outOffset, -thicknessCorrection), new Point(outLength, outThick)));

        }
        public void Draw()
        {
            if (enabled)
            {
                if (outline)
                    outlines.ForEach(line => spriteBatch.Draw(pixelTex, line, null, outlineColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f));
                if (mainLines)
                    lines.ForEach(line => spriteBatch.Draw(pixelTex, line, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0.0f));

            }
            if (centerDot)
            {
                //TODO: Apply position correction size >2 for dot and outline
                var centerDotRect = new Rectangle(c, new Point(centerDotSize, centerDotSize));

                if(centerDotOutline)
                {
                    
                    var centerDotOutRect = new Rectangle(c - new Point(1, 1), new Point(centerDotSize + 2, centerDotSize + 2));
                    spriteBatch.Draw(pixelTex, centerDotOutRect, null, outlineColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);

                }
                spriteBatch.Draw(pixelTex, centerDotRect, null, centerDotColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            }
        }

    }
}

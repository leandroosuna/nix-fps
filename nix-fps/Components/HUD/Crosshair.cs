using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace nixfps.Components.HUD
{
    public class Crosshair
    {
        public bool enabled = true;
        public bool centerDot = false;
        public bool mainLines = true;
        public bool outline = true;
        bool centerDotOutline = true;
        bool hit = false;

        Color color = Color.White;
        Color centerDotColor = Color.White;
        Color outlineColor = Color.Black;
            
        public int centerDotSize = 2;
        public int length = 4;
        public int thickness = 2;
        public int outlineThickness = 1;
        public int offset = 4;
        public int correction = 0;
        public SpriteBatch spriteBatch;
        Point c;
        List<Rectangle> lines = new List<Rectangle>();
        List<Rectangle> outlines = new List<Rectangle>();

        Texture2D pixelTex;
        NixFPS game;
        public bool modified = false;
        //JObject slots;
        JArray slots;
        int currentSlot = 0;
        const string slotsFilePath = "crosshair-slots.json";
        public Crosshair(NixFPS game)
        {
            this.game = game;
            pixelTex = game.Content.Load<Texture2D>(NixFPS.ContentFolder3D + "basic/Tex/white-pixel");
            slots = JArray.Parse(File.ReadAllText(slotsFilePath));

            //Debug.WriteLine(outlineColor.PackedValue.ToString()); //{R:255 G:255 B:255 A:255} white 4294967295  black 4278190080



            Center();
        }
        public void Center(int offsetX = 0, int offsetY = 0)
        {
            c = new Point(game.screenWidth / 2, game.screenHeight / 2);
            int dw = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int dh = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            if(game.screenWidth > dw || game.screenHeight > dh)
            {
                c = new Point(dw/2, dh/2);
            }
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

            var outThick = thickness + outlineThickness * 2;
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

                if (mainLines)
                {
                    if (outline)
                        outlines.ForEach(line => spriteBatch.Draw(pixelTex, line, null, outlineColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f));
                    lines.ForEach(line => spriteBatch.Draw(pixelTex, line, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0.0f));
                }

                if (centerDot)
                {

                    var correction = (int)Math.Ceiling(centerDotSize / 2.0) - 1;

                    var centerDotRect = new Rectangle(c - new Point(correction), new Point(centerDotSize, centerDotSize));

                    if (outline)
                    //if(centerDotOutline)
                    {

                        var centerDotOutRect =
                            new Rectangle(c - new Point(correction) - new Point(outlineThickness, outlineThickness),
                            new Point(centerDotSize + 2 * outlineThickness, centerDotSize + 2 * outlineThickness));
                        spriteBatch.Draw(pixelTex, centerDotOutRect, null, outlineColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);

                    }
                    spriteBatch.Draw(pixelTex, centerDotRect, null, centerDotColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                }
            }
        }
        public void SetSlotActive(int slot)
        {
            if (slot < 0 || slot >= 10)
                return;
            currentSlot = slot;
            enabled = slots[slot]["Enabled"].Value<bool>();
            centerDot = slots[slot]["CenterDot"].Value<bool>();
            mainLines = slots[slot]["MainLines"].Value<bool>();
            outline = slots[slot]["Outline"].Value<bool>();
            centerDotSize = slots[slot]["CenterDotSize"].Value<int>();
            length= slots[slot]["Length"].Value<int>();
            thickness = slots[slot]["Thickness"].Value<int>();
            outlineThickness = slots[slot]["OutlineThickness"].Value<int>();
            offset = slots[slot]["Offset"].Value<int>();
            color = new Color(slots[slot]["Color"].Value<uint>());
            centerDotColor = new Color(slots[slot]["CenterDotColor"].Value<uint>());
            outlineColor = new Color(slots[slot]["OutlineColor"].Value<uint>());
        }
        public void SaveFile()
        {
            slots[currentSlot]["Enabled"] = enabled;
            slots[currentSlot]["CenterDot"] = centerDot;
            slots[currentSlot]["MainLines"] = mainLines;
            slots[currentSlot]["Outline"] = outline;
            slots[currentSlot]["CenterDotSize"] = centerDotSize;
            slots[currentSlot]["Length"] = length;
            slots[currentSlot]["Thickness"] = thickness;
            slots[currentSlot]["OutlineThickness"] = outlineThickness;
            slots[currentSlot]["Offset"] = offset;
            slots[currentSlot]["Color"] = color.PackedValue;
            slots[currentSlot]["CenterDotColor"] = centerDotColor.PackedValue;
            slots[currentSlot]["OutlineColor"] = outlineColor.PackedValue;

            File.WriteAllText(slotsFilePath, slots.ToString());
        }
        public int GetSlotActive()
        {
            return currentSlot;
        }
        public Color GetColor()
        {
            
            return new Color(slots[currentSlot]["Color"].Value<uint>());
        }
        public Color GetDotColor()
        {
            return new Color(slots[currentSlot]["CenterDotColor"].Value<uint>());
        }
        public Color GetOutlineColor()
        {

            return new Color(slots[currentSlot]["OutlineColor"].Value<uint>());
        }
        public bool GetEnableLines()
        {
            return slots[currentSlot]["MainLines"].Value<bool>();
        }
        public bool GetEnableOutline()
        {
            return slots[currentSlot]["Outline"].Value<bool>();
        }
        public bool GetEnableDot()
        {
            return slots[currentSlot]["CenterDot"].Value<bool>();
        }
        public bool GetEnabled()
        {
            return slots[currentSlot]["Enabled"].Value<bool>();
        }

        public int GetLineLength()
        {
            return slots[currentSlot]["Length"].Value<int>();
        }
        public int GetLineThickness()
        {
            return slots[currentSlot]["Thickness"].Value<int>(); 
        }
        public int GetLineOffset()
        {
            return slots[currentSlot]["Offset"].Value<int>();
        }
        public int GetOutlineThickness()
        {
            return slots[currentSlot]["OutlineThickness"].Value<int>();
        }
        public int GetDotSize()
        {
            return slots[currentSlot]["CenterDotSize"].Value<int>();
        }
        public void SetColor(Color color)
        {
            this.color = color;
            modified = true;
        }
        public void SetColorOutline(Color color)
        {
            this.outlineColor = color;
            modified = true;
        }
        public void SetDotColor(Color color)
        {
            centerDotColor = color;
            modified = true;
        }
        public void SetLength(float lenght)
        { 
            this.length = (int)lenght; 
            modified = true;
        }
        public void SetThickness(float thickness)
        { 
            this.thickness = (int)thickness; 
            modified = true; 
        }
        public void SetThicknessOutline(float thickness)
        {
            outlineThickness = (int)thickness;
            modified = true;
        }

        public void SetOffset(float offset)
        {
            this.offset = (int)offset;
            modified = true;
        }
        public void SetDotSize(float dotSize)
        {
            this.centerDotSize = (int)dotSize; 
            modified = true;
        }

        
        public void SetEnableLines(bool en)
        {
            mainLines = en;
        }
        public void SetEnabled(bool en)
        {
            enabled = en;
        }
        public void SetEnableOutline(bool en)
        {
            outline = en;
        }
        public void SetEnableDot(bool en)
        {
            centerDot = en;
        }


    }
}

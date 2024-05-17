using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    
    public class KeyMappings
    {
        public Key Enter;
        public Key Escape;

        public Key Forward;
        public Key Backward;
        public Key Left;
        public Key Right;
        public Key Fire;
        public Key ADS;
        public Key Reload;

        public Key Jump;
        public Key Crouch;
        public Key Sprint;
        public Key Ability1;
        public Key Ability2;
        public Key Ability3;
        public Key Ability4;

        public List<Key> MappedKeys;

        public Key Debug1, Debug2, Debug3, Debug0, Debug7, Debug8, Debug9;
        public Key TAB, CAPS;
        public Key ConvertKey(Keys key)
        {
            //Keys.F20 = MB1
            //Keys.F21 = MB2
            //Keys.F22 = MB3
            //Keys.F23 = Scroll Down
            //Keys.F24 = Scroll Up

            if (key == Keys.F20)
            {
                return new MouseKey(MouseButton.Left);
            }
            if (key == Keys.F21)
            {
                return new MouseKey(MouseButton.Right);
            }
            if (key == Keys.F22)
            {
                return new MouseKey(MouseButton.Middle);
            }
            if (key == Keys.F23)
            {
                return new ScrollWheel(true);
            }
            if (key == Keys.F24)
            {
                return new ScrollWheel(false);
            }
            //if(key == Keys.None)
            //{

            //}
            
            return new KeyboardKey(key);
        }
        public KeyMappings(JsonKeys keys)
        {
            Enter = ConvertKey(keys.KeyEnter);
            Escape = ConvertKey(keys.KeyEscape);
            Forward = ConvertKey(keys.KeyForward);
            Backward = ConvertKey(keys.KeyBackward);
            Left = ConvertKey(keys.KeyLeft);
            Right = ConvertKey(keys.KeyRight);
            Fire = ConvertKey(keys.KeyFire);
            ADS = ConvertKey(keys.KeyADS);
            Reload = ConvertKey(keys.KeyReload);
            Jump = ConvertKey(keys.KeyJump);
            Crouch = ConvertKey(keys.KeyCrouch);
            Sprint = ConvertKey(keys.KeySprint);
            Ability1 = ConvertKey(keys.Ability1);
            Ability2 = ConvertKey(keys.Ability2);
            Ability3 = ConvertKey(keys.Ability3);
            Ability4 = ConvertKey(keys.Ability4);


            MappedKeys = new List<Key>();
            MappedKeys.AddRange(new List<Key>()
            {
                Enter, Escape,
                Forward, Backward, Left, Right, Fire, ADS, Reload, Jump, Crouch, Sprint, 
                Ability1, Ability2, Ability3, Ability4
            });

        }

        
    }
    public class JsonKeys
    {
        public Keys KeyEnter { get; set; }
        public Keys KeyEscape { get; set; }
        public Keys KeyForward { get; set; }
        public Keys KeyBackward { get; set; }
        public Keys KeyLeft { get; set; }
        public Keys KeyRight { get; set; }
        public Keys KeyFire { get; set; }
        public Keys KeyADS{ get; set; }
        public Keys KeyReload { get; set; }
        public Keys KeySprint { get; set; }
        public Keys KeyJump { get; set; }
        public Keys KeyCrouch { get; set; }
        public Keys Ability1 { get; set; }
        public Keys Ability2 { get; set; }
        public Keys Ability3 { get; set; }
        public Keys Ability4 { get; set; }


        public static JsonKeys LoadFromJson(string filePath)
        {
            // Read JSON file content
            string jsonContent = File.ReadAllText(filePath);

            // Deserialize JSON to KeyMappings object
            JsonKeys jsonKeys = JsonConvert.DeserializeObject<JsonKeys>(jsonContent);


            return jsonKeys;
        }
        public void SaveToJson(string filePath)
        {
            string jsonContent = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, jsonContent);
        }

        public void UpdateKey(string propertyName, Keys newKey)
        {
            // Use reflection to find the property by name
            var property = typeof(KeyMappings).GetProperty(propertyName);

            if (property != null && property.PropertyType == typeof(Keys))
            {
                // Set the new key value
                property.SetValue(this, newKey);
            }
            else
            {
                throw new ArgumentException("Invalid property name or type.");
            }
        }
        // Modify key mappings (e.g., user presses a key in the game)
        //keyMappings.UpdateKey("KeyAccelerate", Keys.Space);

        // Save the updated key mappings to JSON file
        //keyMappings.SaveToJson("path/to/your/keymappings.json");
    }
}

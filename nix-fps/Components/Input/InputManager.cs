using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using nixfps.Components.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nixfps.Components.Input
{
    public abstract class InputManager
    {
        public NixFPS game;
        public static List<Key> keysDown = new List<Key>();
        public static KeyboardState keyState;
        public static MouseState mouseState;

        public static Camera camera;
        
        public float mouseSensitivity = .15f;
        public float mouseSensAdapt = .09f;
        public System.Drawing.Point center;
        Vector2 delta;
        Vector2 mousePosition;
        public Vector2 mouseDelta;
        public bool mouseLocked = false;
        public static KeyMappings keyMappings;

        public float speed;
        public const int stateCacheSize = 1024;
        public ClientInputState clientInputState = new ClientInputState();
        public List<ClientInputState> InputStateCache = new List<ClientInputState>();
        //public ServerState[] serverStateCache = new ServerState[stateCacheSize];
        //public ServerState serverState;
        public uint messagesSent = 0;

        public InputManager()
        {
            game = NixFPS.GameInstance();
            
            center = new System.Drawing.Point(game.screenCenter.X, game.screenCenter.Y);

        }
        public static void Init()
        {
            var fileCfg = "input-settings.json";
            var jsonKeys = JsonKeys.LoadFromJson(fileCfg);
            keyMappings = new KeyMappings(jsonKeys);
            keyMappings.Debug0 = new KeyboardKey(Keys.D0);
            keyMappings.Debug1 = new KeyboardKey(Keys.D1);
            keyMappings.Debug2 = new KeyboardKey(Keys.D2);
            keyMappings.Debug3 = new KeyboardKey(Keys.D3);
            keyMappings.Debug7 = new KeyboardKey(Keys.D7);
            keyMappings.Debug8 = new KeyboardKey(Keys.D8);
            keyMappings.Debug9 = new KeyboardKey(Keys.D9);
            keyMappings.TAB = new KeyboardKey(Keys.Tab);
            keyMappings.CAPS = new KeyboardKey(Keys.CapsLock);
        }

        public abstract void ProcessInput(float deltaTime);
        public abstract void ApplyInput(ClientInputState state);
        public void Update(float deltaTime)
        {
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            Key.Update(mouseState, keyState);

            keysDown.RemoveAll(key => !key.IsDown());


            UpdateMousePositionDelta();
            camera.Update(this);
            ProcessInput(deltaTime);
        }
        
        public void UpdateMousePositionDelta()
        {
            mousePosition.X = System.Windows.Forms.Cursor.Position.X;
            mousePosition.Y = System.Windows.Forms.Cursor.Position.Y;

            delta.X = mousePosition.X - center.X;
            delta.Y = mousePosition.Y - center.Y;

            mouseDelta = delta * mouseSensitivity * mouseSensAdapt;
            if (mouseLocked)
                System.Windows.Forms.Cursor.Position = center;
        }
    }
    
}

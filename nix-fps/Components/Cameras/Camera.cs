using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpFont;

namespace nixfps.Components.Cameras
{
    public class Camera
    {
        public Vector3 position;
        public Vector3 frontDirection;
        public Vector3 rightDirection;
        public Vector3 upDirection;

        public Matrix view, projection;
        public Matrix viewProjection;

        public float fieldOfView;
        public float aspectRatio;
        public float nearPlaneDistance;
        public float farPlaneDistance;

        public float yaw;
        public float pitch;

        public BoundingFrustum frustum;

        public Camera(float aspectRatio)
        {
            frustum = new BoundingFrustum(Matrix.Identity);
            fieldOfView = MathHelper.ToRadians(100);
            this.aspectRatio = aspectRatio;
            position = new Vector3(0, 3.5f, 10);
            nearPlaneDistance = 1;
            farPlaneDistance = 1000;
            yaw = 270;
            pitch = 0;
            
            UpdateCameraVectors();
            CalculateView();
            CalculateProjection();
            var game = NixFPS.GameInstance();
            var windowPos = game.Window.Position;
            screenCenter = game.screenCenter + windowPos;
            center = new System.Drawing.Point(screenCenter.X, screenCenter.Y);
        }
        public void UpdatePosition(Vector3 position)
        {
            this.position = position;
        }
        //public void Update(InputManager inputManager, float deltaTime)
        //{
        //    UpdatePosition(inputManager.position);
        //    UpdateMouse(inputManager.mouseDelta);
        //}
        public Point screenCenter;
        public Vector2 mouseDelta;
        public Vector2 delta;
        System.Drawing.Point center;

        public float mouseSensitivity = .3f; //.15f
        public float mouseSensAdapt = .09f;

        public bool mouseLocked = true;
        public void Update(float deltaTime)
        {
            GetMouseDelta(deltaTime);
            UpdateMouse();
            frustum.Matrix = view * projection;
        }
        public void GetMouseDelta(float deltaTime)
        {
            delta.X = System.Windows.Forms.Cursor.Position.X - center.X;
            delta.Y = System.Windows.Forms.Cursor.Position.Y - center.Y;

            mouseDelta = delta * mouseSensitivity * mouseSensAdapt;
            if (mouseLocked)
                System.Windows.Forms.Cursor.Position = center;
        }
        public void UpdateMouse()
        {
            
            yaw += mouseDelta.X ;
            if (yaw < 0)
                yaw += 360;
            yaw %= 360;

            pitch -= mouseDelta.Y;

            if (pitch > 89.0f)
                pitch = 89.0f;
            else if (pitch < -89.0f)
                pitch = -89.0f;

            UpdateCameraVectors();
            CalculateView();
        }    
        public void ResetToCenter()
        {
            yaw = 0;
            pitch = 0;
            UpdateCameraVectors();
            CalculateView();
        }
        void UpdateCameraVectors()
        {
            Vector3 tempFront;

            tempFront.X = MathF.Cos(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));
            tempFront.Y = MathF.Sin(MathHelper.ToRadians(pitch));
            tempFront.Z = MathF.Sin(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));

            frontDirection = Vector3.Normalize(tempFront);

            rightDirection = Vector3.Normalize(Vector3.Cross(frontDirection, Vector3.Up));
            upDirection = Vector3.Normalize(Vector3.Cross(rightDirection, frontDirection));
        }
        void CalculateView()
        {
            view = Matrix.CreateLookAt(position, position + frontDirection, upDirection);
        }
        void CalculateProjection()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        public bool FrustumContains(BoundingSphere collider)
        {
            return !frustum.Contains(collider).Equals(ContainmentType.Disjoint);
        }
        public bool FrustumContains(BoundingBox collider)
        {
            return !frustum.Contains(collider).Equals(ContainmentType.Disjoint);
        }
        public bool FrustumContains(Vector3 point)
        {
            return !frustum.Contains(point).Equals(ContainmentType.Disjoint);
        }


    }
}
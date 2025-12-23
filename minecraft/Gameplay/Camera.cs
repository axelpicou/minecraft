using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace minecraft.Gameplay
{
    public class Camera
    {
        // Position accessible en lecture, modifiable via SetPosition ou Move
        public Vector3 Position { get; private set; }
        public Vector3 Front { get; private set; } = -Vector3.UnitZ;
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; } = Vector3.UnitX;

        public float Yaw { get; private set; } = -90f; // regarde vers -Z
        public float Pitch { get; private set; } = 0f;

        public float MovementSpeed { get; set; } = 2.5f;
        public float MouseSensitivity { get; set; } = 0.1f;

        private Vector2 lastMousePos;
        private bool firstMove = true;

        public Camera(Vector3 startPosition)
        {
            Position = startPosition;
            UpdateCameraVectors();
        }

        public Matrix4 GetViewMatrix()
            => Matrix4.LookAt(Position, Position + Front, Up);

        public void ProcessKeyboard(KeyboardState input, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;

            if (input.IsKeyDown(Keys.W))
                Position += Front * velocity;
            if (input.IsKeyDown(Keys.S))
                Position -= Front * velocity;
            if (input.IsKeyDown(Keys.A))
                Position -= Right * velocity;
            if (input.IsKeyDown(Keys.D))
                Position += Right * velocity;
            if (input.IsKeyDown(Keys.Space))
                Position += Up * velocity;
            if (input.IsKeyDown(Keys.LeftShift))
                Position -= Up * velocity;
        }

        public void ProcessMouse(Vector2 mousePos)
        {
            if (firstMove)
            {
                lastMousePos = mousePos;
                firstMove = false;
            }

            float xOffset = mousePos.X - lastMousePos.X;
            float yOffset = lastMousePos.Y - mousePos.Y;

            lastMousePos = mousePos;

            xOffset *= MouseSensitivity;
            yOffset *= MouseSensitivity;

            Yaw += xOffset;
            Pitch += yOffset;

            Pitch = MathHelper.Clamp(Pitch, -89f, 89f);

            UpdateCameraVectors();
        }

        private void UpdateCameraVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = Vector3.Normalize(front);
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        // ============================================
        // Nouvelles méthodes pour gameplay
        // ============================================

        // Repositionne la caméra à une position donnée
        public void SetPosition(Vector3 newPos)
        {
            Position = newPos;
        }

        // Déplace la caméra d'un offset donné
        public void Move(Vector3 offset)
        {
            Position += offset;
        }
    }
}

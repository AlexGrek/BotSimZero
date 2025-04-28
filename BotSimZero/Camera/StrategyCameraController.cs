using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Extensions;
using System;
using Stride.Core.Diagnostics;
using BotSimZero.Core;
using System.Linq;

namespace BotSimZero.Camera
{
    public class StrategyCameraController : SyncScript
    {
        public float MoveSpeed = 100f;
        public float ZoomSpeed = .5f;
        public float MinZoom = 5f;
        public float MaxZoom = 50f;
        public float CameraAngle = 0f; // degrees (tilt/pitch angle)

        private float orbitAngle = 45f; // degrees (starting angle)
        public float OrbitSpeed = 90f; // degrees per second


        private float zoomLevel;
        private Vector3 focusPoint = new Vector3(10, 0, 10); // camera looks at this
        private Quaternion initialPitchRotation;
        private Vector3 startRotation;


        public override void Start()
        {
            // Set initial zoom level
            zoomLevel = 30f;

            // Save initial editor-set tilt (pitch), only use Y-axis orbit in script
            var currentRotation = Entity.Transform.Rotation;
            Vector3 yawPitchRoll;
            yawPitchRoll = currentRotation.YawPitchRoll;
            initialPitchRotation = Quaternion.RotationZ(yawPitchRoll.Y);
            startRotation = currentRotation.YawPitchRoll;
        }

        public override void Update()
        {
            var deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // --- Orbit input ---
            if (Input.IsKeyDown(Keys.Q))
                orbitAngle += OrbitSpeed * deltaTime;
            if (Input.IsKeyDown(Keys.E))
                orbitAngle -= OrbitSpeed * deltaTime;

            // Clamp or wrap angle (optional)
            orbitAngle %= 360f;



            // --- Input ---
            Vector2 moveInput = Vector2.Zero;

                // Keyboard
                if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W)) moveInput.Y -= 1;
            if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S)) moveInput.Y += 1;
            if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A)) moveInput.X -= 1;
            if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D)) moveInput.X += 1;

            if (Input.HasGamePad)
                moveInput += Input.DefaultGamePad.State.LeftThumb;

            // Normalize if moving diagonally
            if (moveInput.Length() > 1)
                moveInput.Normalize();

            float radians = MathUtil.DegreesToRadians(orbitAngle);
            var forward = new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians));
            var right = Vector3.Cross(Vector3.UnitY, forward);

            focusPoint += (right * moveInput.X + forward * moveInput.Y) * MoveSpeed * deltaTime;

            // --- Input: Zoom ---
            zoomLevel -= Input.MouseWheelDelta * ZoomSpeed;
            if (Input.HasGamePad)
            {
                zoomLevel -= Input.DefaultGamePad.State.LeftTrigger * ZoomSpeed;
                zoomLevel += Input.DefaultGamePad.State.RightTrigger * ZoomSpeed;
            }

            zoomLevel = MathUtil.Clamp(zoomLevel, MinZoom, MaxZoom);

            float height = zoomLevel * 0.6f;
            Vector3 cameraOffset = new Vector3(
                MathF.Sin(radians) * zoomLevel,
                height,
                MathF.Cos(radians) * zoomLevel
            );


            // --- Compute Camera Position ---
            Vector3 cameraPosition = focusPoint + cameraOffset;


            Entity.Transform.Position = cameraPosition;

            // --- Look at focus point ---
            Quaternion tiltDown; 
            Quaternion.RotationAxis(in Vector3.UnitX, MathUtil.DegreesToRadians(-CameraAngle), out tiltDown);
            var yawRotation = Quaternion.RotationY(MathUtil.DegreesToRadians(orbitAngle));
            Entity.Transform.Rotation = tiltDown * yawRotation;
            //Entity.Transform.Rotation = Quaternion.Lerp(Entity.Transform.Rotation, (yawRotation * Quaternion.BetweenDirections(focusPoint, cameraPosition)), CameraAngle);

            var ui = Entity.Scene.Entities
                .FirstOrDefault(e => e.Get<UIComponent>() != null)
                ?.Get<UIComponent>();

            if ( ui != null)
            {
                
            }
        }
    }

}

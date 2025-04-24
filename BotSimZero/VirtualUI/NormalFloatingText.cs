using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public class NormalFloatingText: StartupScript
    {
        public SpriteFont Font; // Assign in editor
        public string Message = "Floating Text!";
        public Color TextColor = Color.White;
        public float Scale = 0.005f; // UI scale in 3D space

        public override void Start()
        {
            // Add UIComponent if missing
            var uiComponent = Entity.GetOrCreate<UIComponent>();

            // Create a UI canvas
            var canvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Create a centered text block
            var textBlock = new TextBlock
            {
                Font = Font,
                Text = Message,
                TextColor = TextColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            canvas.Children.Add(textBlock);

            // Assign to UIComponent
            uiComponent.Page = new UIPage { RootElement = canvas };

            // Optional: scale down the UI in world space
            Entity.Transform.Scale = new Vector3(Scale);

            // Optional: make it always face the camera
            Script.AddTask(() => FaceCamera());
        }

        private async Task FaceCamera()
        {
            //while (Game.IsRunning)
            //{
            //    var camera = SceneSystem?.GraphicsCompositor?.Camera?.Entity;
            //    if (camera != null)
            //    {
            //        Entity.Transform.LookAt(camera.Transform.WorldMatrix.TranslationVector);
            //    }

            //    await Script.NextFrame();
            //}
        }
    }
}

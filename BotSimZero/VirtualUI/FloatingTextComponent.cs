using BotSimZero.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using Stride.Rendering.Materials;
using Stride.Rendering.Sprites;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public class FloatingTextComponent : AsyncScript
    {
        public string Text = "Hello VR World!";
        public int Width = 512;
        public int Height = 512;
        public Color TextColor = Color.Red;
        public SpriteFont Font;

        private Texture renderTexture;
        private SpriteBatch spriteBatch;
        private Material material;
        private bool initialized = false;
        private Texture depthBuffer;

        public override async Task Execute()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            while (Game.IsRunning)
            {
                if (!initialized)
                {
                    InitRenderTarget();
                    var ctx = Game.GraphicsContext;
                    var list = ctx.CommandList;
                    UpdateTexture(renderTexture, list, ctx);
                    initialized = true;
                }

                // Optional: Billboard effect (always face camera)
                var camera = this.Entity.Scene.Entities
                .FirstOrDefault(e => e.Get<CameraComponent>() != null)
                ?.Get<CameraComponent>();
                Entity.Transform.Rotation = Quaternion.Invert(camera?.Entity.Transform.Rotation ?? Quaternion.Identity);

                

                await Script.NextFrame();
            }
        }

        private void InitRenderTarget()
        {
            renderTexture = Texture.New2D(GraphicsDevice, Width, Height, PixelFormat.B8G8R8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            depthBuffer = Texture.New2D(GraphicsDevice, 512, 512, false, PixelFormat.D16_UNorm, TextureFlags.DepthStencil);
            //material = new Material();

            // Apply to model
            var modelComponent = Entity.Get<ModelComponent>();
            var mat = modelComponent.GetMaterial(0);
            var pass = mat.Passes[0];
            pass.Parameters.Set(MaterialKeys.DiffuseMap, renderTexture);
            material = mat;
        }

        public void UpdateText(string newText)
        {
            Text = newText;
            var ctx = Game.GraphicsContext;
            var list = ctx.CommandList;
            UpdateTexture(renderTexture, list, ctx);
        }

        private void UpdateTexture(Texture texture, CommandList list, GraphicsContext ctx)
        {
            
            // Set the render target using the provided CommandList instance  
            list.SetRenderTargetAndViewport(depthBuffer, texture);
            var spriteBatch = new SpriteBatch(GraphicsDevice);

            // Clear the render target with a transparent color  
            list.Clear(texture, Color.Transparent);

            // Begin drawing with the SpriteBatch  
            spriteBatch.Begin(ctx);
            spriteBatch.Draw(texture, new Vector2(1f, 1f), Color.Red);
            spriteBatch.DrawString(Font, Text, new Vector2(0.10f, 0.10f), TextColor);
            
            spriteBatch.End();

            var image = list.RenderTargets[0].GetDataAsImage(list);
            image.Save(new System.IO.FileStream("output.jpg", System.IO.FileMode.Create), ImageFileType.Jpg);


            // Reset the render target  
            list.ResetTargets();
        }
    }
}

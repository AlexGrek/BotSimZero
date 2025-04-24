using SimuliEngine.Interop;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Materials;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public abstract class UiAsyncScript: AsyncScript
    {
        public SpriteFont Font; // Assign in editor
        public Entity PlaneEntity; // Assign in editor (3D plane in scene) or use "this" entity
        public int TextureWidth = 1024;
        public int TextureHeight = 1024;

        protected Texture renderTexture;
        protected Texture depthTexture;
        protected SpriteBatch spriteBatch;
        
        public Color BgColor = Color.Black;
        public float FontSize = 48f; // Font size for the text

        public string Message = "Hello World!";

        public void Initialize()
        {
            PlaneEntity ??= Entity;

            // 2. Create texture to render to
            renderTexture = Texture.New2D(GraphicsDevice, TextureWidth, TextureHeight, PixelFormat.B8G8R8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            depthTexture = Texture.New2D(GraphicsDevice, TextureWidth, TextureHeight, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);
            var ctx = Game.GraphicsContext;
            var commandList = ctx.CommandList;
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //await Script.NextFrame();
            var planeMaterial = PlaneEntity.Get<ModelComponent>().Materials[0];

            planeMaterial.Passes[0].Parameters.Set(MaterialKeys.DiffuseMap, renderTexture);
            planeMaterial.Passes[0].Parameters.Set(MaterialKeys.EmissiveMap, renderTexture);
        }

        protected async Task RunUpdateLoop(Func<CommandList, GraphicsContext, Task> userCode)
        {
            var ctx = Game.GraphicsContext;
            var commandList = ctx.CommandList;
            while (Game.IsRunning)
            {
                await Script.NextFrame();
                await userCode(commandList, ctx);
            }
        }

        protected void RenderUIToTexture(string data, string header)
        {
            spriteBatch.Begin(Game.GraphicsContext);
            var dataSize = spriteBatch.MeasureString(Font, data, FontSize);
            var centerPosition = new Vector2(TextureWidth / 2, TextureHeight / 2);
            var dataCenterPosition = new Vector2(centerPosition.X - dataSize.X / 2, centerPosition.Y - dataSize.Y / 2);
            spriteBatch.DrawString(Font, data, FontSize, dataCenterPosition, Color.LimeGreen);
            spriteBatch.DrawString(Font, header, new Vector2(10, 0), Color.LimeGreen);
            spriteBatch.End();
        }

        protected void UpdateTexture(CommandList commandList, Texture depth, (string, string) data)
        {
            commandList.Clear(renderTexture, BgColor);
            commandList.Clear(depth, DepthStencilClearOptions.DepthBuffer);
            commandList.SetRenderTargetAndViewport(depth, renderTexture);
            RenderUIToTexture(data.Item1, data.Item2);
            commandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
        }

        
    }
}

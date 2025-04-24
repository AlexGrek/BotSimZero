using SimuliEngine.Interop;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Materials;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using Stride.Updater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public class WorldSpaceUIText : UiAsyncScript
    {
        private TextBlock textBlock;
        private string _dataString = "Dummy data value";
        private string _headerString = "Dummy data header";
        private IDisplayDataStringProvider _dataSource = new RandomDaatProvider();
        public Color BgColor = Color.Black;
        public float FontSize = 48f; // Font size for the text

        public IDisplayDataStringProvider DataProvider { set
            {
                _dataSource = value;
            } }

        public int UpdateEveryNFrames = 10;

        private int _frameCounter = 0;

        public string Message = "Hello World!";

        public override async Task Execute()
        {
            PlaneEntity ??= Entity;

            // 2. Create texture to render to
            renderTexture = Texture.New2D(GraphicsDevice, TextureWidth, TextureHeight, PixelFormat.B8G8R8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            var depth = Texture.New2D(GraphicsDevice, TextureWidth, TextureHeight, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);
            var ctx = Game.GraphicsContext;
            var commandList = ctx.CommandList;
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //await Script.NextFrame();
            var planeMaterial = PlaneEntity.Get<ModelComponent>().Materials[0];

            planeMaterial.Passes[0].Parameters.Set(MaterialKeys.DiffuseMap, renderTexture);
            planeMaterial.Passes[0].Parameters.Set(MaterialKeys.EmissiveMap, renderTexture);

            // 4. Render loop
            while (Game.IsRunning)
            {
                _frameCounter++;
                if (_frameCounter >= UpdateEveryNFrames)
                {
                    UpdateData();
                    UpdateTexture(commandList, depth);
                    _frameCounter = 0;
                }
                //commandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Red);
                await Script.NextFrame();
            }
        }

        private void UpdateTexture(CommandList commandList, Texture depth)
        {
            commandList.Clear(renderTexture, BgColor);
            commandList.Clear(depth, DepthStencilClearOptions.DepthBuffer);
            commandList.SetRenderTargetAndViewport(depth, renderTexture);
            RenderUIToTexture();
            commandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
        }

        private void UpdateData()
        {
            if (_dataSource != null) {
                _dataString = _dataSource.GetDisplayDataString(null);
                _headerString = _dataSource.GetDisplayDataHeader(null);
            }
        }

        private void RenderUIToTexture()
        {
            spriteBatch.Begin(Game.GraphicsContext);
            var dataSize = spriteBatch.MeasureString(Font, _dataString, FontSize);
            var centerPosition = new Vector2(TextureWidth / 2, TextureHeight / 2);
            var dataCenterPosition = new Vector2(centerPosition.X - dataSize.X / 2, centerPosition.Y - dataSize.Y / 2);
            spriteBatch.DrawString(Font, _dataString, FontSize, dataCenterPosition, Color.LimeGreen);
            spriteBatch.DrawString(Font, _headerString, new Vector2(10, 0), Color.LimeGreen);
            spriteBatch.End();
        }

        public void UpdateText(string newText)
        {
            Message = newText;
            if (textBlock != null)
                textBlock.Text = newText;
        }
    }
}

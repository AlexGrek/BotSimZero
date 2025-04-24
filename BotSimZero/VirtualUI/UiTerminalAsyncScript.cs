using BotSimZero.Core;
using BotSimZero.VirtualUI.Terminal;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Materials;
using System;
using System.Threading.Tasks;
using Utils = SimuliEngine.Utils;

namespace BotSimZero.VirtualUI
{
    public class UiTerminalAsyncScript: AsyncScript
    {
        public string AppName = "HelloWorld";
        protected dynamic AppOptions = null;

        public SpriteFont Font; // Assign in editor
        public Entity PlaneEntity; // Assign in editor (3D plane in scene) or use "this" entity
        public int TextureWidth = 1024;
        public int TextureHeight = 1024;

        protected Texture renderTexture;
        protected Texture depthTexture;
        protected SpriteBatch spriteBatch;

        public Color BgColor = Color.Black;
        public float FontSize = 48f; // Font size for the text


        public void AppOptionsString(string options)
        {
            if (!String.IsNullOrEmpty(options))
            {
                AppOptions = Utils.ParseTuple(options);
            } else
            {
                AppOptions = null;
            }
        }

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

            if (!string.IsNullOrEmpty(AppName))
            {
                ITerminalApp app = GlobalGameContext.Instance.Repository.CreateApp(AppName, AppOptions);
                if (app != null)
                {
                    _app = app;
                }
            }
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

        public override async Task Execute()
        {
            Initialize();

            var userCode = new Func<CommandList, GraphicsContext, Task>(async (commandList, ctx) =>
            {
                _frameCounter++;
                if (_frameCounter >= UpdateEveryNFrames)
                {
                    UpdateData();
                    UpdateTexture(commandList, depthTexture, UpdateData());
                    _frameCounter = 0;
                }
            });

            await RunUpdateLoop(userCode);
        }

        protected void RenderAppToTexture(string data, int n)
        {
            spriteBatch.Begin(Game.GraphicsContext);
            //var dataSize = spriteBatch.MeasureString(Font, data, FontSize);
            //var centerPosition = new Vector2(TextureWidth / 2, TextureHeight / 2);
            //var dataCenterPosition = new Vector2(centerPosition.X - dataSize.X / 2, centerPosition.Y - dataSize.Y / 2);
            spriteBatch.DrawString(Font, data, FontSize, new Vector2(10f, (float)n*FontSize), Color.LimeGreen);
            spriteBatch.End();
        }

        protected void UpdateTexture(CommandList commandList, Texture depth, string[] data)
        {
            commandList.Clear(renderTexture, BgColor);
            commandList.Clear(depth, DepthStencilClearOptions.DepthBuffer);
            commandList.SetRenderTargetAndViewport(depth, renderTexture);
            int i = 0;
            foreach (string str in data)
            {
                RenderAppToTexture(str, i);
                i++;
            }
            commandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
        }

        public ITerminalApp App
        {
            set
            {
                _app = value;
            }
        }

        public int UpdateEveryNFrames = 10;
        private int _frameCounter = 0;

        private string[] _dataStrings = new[] { "Dummy data value" };
        private ITerminalApp _app = null;

        protected string[] UpdateData()
        {
            if (_app != null)
            {
                _dataStrings = _app.GetLines();
            }
            return _dataStrings;
        }
    }
}

using SimuliEngine;
using SimuliEngine.Interop;
using Stride.Core.Mathematics;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public class UiDisplayAsyncScript: UiAsyncScript
    {
        public string DataSourceAddress = "Random";
        protected dynamic DataSourceOptions = null;

        public void DataSourceOptionsString(string options)
        {
            if (!String.IsNullOrEmpty(options))
            {
                DataSourceOptions = Utils.ParseTuple(options);
            } else
            {
                DataSourceOptions = null;
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

        public IDisplayDataStringProvider DataProvider
        {
            set
            {
                _dataSource = value;
            }
        }

        public int UpdateEveryNFrames = 10;
        private int _frameCounter = 0;

        private string _dataString = "Dummy data value";
        private string _headerString = "Dummy data header";
        private IDisplayDataStringProvider _dataSource = new RandomDaatProvider();

        protected (string, string) UpdateData()
        {
            if (_dataSource != null)
            {
                _dataString = _dataSource.GetDisplayDataString(DataSourceOptions);
                _headerString = _dataSource.GetDisplayDataHeader(DataSourceOptions);
            }
            return (_dataString, _headerString);
        }
    }
}

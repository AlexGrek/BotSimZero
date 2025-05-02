using BotSimZero.VirtualUI;
using BotSimZero.VirtualUI.Terminal;
using SimuliEngine;
using SimuliEngine.Interop;
using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace BotSimZero.Core
{
    internal class GlobalGameContext
    {
        private static GlobalGameContext _instance;
        public static GlobalGameContext Instance => _instance ??= new GlobalGameContext(64, 64);

        public static float CellSize = 1f;

        public static float CellHalfSize => CellSize / 2f;

        private Dictionary<string, IDisplayDataStringProvider> _dataSources = new();

        public Repository Repository = new Repository();

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }

        public UiContext UiContext { get; private set; } = new UiContext();

        private GlobalGameContext(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;

            ConfigureGlobalLogger();

            _dataSources.Add("Random", new RandomDaatProvider());
        }

        private void ConfigureGlobalLogger()
        {
            GlobalSimLogger.AddLogger(new FileLogConsumer(null));
        }

        public static (int, int) GetSize => (Instance.SizeX, Instance.SizeY);
        public static int GetSizeX => Instance.SizeX;
        public static int GetSizeY => Instance.SizeY;

        public static void Reset()
        {
            if (_instance != null)
            {
                _instance = new GlobalGameContext(64, 64);
            } else
            {
                throw new InvalidOperationException("GlobalGameContext is not initialized.");
            }
        }

        public void RegisterDataSource(string address, IDisplayDataStringProvider dataSource)
        {
            ArgumentNullException.ThrowIfNull(dataSource);
            // Register the data source in a dictionary or list
            _dataSources.Add(address, dataSource);
        }

        public IDisplayDataStringProvider GetDataSourceByAddress(string address)
        {
            return _dataSources.TryGetValue(address, out var dataSource) ? dataSource : ParseGetDataSourceByAddress(address);
        }

        private IDisplayDataStringProvider ParseGetDataSourceByAddress(string address)
        {
            // TODO: implement
            return null;
        }
    }
}

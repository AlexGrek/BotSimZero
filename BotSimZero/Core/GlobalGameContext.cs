using BotSimZero.VirtualUI;
using BotSimZero.VirtualUI.Terminal;
using SimuliEngine;
using SimuliEngine.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Data;

namespace BotSimZero.Core
{
    public class TerminalLast3Logger : ILogConsumer
    {
        private ConcurrentQueue<string> _entries = new ConcurrentQueue<string>();

        public void Dispose()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void Log(string message)
        {
            JustLog(message);
        }

        private void JustLog(string message)
        {
            _entries.Enqueue(message);
            if (_entries.Count > 3)
            {
                _entries.TryDequeue(out _);
            }
        }

        public string Last()
        {
            if (_entries.TryPeek(out var entry))
            {
                return entry;
            }
            return "";
        }

        public void LogError(string message)
        {
            JustLog(message);
        }

        public void LogWarning(string message)
        {
            JustLog(message);
        }
    }

    public class LogDataStringProvider : IDisplayDataStringProvider
    {
        private TerminalLast3Logger _logger;

        public LogDataStringProvider(TerminalLast3Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetDisplayDataHeader(dynamic options)
        {
            return "Log";
        }

        public string GetDisplayDataString(dynamic options)
        {
            return _logger.Last();
        }
    }

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

            ConfigureGlobalLogger(out var logger);

            _dataSources.Add("Random", new RandomDaatProvider());
            _dataSources.Add("LastLog", new LogDataStringProvider(logger));
        }

        private void ConfigureGlobalLogger(out TerminalLast3Logger logger)
        {
            GlobalSimLogger.AddLogger(new FileLogConsumer(null));
            logger = new TerminalLast3Logger();
            GlobalSimLogger.AddLogger(logger);
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

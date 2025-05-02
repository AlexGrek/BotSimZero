using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine
{
    public interface ILogConsumer : IDisposable
    {
        void Log(string message);
        void LogError(string message);
        void LogWarning(string message);
        void Initialize();
    }

    public interface ILoggable
    {
        public void Log(string message)
        {
            GlobalSimLogger.Logger.Log($"[{GetType().Name}] {message}");
        }
        public void LogError(string message) => GlobalSimLogger.Logger.LogError($"[{GetType().Name}] {message}");
        public void LogWarning(string message) => GlobalSimLogger.Logger.LogWarning($"[{GetType().Name}] {message}");
    }

    public class SimLogger
    {
        public required ILogConsumer LogConsumer { get; set; }

        private bool _initialized = false;

        public virtual void Log(string message)
        {
            EnsureInitialized();
            LogConsumer.Log(message);
        }
        public virtual void LogError(string message)
        {
            EnsureInitialized();
            LogConsumer.LogError(message);
        }
        public virtual void LogWarning(string message)
        {
            EnsureInitialized();
            LogConsumer.LogWarning(message);
        }

        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                LogConsumer.Initialize();
                _initialized = true;
            }
        }

        public void Initialize()
        {
            if (_initialized)
                return;
            LogConsumer.Initialize();
            _initialized = true;
        }
    }

    public class ChainedSimLogger : SimLogger
    {
        private readonly SimLogger _next;

        public ChainedSimLogger(SimLogger next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public override void Log(string message)
        {
            base.Log(message); // Call the current logger's consumer
            _next.Log(message); // Pass the log to the next logger in the chain
        }

        public override void LogError(string message)
        {
            base.LogError(message);
            _next.LogError(message);
        }

        public override void LogWarning(string message)
        {
            base.LogWarning(message);
            _next.LogWarning(message);
        }
    }

    public static class GlobalSimLogger
    {
        public static SimLogger Logger { get; set; } = new SimLogger
        {
            LogConsumer = new DefaultLogConsumer() // Provide a default implementation of ILogConsumer
        };

        public static void Log(string message) => Logger.Log(message);

        public static void LogError(string message) => Logger.LogError(message);

        public static void LogWarning(string message) => Logger.LogWarning(message);

        public static void SetLogger(ILogConsumer consumer)
        {
            Logger = new SimLogger() { LogConsumer = consumer };
        }

        public static void AddLogger(ILogConsumer newConsumer)
        {
            if (newConsumer == null)
                throw new ArgumentNullException(nameof(newConsumer));

            // Wrap the current logger in a ChainedSimLogger and set the new logger as the primary
            Logger = new ChainedSimLogger(Logger)
            {
                LogConsumer = newConsumer
            };
        }
    }

    // Example default implementation of ILogConsumer
    public class DefaultLogConsumer : ILogConsumer
    {
        public void Log(string message) => Console.WriteLine($"Log: {message}");
        public void LogError(string message) => Console.WriteLine($"Error: {message}");
        public void LogWarning(string message) => Console.WriteLine($"Warning: {message}");
        public void Initialize() => Console.WriteLine("DefaultLogConsumer initialized.");
        public void Dispose() { }
    }

    public class FileLogConsumer : ILogConsumer
    {
        private readonly string _filePath;
        public FileLogConsumer(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                filePath = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
            _filePath = filePath;
        }
        public void Log(string message) => File.AppendAllText(_filePath, $"Log: {message}\n");
        public void LogError(string message) => File.AppendAllText(_filePath, $"Error: {message}\n");
        public void LogWarning(string message) => File.AppendAllText(_filePath, $"Warning: {message}\n");
        public void Initialize() => File.AppendAllText(_filePath, "FileLogConsumer initialized.\n");
        public void Dispose() { }
    }

    public class BufferedFileLogConsumer : ILogConsumer
    {
        private readonly string _filePath;
        private StreamWriter? _streamWriter;

        public BufferedFileLogConsumer(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";

            _filePath = filePath;
        }

        public void Initialize()
        {
            _streamWriter = new StreamWriter(_filePath, append: true);
            _streamWriter.WriteLine("BufferedFileLogConsumer initialized.");
        }

        public void Log(string message)
        {
            EnsureStreamWriterInitialized();
            _streamWriter!.WriteLine($"Log: {message}");
        }

        public void LogError(string message)
        {
            EnsureStreamWriterInitialized();
            _streamWriter!.WriteLine($"Error: {message}");
        }

        public void LogWarning(string message)
        {
            EnsureStreamWriterInitialized();
            _streamWriter!.WriteLine($"Warning: {message}");
        }

        private void EnsureStreamWriterInitialized()
        {
            if (_streamWriter == null)
                throw new InvalidOperationException("BufferedFileLogConsumer is not initialized.");
        }

        public void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Flush(); // Ensure all data is written to the file
                _streamWriter.Dispose();
                _streamWriter = null;
            }
        }
    }
}

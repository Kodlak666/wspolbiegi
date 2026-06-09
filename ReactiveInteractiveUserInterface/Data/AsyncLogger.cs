using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class AsyncLogger : IDisposable
    {
        // BlockingCollection działa jak kolejka, a boundedCapacity zapobiega zapchaniu pamięci.
        private readonly BlockingCollection<string> _buffer = new BlockingCollection<string>(boundedCapacity: 100);
        private readonly Task _loggingTask;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _filePath = "diagnostic_log.txt";

        public AsyncLogger()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
            _loggingTask = Task.Run(LogToFile);
        }

        public void Log(string message)
        {
            if (!_buffer.IsAddingCompleted)
            {
                // TryAdd NIE blokuje wątku. Jeśli bufor jest pełny (brak przepustowości),
                // po prostu ignoruje wpis i aplikacja działa dalej płynnie.
                _buffer.TryAdd($"{DateTime.Now:O}: {message}");
            }
        }

        private void LogToFile()
        {
            try
            {
                using (var writer = new StreamWriter(_filePath, append: true))
                {
                    // Kodowanie domyślnie opiera się na tekście, zgodne z ASCII/UTF-8.
                    foreach (var message in _buffer.GetConsumingEnumerable(_cts.Token))
                    {
                        writer.WriteLine(message);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            _buffer.CompleteAdding();
            _cts.Cancel();
            _loggingTask.Wait();
            _buffer.Dispose();
            _cts.Dispose();
        }
    }
}
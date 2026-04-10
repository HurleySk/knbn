using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Knbn.Extension.Models;

namespace Knbn.Extension.Services
{
    public class PersistenceService
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public PersistenceService(string filePath)
        {
            _filePath = filePath;
        }

        public void Append(EventPayload payload)
        {
            lock (_lock)
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(payload);
                File.AppendAllText(_filePath, json + "\n");
            }
        }

        public IEnumerable<EventPayload> Replay()
        {
            if (!File.Exists(_filePath))
                yield break;

            foreach (var line in File.ReadLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var payload = JsonSerializer.Deserialize<EventPayload>(line);
                if (payload != null)
                    yield return payload;
            }
        }
    }
}

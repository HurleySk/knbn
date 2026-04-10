using System.IO;
using System.Linq;
using Knbn.Extension.Models;
using Knbn.Extension.Services;
using Xunit;

namespace Knbn.Tests
{
    public class PersistenceServiceTests : System.IDisposable
    {
        private readonly string _tempDir;
        private readonly string _eventFile;

        public PersistenceServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "knbn-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            _eventFile = Path.Combine(_tempDir, "events.jsonl");
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Fact]
        public void Append_WritesEventToFile()
        {
            var svc = new PersistenceService(_eventFile);
            var payload = new EventPayload
            {
                Action = "register",
                Title = "Auth System",
                SessionId = "s1",
                Cwd = "C:/projects/auth"
            };

            svc.Append(payload);

            var lines = File.ReadAllLines(_eventFile);
            Assert.Single(lines);
            Assert.Contains("Auth System", lines[0]);
        }

        [Fact]
        public void Replay_ReturnsAllEvents()
        {
            var svc = new PersistenceService(_eventFile);
            svc.Append(new EventPayload { Action = "register", Title = "A", SessionId = "s1", Cwd = "/a" });
            svc.Append(new EventPayload { Action = "register", Title = "B", SessionId = "s2", Cwd = "/b" });

            var events = svc.Replay().ToList();

            Assert.Equal(2, events.Count);
            Assert.Equal("A", events[0].Title);
            Assert.Equal("B", events[1].Title);
        }

        [Fact]
        public void Replay_ReturnsEmpty_WhenFileDoesNotExist()
        {
            var svc = new PersistenceService(Path.Combine(_tempDir, "nonexistent.jsonl"));

            var events = svc.Replay().ToList();

            Assert.Empty(events);
        }
    }
}

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Knbn.Extension.Models;
using Knbn.Extension.Services;
using Xunit;

namespace Knbn.Tests
{
    public class HttpListenerServiceTests : System.IDisposable
    {
        private readonly AggregatorService _aggregator;
        private readonly HttpListenerService _server;
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public HttpListenerServiceTests()
        {
            _aggregator = new AggregatorService();
            var port = GetFreePort();
            _baseUrl = $"http://localhost:{port}";
            _server = new HttpListenerService(_aggregator, null, port);
            _server.Start();
            _client = new HttpClient();
        }

        public void Dispose()
        {
            _server.Stop();
            _client.Dispose();
        }

        private static int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        [Fact]
        public async Task PostEvents_Register_ReturnsCreated()
        {
            var payload = new EventPayload
            {
                Action = "register",
                Title = "Auth System",
                SessionId = "s1",
                Cwd = "C:/projects/auth"
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_baseUrl}/events", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("created", body);
        }

        [Fact]
        public async Task GetCards_ReturnsRegisteredCards()
        {
            _aggregator.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "Auth System",
                SessionId = "s1",
                Cwd = "C:/projects/auth"
            });

            var response = await _client.GetAsync($"{_baseUrl}/cards");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Auth System", body);
        }

        [Fact]
        public async Task UnknownPath_Returns404()
        {
            var response = await _client.GetAsync($"{_baseUrl}/unknown");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

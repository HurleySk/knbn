using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Knbn.Extension.Models;

namespace Knbn.Extension.Services
{
    public class HttpListenerService
    {
        private readonly IAggregatorService _aggregator;
        private readonly PersistenceService _persistence;
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _listenTask;

        public HttpListenerService(IAggregatorService aggregator, PersistenceService persistence, int port = 9090)
        {
            _aggregator = aggregator;
            _persistence = persistence;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
        }

        public void Start()
        {
            _listener.Start();
            _listenTask = Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }

        private async Task ListenLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }

                try
                {
                    await HandleRequest(context);
                }
                catch
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            var path = context.Request.Url.AbsolutePath.TrimEnd('/');
            var method = context.Request.HttpMethod;

            if (path == "/events" && method == "POST")
                await HandlePostEvents(context);
            else if (path == "/cards" && method == "GET")
                HandleGetCards(context);
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }

        private async Task HandlePostEvents(HttpListenerContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                body = await reader.ReadToEndAsync();

            var payload = JsonSerializer.Deserialize<EventPayload>(body);
            _persistence?.Append(payload);
            var result = _aggregator.HandleEvent(payload);

            var json = JsonSerializer.Serialize(result);
            var buffer = Encoding.UTF8.GetBytes(json);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        private void HandleGetCards(HttpListenerContext context)
        {
            var cards = _aggregator.GetCards();
            var json = JsonSerializer.Serialize(new { cards });
            var buffer = Encoding.UTF8.GetBytes(json);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }
}

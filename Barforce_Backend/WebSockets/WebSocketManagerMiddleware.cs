using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Barforce_Backend.WebSockets
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler { get; set; }
        private readonly ILogger _logger;

        public WebSocketManagerMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler, ILoggerFactory loggerFactory)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
            _logger = loggerFactory.CreateLogger<WebSocketManagerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation($"Websocketiddleware, Invoke, Context: {context.ToString()}");
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation($"Websocketiddleware, Invoke, Accept Websocket, Socket: {socket.ToString()}");
            _webSocketHandler.OnConnected(socket); // aus Query String Geräte-Id => Socket damit aufbauen => auch in DB speicher

            await Receive(socket, async (result, buffer) =>
            {
                _logger.LogInformation($"Websocketiddleware, Invoke, Received Websocket, Result: {result.ToString()}, Buffer: {buffer.ToString()}");
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                    _logger.LogInformation($"Websocketiddleware, Invoke, Received Websocket Text");
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocketHandler.OnDisconnected(socket);
                    _logger.LogInformation($"Websocketiddleware, Invoke, Received Websocket Close");
                    return;
                }

            });
            await _next(context).ConfigureAwait(true);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                _logger.LogInformation($"Websocketiddleware, Invoke, Receive Websocket, socketState: {socket.State}");
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                           cancellationToken: CancellationToken.None);

                    handleMessage(result, buffer);
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}

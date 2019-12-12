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
        private MachineHandler _machineHandler { get; set; }
        private readonly ILogger _logger;

        public WebSocketManagerMiddleware(RequestDelegate next, MachineHandler machineHandler, ILoggerFactory loggerFactory)
        {
            _next = next;
            _machineHandler = machineHandler;
            _logger = loggerFactory.CreateLogger<WebSocketManagerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            _machineHandler.OnConnected(socket);

            await Receive(socket, async (result, buffer) =>
            {
                _logger.LogInformation($"Websocketiddleware, Received Websocket MessageStatus: {result.MessageType}");
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _machineHandler.ReceiveAsync(socket, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _machineHandler.OnDisconnected(socket);
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

using Microsoft.AspNetCore.Http;
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

        public WebSocketManagerMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine($"Websocketiddleware, Invoke, Context: {context}");
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            Console.WriteLine($"Websocketiddleware, Invoke, Accept Websocket, Socket: {socket}");
            _webSocketHandler.OnConnected(socket); // aus Query String Geräte-Id => Socket damit aufbauen => auch in DB speicher

            await Receive(socket, async (result, buffer) =>
            {
                Console.WriteLine($"Websocketiddleware, Invoke, Received Websocket, Result: {result}, Buffer: {buffer}");
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                    Console.WriteLine($"Websocketiddleware, Invoke, Received Websocket Text");
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocketHandler.OnDisconnected(socket);
                    Console.WriteLine($"Websocketiddleware, Invoke, Received Websocket Close");
                    return;
                }

            });
            await _next.Invoke(context);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                Console.WriteLine($"Websocketiddleware, Invoke, Receive Websocket, socketState: {socket.State}");
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

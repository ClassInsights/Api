using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Api.Controllers;

[ApiExplorerSettings(IgnoreApi = true)] // ignore in swagger
public class WebSocketController : ControllerBase
{
    private static readonly Dictionary<int, Dictionary<string, WebSocket>> RoomWebSockets = new();

    [Route("/ws")]
    public async Task GetApp(int room)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!RoomWebSockets.TryGetValue(room, out var roomWebSockets))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        
        using var clientWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // filter websockets which are still alive
        roomWebSockets = roomWebSockets.Where(x => x.Value.State == WebSocketState.Open)
            .ToDictionary(i => i.Key, i => i.Value);

        // while connection is alive, read pc websockets and send result to client websocket
        while (!clientWebSocket.CloseStatus.HasValue)
        {
            var heartbeats = new List<Heartbeat>();

            foreach (var (_, pcWebSocket) in roomWebSockets)
            {
                // skip closed websockets
                if (pcWebSocket.State != WebSocketState.Open)
                    continue;

                var result = await ReadTextAsync(pcWebSocket);
                
                // skip if close handshake is performed
                if (result is null)
                    continue;

                var heartbeat = JsonConvert.DeserializeObject<Heartbeat>(result);
                if (heartbeat != null)
                    heartbeats.Add(heartbeat);
            }

            if (heartbeats.Count > 0)
                await SendTextAsync(JsonConvert.SerializeObject(heartbeats), clientWebSocket);
            else await Task.Delay(500);
        }

        // close connection because client has disconnected
        await HandleCloseAsync(clientWebSocket);
    }

    // todo: use IDs for PCs (in heartbeat and database)
    [Route("/ws/pc")]
    public async Task GetPc()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // read text from socket
        var input = await ReadTextAsync(webSocket);

        // return if shutdown is requested
        if (input is null)
            return;

        // deserialize to object
        var heartbeat = JsonConvert.DeserializeObject<Heartbeat>(input);

        // check if valid heartbeat object
        // todo: return error object
        if (heartbeat is null)
            return;

        // add socket to static dictionary, so we can access it later (create pc dictionary if not exists)
        if (!RoomWebSockets.TryGetValue(heartbeat.Room, out var pcWebSockets))
        {
            pcWebSockets = new Dictionary<string, WebSocket>();
            RoomWebSockets[heartbeat.Room] = pcWebSockets;
        }

        pcWebSockets[heartbeat.Name] = webSocket;

        while (webSocket.State != WebSocketState.Closed) await Task.Delay(10000); // keep websocket alive
    }

    private class Heartbeat
    {
        public string Type { get; set; } = null!;
        public int Room { get; set; }
        public string Name { get; set; } = null!;
        public Data? Data { get; set; }
    }

    private class Data
    {
        public float Power { get; set; }
        public string? Token { get; set; }
    }

    private static async Task SendTextAsync(string text, WebSocket webSocket)
    {
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(text));
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string?> ReadTextAsync(WebSocket webSocket)
    {
        var buffer = new byte[8192];
        var text = new StringBuilder();

        WebSocketReceiveResult receiveResult;
        do
        {
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (receiveResult.MessageType != WebSocketMessageType.Close)
                text.Append(Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, receiveResult.Count)));
            else
            {
                await HandleCloseAsync(webSocket);
                return null; // return null if close
            }
        } while (!receiveResult.EndOfMessage);

        return text.ToString();
    }

    private static async Task HandleCloseAsync(WebSocket webSocket)
    {
        if (webSocket.State == WebSocketState.CloseReceived)
            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None);
        else
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None);
    }
}
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Api.Controllers;

/// <inheritdoc />
[ApiExplorerSettings(IgnoreApi = true)] // ignore in swagger
public class WebSocketController : ControllerBase
{
    private static readonly Dictionary<int, WebSocket> PcWebSockets = new();

    /// <summary>
    ///     Returns power and usage information of Pc
    /// </summary>
    /// <param name="pc">Id of Pc</param>
    [Route("/ws")]
    public async Task GetApp(int pc)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // check if any websockets for requested pc
        if (!PcWebSockets.TryGetValue(pc, out var pcWebSocket))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        using var clientWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // while connection is alive, read pc websockets and send result to client websocket
        while (!clientWebSocket.CloseStatus.HasValue)
            // skip if close handshake is performed
            if (pcWebSocket.State == WebSocketState.Open && await ReadTextAsync(pcWebSocket) is { } result)
            {
                var heartbeat = JsonConvert.DeserializeObject<Heartbeat>(result);
                if (heartbeat != null)
                    await SendTextAsync(JsonConvert.SerializeObject(heartbeat), clientWebSocket);
            }
            else
            {
                await Task.Delay(500);
            }

        // close connection because client has disconnected
        await HandleCloseAsync(clientWebSocket);
    }

    /// <summary>
    ///     Send power and usage information of pc
    /// </summary>
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

        // add socket to static dictionary, so we can access it later
        PcWebSockets[heartbeat.ComputerId] = webSocket;

        while (webSocket.State != WebSocketState.Closed) await Task.Delay(10000); // keep websocket alive
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
            {
                text.Append(Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, receiveResult.Count)));
            }
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
            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription,
                CancellationToken.None);
        else
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription,
                CancellationToken.None);
    }


    private class Heartbeat
    {
        public int ComputerId { get; }
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Room { get; set; }
        public DateTime UpTime { get; set; }
        public Data? Data { get; set; }
    }

    internal class Data
    {
        public float Power { get; set; }
        public float RamUsage { get; set; }
        public List<float>? CpuUsage { get; set; }
        public List<float>? DiskUsages { get; set; }
        public List<Dictionary<string, float>>? EthernetUsages { get; set; }
    }
}
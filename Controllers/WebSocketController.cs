using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Api.Models.Database;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Api.Controllers;

public class WebSocketController(IClock clock, ClassInsightsContext context) : ControllerBase
{
    /// <summary>
    ///     Dictionary of all client WebSockets for a computer
    /// </summary>
    private static readonly Dictionary<long, List<WebSocket>> AppWebSockets = new();

    /// <summary>
    ///     Dictionary of all connected ComputerWebSockets
    /// </summary>
    public static readonly Dictionary<long, WebSocket> ComputerWebSockets = new();

    /// <summary>
    ///     Returns power and usage information of Computer
    /// </summary>
    /// <param name="computerId">ID of Computer</param>
    [Route("/ws/computers/{computerId:int}")]
    public async Task GetComputerInformation(int computerId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var clientWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        if (!AppWebSockets.ContainsKey(computerId))
            AppWebSockets[computerId] = [];

        AppWebSockets[computerId].Add(clientWebSocket);

        // while connection is alive
        while (!clientWebSocket.CloseStatus.HasValue) await Task.Delay(500); // keep websocket alive

        // remove ws from list if client disconnects
        AppWebSockets[computerId].Remove(clientWebSocket);

        // close connection because client has disconnected
        await HandleCloseAsync(clientWebSocket);
    }

    /// <summary>
    ///     Send power and usage information of pc
    /// </summary>
    [Route("/ws/computers")]
    public async Task SendComputerInformation()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var computerWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var sendWatch = new Stopwatch();
        var onlineWatch = new Stopwatch();
        sendWatch.Start();
        onlineWatch.Start();

        Heartbeat? lastHeartbeat = null;
        while (!computerWebSocket.CloseStatus.HasValue)
        {
            // wait min 500ms after each send 
            if (sendWatch.Elapsed.TotalMilliseconds < 500)
            {
                await Task.Delay(Math.Abs((int)(500 - sendWatch.Elapsed.TotalMilliseconds)));
                sendWatch.Restart();
            }

            // read text from socket
            var input = await ReadTextAsync(computerWebSocket);

            // return if shutdown is requested
            if (input is null)
                return;

            // deserialize to object
            var heartbeat = JsonSerializer.Deserialize<Heartbeat>(input);

            // check if valid heartbeat object
            if (heartbeat is null)
                continue;

            // set ws in dictionary on first heartbeat
            if (lastHeartbeat is null)
            {
                lastHeartbeat = heartbeat;
                ComputerWebSockets[lastHeartbeat.ComputerId] = computerWebSocket;
            }

            // check if any clients are connected
            if (AppWebSockets.TryGetValue(heartbeat.ComputerId, out var appWebsockets))
            {
                // send information of computer to all connected clients
                var sendInformationTasks = (from appWebsocket in appWebsockets
                    where appWebsocket.State == WebSocketState.Open
                    select SendTextAsync(JsonSerializer.Serialize(heartbeat), appWebsocket)).ToList();
                await Task.WhenAll(sendInformationTasks);
            }

            // update LastSeen all 8 seconds
            if (!(onlineWatch.Elapsed.TotalSeconds > 8))
                continue;

            var computer = await context.Computers.FindAsync(heartbeat.ComputerId);
            if (computer == null)
                continue;

            computer.LastSeen = clock.GetCurrentInstant();
            await context.SaveChangesAsync();
        }

        // remove ws from Dictionary
        if (lastHeartbeat != null) ComputerWebSockets.Remove(lastHeartbeat.ComputerId);

        // close connection because client has disconnected
        await HandleCloseAsync(computerWebSocket);
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

    private record Heartbeat(long ComputerId, string Type, string Name, long? Room, DateTime UpTime, Data? Data);

    private record Data(
        float Power,
        float RamUsage,
        List<float>? CpuUsage,
        List<float>? DiskUsages,
        List<Dictionary<string, float>>? EthernetUsages);
}
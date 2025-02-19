using Newtonsoft.Json;
using RushdownGrainInterfaces;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace RushdownRpc.Services;

internal class WebSocketService(
    ILocalSiloDetails localSiloDetails,
    IClusterClient clusterClient,
    ILogger<IWebSocketService> logger)
    : IWebSocketService, IClientCallback
{
    private readonly ILocalSiloDetails _localSiloDetails = localSiloDetails;
    private readonly IClusterClient _clusterClient = clusterClient;
    private readonly ConcurrentDictionary<Guid, WebSocketDetails> _connections = new();
    private readonly Guid _channelId = Guid.Parse("e7f4d5dc-6aa7-4a83-b2ed-4d12d8983447");
    private readonly ILogger<IWebSocketService> _logger = logger;

    public async Task Init()
    {
        var publisherGrain = _clusterClient.GetGrain<IPublisherGrain>(_localSiloDetails.SiloAddress.ToParsableString());
        var observerReference = _clusterClient.CreateObjectReference<IClientCallback>(this);
        await publisherGrain.Subscribe(observerReference);
    }

    public async Task AddConnection(Guid playerId, WebSocket webSocket, CancellationTokenSource cancellationTokenSource)
    {
        await AddOrUpdateWebSocket(playerId, new WebSocketDetails(webSocket, cancellationTokenSource));
        var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(playerId);
        await playerGrain.JoinChannel(_channelId, _localSiloDetails.SiloAddress);
    }

    private async Task RemoveConnection(Guid playerId)
    {
        if (_connections.TryRemove(playerId, out var _))
        {
            var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(playerId);
            await playerGrain.LeaveChannel(_channelId, _localSiloDetails.SiloAddress);
        }
    }

    public async Task ReceiveLoopAsync(Guid playerId, WebSocket webSocket, CancellationTokenSource cancellationTokenSource)
    {
        var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(playerId);

        _logger.LogInformation("User connected {userId}", playerId.ToString());

        try
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                buffer: new ArraySegment<byte>(buffer),
                cancellationToken: cancellationTokenSource.Token);

            while (!receiveResult.CloseStatus.HasValue)
            {
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    await playerGrain.SendMessage(_channelId, message);
                }

                receiveResult = await webSocket.ReceiveAsync(
                    buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: cancellationTokenSource.Token);
            }

            await webSocket.CloseAsync(
                closeStatus: receiveResult.CloseStatus.Value,
                statusDescription: receiveResult.CloseStatusDescription,
                cancellationToken: cancellationTokenSource.Token);
        }
        catch (WebSocketException)
        {
            // The remote party closed the WebSocket connection without completing the close handshake
        }
        finally
        {
            _logger.LogInformation("User disconnected {userId}", playerId.ToString());
            await RemoveConnection(playerId);
        }
    }

    public async Task PublishMessage(Guid[] playerIds, ChatMessage message)
    {
        var semaphore = new SemaphoreSlim(10);
        var messageText = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageText);
        var tasks = playerIds.Select(SendMessageAsync).ToList();
        await Task.WhenAll(tasks);

        async Task SendMessageAsync(Guid playerId)
        {
            if (!_connections.TryGetValue(playerId, out var webSocketDetails))
                return;

            await semaphore.WaitAsync();

            try
            {
                await webSocketDetails.WebSocket.SendAsync(
                    buffer: new ArraySegment<byte>(messageBytes),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: webSocketDetails.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send message to {playerId}: {exMessage}", playerId, ex.Message);
            }
            finally
            {
                semaphore.Release();

            }
        }
    }

    private async Task AddOrUpdateWebSocket(Guid playerIds, WebSocketDetails newWebSocketDetails)
    {
        if (_connections.TryGetValue(playerIds, out var existingWebSocketDetails))
        {
            if (existingWebSocketDetails.WebSocket.State == WebSocketState.Open)
            {
                await existingWebSocketDetails.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Replaced by new connection", CancellationToken.None);
            }
        }

        _connections[playerIds] = newWebSocketDetails;
    }

    private class WebSocketDetails(WebSocket webSocket, CancellationTokenSource cancellationTokenSource)
    {
        public readonly WebSocket WebSocket = webSocket;
        public readonly CancellationTokenSource CancellationTokenSource = cancellationTokenSource;
    }
}
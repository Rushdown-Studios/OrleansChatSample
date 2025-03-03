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
    private readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();
    private readonly string _defaultChannelId = "global";
    private readonly ILogger<IWebSocketService> _logger = logger;
    private const int _bufferSizeKb = 1024 * 4;

    public async Task Init()
    {
        var publisherGrain = _clusterClient.GetGrain<IPublisherGrain>(_localSiloDetails.SiloAddress.ToParsableString());
        var observerReference = _clusterClient.CreateObjectReference<IClientCallback>(this);
        await publisherGrain.Subscribe(observerReference);
    }

    public async Task AddConnection(Guid playerId, WebSocket webSocket)
    {
        await AddOrUpdateWebSocket(playerId, webSocket);
        var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(playerId);
        await playerGrain.JoinChannel(_defaultChannelId, _localSiloDetails.SiloAddress);
    }

    private async Task RemoveConnection(Guid playerId)
    {
        if (_connections.TryRemove(playerId, out var _))
        {
            var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(playerId);
            await playerGrain.LeaveChannel(_localSiloDetails.SiloAddress);
        }
    }

    public async Task ReceiveLoopAsync(Guid playerId, WebSocket webSocket)
    {
        try
        {
            _logger.LogInformation("User connected {userId}", playerId.ToString());
            var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(playerId);

            var buffer = new byte[_bufferSizeKb];
            var receiveResult = await webSocket.ReceiveAsync(
                buffer: new ArraySegment<byte>(buffer),
                cancellationToken: CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    var command = JsonConvert.DeserializeObject<Command>(message);
                    if (command.Type == CommandType.Channel)
                    {
                        await playerGrain.LeaveChannel(_localSiloDetails.SiloAddress);
                        await playerGrain.JoinChannel(command.Data, _localSiloDetails.SiloAddress);
                    }
                    else if (command.Type == CommandType.ChatMessage)
                    {
                        await playerGrain.SendMessage(command.Data);
                    }
                    else
                    {
                        _logger.LogError("Invalid command type = {commandType}", command.Type);
                    }
                }

                receiveResult = await webSocket.ReceiveAsync(
                    buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);
            }

            await webSocket.CloseAsync(
                closeStatus: receiveResult.CloseStatus.Value,
                statusDescription: receiveResult.CloseStatusDescription,
                cancellationToken: CancellationToken.None);
        }
        catch (WebSocketException)
        {
            // The remote party closed the WebSocket connection without completing the close handshake
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
        }
        finally
        {
            _logger.LogInformation("User disconnected {userId}", playerId.ToString());
            await RemoveConnection(playerId);
        }
    }

    public async Task PublishMessage(Guid[] playerIds, ChatMessage message)
    {
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(10);
        var messageText = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageText);
        var tasks = playerIds.Select(SendMessageAsync).ToList();
        await Task.WhenAll(tasks);

        async Task SendMessageAsync(Guid playerId)
        {
            if (!_connections.TryGetValue(playerId, out var webSocket))
                return;

                await semaphore.WaitAsync();

            try
            {
                await webSocket.SendAsync(
                    buffer: new ArraySegment<byte>(messageBytes),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send message to {playerId}: {exMessage}", playerId, ex.Message);
            }
            finally
            {
                semaphore.Release();

                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task AddOrUpdateWebSocket(Guid playerIds, WebSocket newWebSocket)
    {
        if (_connections.TryGetValue(playerIds, out var existingWebSocket))
        {
            if (existingWebSocket.State == WebSocketState.Open)
            {
                await existingWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Replaced by new connection", CancellationToken.None);
            }
        }

        _connections[playerIds] = newWebSocket;
    }
}
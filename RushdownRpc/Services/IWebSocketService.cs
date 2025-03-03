using Newtonsoft.Json;
using System.Net.WebSockets;

namespace RushdownRpc.Services;

public readonly struct CommandType
{
    public const string Channel = "channel";
    public const string ChatMessage = "chat_message";
}

public struct Command
{
    [JsonProperty("type")]
    public string Type;

    [JsonProperty("data")]
    public string Data;
}

/// <summary>
/// Interface for the internal service that manages websocket connections and lifetime
/// </summary>
public interface IWebSocketService
{
    /// <summary>
    /// Initialize the web socket service
    /// </summary>
    /// <returns></returns>
    Task Init();

    /// <summary>
    /// Add a new websocket connection
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="webSocket"></param>
    /// <param name="cancellationTokenSource"></param>
    /// <returns></returns>
    Task AddConnection(Guid playerId, WebSocket webSocket);

    /// <summary>
    /// Start an async task to receive websocket messages
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="webSocket"></param>
    /// <param name="cancellationTokenSource"></param>
    /// <returns></returns>
    Task ReceiveLoopAsync(Guid playerId, WebSocket webSocket);
}

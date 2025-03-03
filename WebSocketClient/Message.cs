using Newtonsoft.Json;

namespace WebSocketClient;

public struct Command(string type, string data)
{
    [JsonProperty("type")]
    public readonly string Type = type;

    [JsonProperty("data")]
    public readonly string Data = data;
}

public struct ChatMessage(string senderId, string message)
{
    [JsonProperty("sender")]
    public readonly string SenderId = senderId;

    [JsonProperty("message")]
    public readonly string Message = message;

    public bool IsValid() => string.IsNullOrEmpty(Message) && string.IsNullOrEmpty(SenderId);
}

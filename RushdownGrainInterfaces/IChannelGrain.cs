using Newtonsoft.Json;

namespace RushdownGrainInterfaces;

[GenerateSerializer]
[Alias("RushdownGrainInterfaces.ChatMessage")]
public readonly struct ChatMessage(Guid senderId, string message)
{
    [Id(0)]
    [JsonProperty("sender")]
    public readonly Guid SenderId = senderId;

    [Id(1)]
    [JsonProperty("message")]
    public readonly string Message = message;
}

[Alias("RushdownGrainInterfaces.IChannelGrain")]
public interface IChannelGrain : IGrainWithStringKey
{
    [Alias("JoinChannel")]
    Task JoinChannel(Guid UserId, SiloAddress siloAddress);

    [Alias("LeaveChannel")]
    Task LeaveChannel(Guid userId, SiloAddress siloAddress);

    [Alias("SendMessage")]
    Task SendMessage(ChatMessage message);
}

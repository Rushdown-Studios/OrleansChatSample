using Newtonsoft.Json;

namespace RushdownGrainInterfaces;

[GenerateSerializer]
[Alias("RushdownGrainInterfaces.ChatMessage")]
public readonly struct ChatMessage(Guid sender, string message)
{
    [Id(0)]
    [JsonProperty("sender")]
    public readonly Guid Sender = sender;

    [Id(1)]
    [JsonProperty("message")]
    public readonly string Message = message;
}

[Alias("RushdownGrainInterfaces.IChannelGrain")]
public interface IChannelGrain : IGrainWithGuidKey
{
    [Alias("JoinChannel")]
    Task JoinChannel(Guid UserId, SiloAddress siloAddress);

    [Alias("LeaveChannel")]
    Task LeaveChannel(Guid UserId, SiloAddress siloAddress);

    [Alias("SendMessage")]
    Task SendMessage(ChatMessage Message);
}

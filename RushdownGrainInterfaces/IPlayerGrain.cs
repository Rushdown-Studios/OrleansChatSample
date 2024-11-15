namespace RushdownGrainInterfaces;

[Alias("RushdownGrainInterfaces.IUserGrain")]
public interface IPlayerGrain : IGrainWithGuidKey
{
    [Alias("SendMessage")]
    Task SendMessage(Guid channelId, string message);

    [Alias("JoinChannel")]
    Task JoinChannel(Guid channelId, SiloAddress siloAddress);

    [Alias("LeaveChannel")]
    Task LeaveChannel(Guid channelId, SiloAddress siloAddress);
}

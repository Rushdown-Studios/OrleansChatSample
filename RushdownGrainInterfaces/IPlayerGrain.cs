namespace RushdownGrainInterfaces;

[Alias("RushdownGrainInterfaces.IUserGrain")]
public interface IPlayerGrain : IGrainWithGuidKey
{
    [Alias("SendMessage")]
    Task SendMessage(string message);

    [Alias("JoinChannel")]
    Task JoinChannel(string channelId, SiloAddress siloAddress);

    [Alias("LeaveChannel")]
    Task LeaveChannel(SiloAddress siloAddress);
}

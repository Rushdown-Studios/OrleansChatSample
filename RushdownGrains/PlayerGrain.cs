using RushdownGrainInterfaces;

namespace RushdownGrains;

internal class PlayerGrain(IGrainFactory grainFactory) : Grain, IPlayerGrain
{
    private readonly IGrainFactory _grainFactory = grainFactory;

    public async Task JoinChannel(Guid channelId, SiloAddress siloAddress)
    {
        var channelGrain = _grainFactory.GetGrain<IChannelGrain>(channelId);
        await channelGrain.JoinChannel(this.GetPrimaryKey(), siloAddress);
    }

    public async Task LeaveChannel(Guid channelId, SiloAddress siloAddress)
    {
        var channelGrain = _grainFactory.GetGrain<IChannelGrain>(channelId);
        await channelGrain.LeaveChannel(this.GetPrimaryKey(), siloAddress);
    }

    public async Task SendMessage(Guid channelId, string message)
    {
        var channelGrain = _grainFactory.GetGrain<IChannelGrain>(channelId);
        await channelGrain.SendMessage(new ChatMessage(this.GetPrimaryKey(), message));
    }
}

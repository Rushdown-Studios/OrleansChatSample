using RushdownGrainInterfaces;

namespace RushdownGrains;

internal class PlayerGrain(IGrainFactory grainFactory) : Grain, IPlayerGrain
{
    private readonly IGrainFactory _grainFactory = grainFactory;
    private string _currentChannel = string.Empty;

    public async Task JoinChannel(string channelId, SiloAddress siloAddress)
    {
        var channelGrain = _grainFactory.GetGrain<IChannelGrain>(channelId);
        await channelGrain.JoinChannel(this.GetPrimaryKey(), siloAddress);
        _currentChannel = channelId;
    }

    public async Task LeaveChannel(SiloAddress siloAddress)
    {
        var channelGrain = _grainFactory.GetGrain<IChannelGrain>(_currentChannel);
        await channelGrain.LeaveChannel(this.GetPrimaryKey(), siloAddress);
        _currentChannel = string.Empty;
    }

    public async Task SendMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(_currentChannel) && !string.IsNullOrWhiteSpace(message))
        {
            var channelGrain = _grainFactory.GetGrain<IChannelGrain>(_currentChannel);
            await channelGrain.SendMessage(new ChatMessage(this.GetPrimaryKey(), message));
        }
    }
}

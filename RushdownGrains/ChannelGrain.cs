using RushdownGrainInterfaces;

namespace RushdownGrains;

internal class ChannelGrain(IGrainFactory grainFactory) : Grain, IChannelGrain
{
    private readonly IGrainFactory _grainFactory = grainFactory;
    private readonly Dictionary<SiloAddress, HashSet<Guid>> _memberSiloMap = [];

    public Task JoinChannel(Guid userId, SiloAddress siloAddress)
    {
        if (_memberSiloMap.TryGetValue(siloAddress, out var members))
        {
            members.Add(userId);
        }
        else
        {
            _memberSiloMap.Add(siloAddress, [userId]);
        }
        return Task.CompletedTask;
    }

    public Task LeaveChannel(Guid UserId, SiloAddress siloAddress)
    {
        if (_memberSiloMap.TryGetValue(siloAddress, out var members))
        {
            members.Remove(UserId);
        }
        return Task.CompletedTask;
    }

    public async Task SendMessage(ChatMessage Message)
    {
        foreach (var memberSilo in _memberSiloMap)
        {
            var publisherGrain = _grainFactory.GetGrain<IPublisherGrain>(memberSilo.Key.ToParsableString());
            await publisherGrain.PublishMessage([.. memberSilo.Value], Message);
        }
    }
}

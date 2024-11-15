using RushdownGrainInterfaces;

namespace RushdownGrains;

internal class PublisherGrain(
    [PersistentState("Publisher", "GrainState")] IPersistentState<PublisherGrainState> persistentState)
    : Grain, IPublisherGrain
{
    private readonly IPersistentState<PublisherGrainState> _persistentState = persistentState;

    public async Task Subscribe(IClientCallback observer)
    {
        _persistentState.State.Subscriber = observer;
        await _persistentState.WriteStateAsync();
    }

    public async Task PublishMessage(Guid[] userIds, ChatMessage message)
    {
        if (_persistentState.State.Subscriber != null)
        {
            await _persistentState.State.Subscriber.PublishMessage(userIds, message);
        }
    }
}

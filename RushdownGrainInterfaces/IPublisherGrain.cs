namespace RushdownGrainInterfaces;

[Alias("RushdownGrainInterfaces.IClientCallback")]
public interface IClientCallback : IGrainObserver
{
    [Alias("PublishMessage")]
    Task PublishMessage(Guid[] userIds, ChatMessage message);
}

[GenerateSerializer]
[Alias("RushdownGrainInterfaces.PublisherGrainState")]
public class PublisherGrainState
{
    [Id(0)]
    public IClientCallback? Subscriber { get; set; } = null;
}

[Alias("RushdownGrainInterfaces.ISiloBroadcastGrain")]
public interface IPublisherGrain : IGrainWithStringKey
{
    [Alias("Subscribe")]
    Task Subscribe(IClientCallback observer);

    [Alias("PublishMessage")]
    Task PublishMessage(Guid[] userIds, ChatMessage message);
}

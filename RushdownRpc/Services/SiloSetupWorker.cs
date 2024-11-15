namespace RushdownRpc.Services;

internal class SiloSetupWorker(IWebSocketService webSocketService) : IHostedService
{
    private readonly IWebSocketService _webSocketService = webSocketService;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _webSocketService.Init();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

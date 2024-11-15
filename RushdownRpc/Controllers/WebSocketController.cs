using Microsoft.AspNetCore.Mvc;
using RushdownRpc.Services;

namespace RushdownRpc.Controllers;

public class WebSocketController(IWebSocketService webSocketService) : ControllerBase
{
    private readonly IWebSocketService _webSocketService = webSocketService;

    [Route("/ws")]
    public async Task WebSocket([FromQuery] Guid userId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var cts = new CancellationTokenSource();
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketService.AddConnection(userId, webSocket, cts);
            await _webSocketService.ReceiveLoopAsync(userId, webSocket, cts);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}

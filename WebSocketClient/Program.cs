using WebSocketClient;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

using var client = new ClientWebSocket();
Console.CursorVisible = true;
var userId = Guid.NewGuid().ToString();
var port = 5000;
var uri = new Uri($"ws://localhost:{port}/ws?userId={userId}");
var cts = new CancellationTokenSource();
var bufferSizeKb = 1024 * 4;

await client.ConnectAsync(
    uri: uri,
    cancellationToken: cts.Token);

Console.WriteLine($"Hello user {userId}.");
Console.WriteLine($"You are connected to a server.");

var buffer = new byte[bufferSizeKb];

_ = Task.Run(async () =>
{
    while (!cts.IsCancellationRequested)
    {
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        if (string.Equals(input, "/quit", StringComparison.OrdinalIgnoreCase))
        {
            cts.Cancel();
        }

        Command cmd;

        if (input.StartsWith("/channel"))
        {
            var channelName = input.Replace("/channel ", "");
            if (string.IsNullOrWhiteSpace(channelName))
            {
                Console.WriteLine("Invalid channel name.");
                continue;
            }
            cmd = new Command(type: "channel", data: channelName);
        }
        else
        {
            cmd = new Command(type: "chat_message", data: input);
        }

        var message = JsonConvert.SerializeObject(cmd);

        //Send a message to the server
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await client.SendAsync(
            buffer: new ArraySegment<byte>(messageBytes),
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: cts.Token);
    }
});

while (!cts.IsCancellationRequested)
{
    // Receive response
    WebSocketReceiveResult result = await client.ReceiveAsync(
        buffer: new ArraySegment<byte>(buffer),
        cancellationToken: cts.Token);

    var chatMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
    var deserializedChatMessage = JsonConvert.DeserializeObject<ChatMessage>(chatMessage);

    if (!deserializedChatMessage.IsValid() && !string.Equals(deserializedChatMessage.SenderId, userId))
    {
        Console.WriteLine($"{deserializedChatMessage.SenderId} -> {deserializedChatMessage.Message}");
    }
}

// Close the connection
await client.CloseAsync(
    closeStatus: WebSocketCloseStatus.NormalClosure,
    statusDescription: "Closing",
    cancellationToken: CancellationToken.None);

Console.WriteLine("WebSocket connection closed.");
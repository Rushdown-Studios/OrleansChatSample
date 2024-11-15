using RushdownRpc.Services;
using System.Net;
using System.Net.Sockets;

var hostName = Dns.GetHostName();
var ip = Dns.GetHostEntry(hostName).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

var siloPort = int.Parse(Environment.GetEnvironmentVariable("SILO_PORT") ?? throw new Exception());
var gatewayPort = int.Parse(Environment.GetEnvironmentVariable("GATEWAY_PORT") ?? throw new Exception());

var dynamoDb = Environment.GetEnvironmentVariable("DYNAMODB_CONNECTION_STRING");

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleans(siloBuilder =>
{
    siloBuilder.UseDynamoDBClustering(options =>
    {
        options.Service = dynamoDb;
    })
    .AddDynamoDBGrainStorage("GrainState", options =>
    {
        options.Service = dynamoDb;
    })
    .ConfigureEndpoints(siloPort, gatewayPort, AddressFamily.InterNetwork);
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
builder.Services.AddHostedService<SiloSetupWorker>();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

var app = builder.Build();
app.UseWebSockets(webSocketOptions);
app.MapControllers();
app.UseRouting();
app.Run();
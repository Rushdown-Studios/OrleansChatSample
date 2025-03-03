using RushdownRpc.Services;
using RushdownRpc;
using System.Net.Sockets;

var siloPort = int.Parse(Environment.GetEnvironmentVariable(Names.SiloPort) 
    ?? throw new Exception($"{Names.SiloPort} missing!"));

var gatewayPort = int.Parse(Environment.GetEnvironmentVariable(Names.GatewayPort) 
    ?? throw new Exception($"{Names.GatewayPort} missing!"));

var dynamoDb = Environment.GetEnvironmentVariable(Names.DynamoDbConnectionString) 
    ?? throw new Exception($"{Names.DynamoDbConnectionString} missing!");

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleans(siloBuilder =>
{
    siloBuilder.UseDynamoDBClustering(options => { options.Service = dynamoDb; })
        .AddDynamoDBGrainStorage("GrainState", options => { options.Service = dynamoDb; })
        .ConfigureEndpoints(siloPort, gatewayPort, AddressFamily.InterNetwork);
});

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = false;
    options.SingleLine = true;
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
builder.Services.AddHostedService<SiloSetupWorker>();

var app = builder.Build();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(2) });
app.MapControllers();
app.UseRouting();
app.Run();

public partial class Program;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("WebSocket connection closed by the client.");
    }
});

app.Run();

using Tetrispp.Services;

namespace Tetrispp;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<GameConnectionManager>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.Use(async (context, next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var connectionManager = app.ApplicationServices.GetService<GameConnectionManager>();
                await connectionManager!.HandlePlayer(webSocket);
            } else
            {
                await next();
            }
        });
    }
}
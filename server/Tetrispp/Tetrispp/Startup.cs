using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tetrispp.Data;
using Tetrispp.Services;
using Tetrispp.Tetris.Randomizer;

namespace Tetrispp;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<GameConnectionManager>();
        services.AddScoped<IRandomizer, PickOneRandomizer>();

        services.AddDbContext<SqlContext>(options =>
        {
            options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddSingleton<MigrationManagerService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Auth:Secret"]!)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddControllers();
        services.AddScoped<AuthService>();
        services.AddScoped<ScoreService>();
    }

    public void Configure(IApplicationBuilder app)
    {
        var migrationManager = app.ApplicationServices.GetRequiredService<MigrationManagerService>();
        migrationManager.Start().Wait();

        app.UseCors(x => x
           .AllowAnyMethod()
           .AllowAnyHeader()
           .SetIsOriginAllowed(_ => true)
           .AllowCredentials());

        app.UseRouting();
        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                // extract token vom query string (der Token wird im Client bei der ws_url angehängt)
                string token = context.Request.Query["token"]!;

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Authentication token required");
                    return;
                }

                // Prüfen, ob ein Spectator-Parameter existiert (wird im Client mitgegeben)
                bool isSpectator = context.Request.Query.ContainsKey("spectate");
                string roomId = context.Request.Query["roomId"].ToString();

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var connectionManager = app.ApplicationServices.GetService<GameConnectionManager>();

                if (isSpectator && !string.IsNullOrEmpty(roomId))
                {
                    await connectionManager!.HandleSpectator(webSocket, token, roomId);
                } else
                {
                    await connectionManager!.HandlePlayer(webSocket, token);
                }
            } else
            {
                await next();
            }
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
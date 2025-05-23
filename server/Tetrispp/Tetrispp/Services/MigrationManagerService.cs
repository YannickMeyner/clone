using Microsoft.EntityFrameworkCore;
using Tetrispp.Data;

namespace Tetrispp.Services;

public class MigrationManagerService
{
    private readonly ILogger<MigrationManagerService> logger;
    private readonly IServiceProvider serviceProvider;

    public MigrationManagerService(ILogger<MigrationManagerService> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public Task Start()
    {
        try
        {
            logger.LogInformation("checking for needed migrations");

            var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<SqlContext>();

            service.Database.Migrate();

            logger.LogInformation("migrations finished");

            return Task.CompletedTask;
        } catch (Exception ex)
        {
            logger.LogError(ex, "error occured with exception");
            return Task.CompletedTask;
        }
    }
}
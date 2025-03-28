using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ParticipantManager.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ParticipantManagerDbContext>
{
    public ParticipantManagerDbContext CreateDbContext(string[] args)
    {
        // Build configuration to access appsettings.json or environment variables
        var connectionString = Environment.GetEnvironmentVariable("ParticipantManagerDatabaseConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ParticipantManagerDatabase' is not configured.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ParticipantManagerDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ParticipantManagerDbContext(optionsBuilder.Options);
    }
}

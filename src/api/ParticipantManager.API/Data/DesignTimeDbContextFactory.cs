using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParticipantManager.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ParticipantManagerDbContext>
{
    public ParticipantManagerDbContext CreateDbContext(string[] args)
    {
        // var connectionString = Environment.GetEnvironmentVariable("ParticipantManagerDatabaseConnectionString");
        var connectionString = "Host=localhost;Port=5432;Database=participant_database;Username=postgres;Password=yourpassword;";
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ParticipantManagerDatabase' is not configured.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ParticipantManagerDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ParticipantManagerDbContext(optionsBuilder.Options);
    }
}

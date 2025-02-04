namespace ParticipantManager.API.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ParticipantManagerDbContext>
  {
    public ParticipantManagerDbContext CreateDbContext(string[] args)
    {
      // Build configuration to access appsettings.json or environment variables
      var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

      var connectionString = configuration["ConnectionStrings:ParticipantManagerDatabase"];
      if (string.IsNullOrEmpty(connectionString))
      {
        throw new InvalidOperationException("Connection string 'ParticipantManagerDatabase' is not configured.");
      }

      var optionsBuilder = new DbContextOptionsBuilder<ParticipantManagerDbContext>();
      optionsBuilder.UseSqlServer(connectionString);

      return new ParticipantManagerDbContext(optionsBuilder.Options);
    }
  }

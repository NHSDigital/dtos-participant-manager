using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParticipantManager.Experience.API.Client;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;


var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices(services =>
  {
    services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    {
      client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
    });

    services.AddSingleton<IJwksProvider>(provider =>
    {
      var logger = provider.GetRequiredService<ILogger<JwksProvider>>();
      string issuer = Environment.GetEnvironmentVariable("OAUTH_ISSUER");
      return new JwksProvider(logger, issuer);
    });

    services.AddSingleton<ITokenService,TokenService>();

    services.AddAuthorization();
  })
  .Build();

host.Run();

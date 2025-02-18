using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParticipantManager.Experience.API.Client;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using ParticipantManager.Experience.API.Services;


var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices(services =>
  {
    services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    {
      client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
    });

    string keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
    if (string.IsNullOrEmpty(keyVaultUrl))
    {
      throw new InvalidOperationException("KeyVaultUrl is not set in environment variables.");
    }
    services.AddSingleton(new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential()));
    services.AddSingleton<ITokenService,TokenService>();
    services.AddAuthorization();
  })
  .Build();

host.Run();

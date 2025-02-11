using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ParticipantManager.Experience.API.Client;

var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices(services =>
  {
    services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    {
      client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiClients:ParticipantManagerApiClientUri") ?? string.Empty);
    });
    // Configure Authentication
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.Authority = Environment.GetEnvironmentVariable("OIDC_Authority");
        options.Audience = Environment.GetEnvironmentVariable("OIDC_Audience");
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true
        };
      });

    services.AddAuthorization();
  })
  .Build();

host.Run();

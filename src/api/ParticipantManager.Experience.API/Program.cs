using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ParticipantManager.Experience.API.Client;

var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices(services =>
  {
    services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    {
      client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
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

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS installer-env
WORKDIR /src/dotnet-function-app

COPY ./ParticipantManager.Shared /ParticipantManager.Shared
COPY ["ParticipantManager.API.csproj", "./"]
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS production
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Production
COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]

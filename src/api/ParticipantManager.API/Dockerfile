FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env
WORKDIR /src/dotnet-function-app

COPY ["ParticipantManager.API.csproj", "./"]
RUN dotnet restore

WORKDIR /src/dotnet-function-app
COPY . .
RUN dotnet publish --no-restore -c release -o /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]

# RUN mkdir -p /azure-functions-host/Secrets/

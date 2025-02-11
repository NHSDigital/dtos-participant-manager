# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS installer-env
WORKDIR /src/dotnet-function-app

COPY ["ParticipantManager.Experience.API.csproj", "./"]
RUN dotnet restore

RUN apt-get update \
    && apt-get -y install curl gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get -y install nodejs \
    && npm i -g azure-functions-core-tools@4 --unsafe-perm true

COPY . .

#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
#COPY --from=installer-env /src/dotnet-function-app .
ENV AzureWebJobsScriptRoot=/src/dotnet-function-app \
    DOTNET_USE_POLLING_FILE_WATCHER=1 \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "watch", "run", "--no-restore"]

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS installer-env
WORKDIR /src/dotnet-function-app

COPY ./ParticipantManager.Shared/ParticipantManager.Shared.csproj ../ParticipantManager.Shared/
COPY ./ParticipantManager.EventHandler/ParticipantManager.EventHandler.csproj .
RUN dotnet restore

COPY ./ParticipantManager.Shared/ ../ParticipantManager.Shared/
COPY ./ParticipantManager.EventHandler/ .

RUN dotnet publish -c Release -o /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS production
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Production

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
RUN mkdir -p /home/LogFiles /azure-functions-host /workers && \
    chown -R appuser:appgroup /home /azure-functions-host /workers

USER appuser

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]

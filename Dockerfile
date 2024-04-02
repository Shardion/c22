FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
COPY . ./
RUN dotnet restore
RUN dotnet publish Shardion.Achromatic/ -c Release -o /out

FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
USER $APP_UID
COPY --from=build-env --chown=$APP_UID --chmod=700 /out /achromatic
WORKDIR /achromatic
ENTRYPOINT ["dotnet", "Shardion.Achromatic.dll"]

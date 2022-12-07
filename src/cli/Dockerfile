FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore
RUN dotnet publish -c release -o /source --no-restore

FROM mcr.microsoft.com/dotnet/nightly/runtime:7.0
WORKDIR /app
COPY --from=build /source ./
ENTRYPOINT ["dotnet", "cli.dll"]

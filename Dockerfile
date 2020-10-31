FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Restore the project
COPY . ./
RUN dotnet restore

# Build the project
RUN dotnet publish BeatTogether.MasterServer -c Release -p:PublishReadyToRun=true -r linux-x64 -o out

# Run the application
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "/app/BeatTogether.MasterServer.dll"]

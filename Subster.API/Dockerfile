# Build environment (Has SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
WORKDIR /app/Subster.API

# Restore dependencies
RUN dotnet restore

# Build & publish the API
RUN dotnet publish -c Release -o /app/out

# Final container (Use SDK, not just runtime)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .

# Expose API port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "Subster.API.dll"]

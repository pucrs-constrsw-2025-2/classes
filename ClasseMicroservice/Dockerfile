FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todo o repositório de ClasseMicroservice
COPY . .

# Restore and publish the API project
RUN dotnet restore "src/ClasseMicroservice.API/ClasseMicroservice.API.csproj"
RUN dotnet publish "src/ClasseMicroservice.API/ClasseMicroservice.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Expose the port expected by the service (internal)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ClasseMicroservice.API.dll"]
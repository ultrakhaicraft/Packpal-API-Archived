# Build stage
# Use the official Node.js image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy csproj files and restore dependencies
COPY Packpal/*.csproj Packpal/
COPY Packpal.Repository/*.csproj Packpal.Repository/
COPY Packpal.Service/*.csproj Packpal.Service/
RUN dotnet restore Packpal/Packpal.csproj

# Copy the rest of the application code
COPY . .

# Publish app
RUN dotnet publish Packpal/Packpal.csproj -c Release -o /app/publish

#---------------------------------------
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (default for ASP.NET Core is 8080 in container)
EXPOSE 8080

# Set environment variables (PORT and ENV)
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Run app
ENTRYPOINT ["dotnet", "Packpal.dll"]

# Labels
LABEL version="1.0" \
      description="Packpal .NET API with Swagger" \
      maintainer="Packpal"

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -fs http://localhost:8080/swagger/index.html || exit 1

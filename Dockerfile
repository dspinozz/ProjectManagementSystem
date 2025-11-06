# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ProjectManagementSystem.sln .
COPY src/ProjectManagementSystem.Domain/ProjectManagementSystem.Domain.csproj src/ProjectManagementSystem.Domain/
COPY src/ProjectManagementSystem.Application/ProjectManagementSystem.Application.csproj src/ProjectManagementSystem.Application/
COPY src/ProjectManagementSystem.Infrastructure/ProjectManagementSystem.Infrastructure.csproj src/ProjectManagementSystem.Infrastructure/
COPY src/ProjectManagementSystem.API/ProjectManagementSystem.API.csproj src/ProjectManagementSystem.API/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY src/ProjectManagementSystem.Domain/ src/ProjectManagementSystem.Domain/
COPY src/ProjectManagementSystem.Application/ src/ProjectManagementSystem.Application/
COPY src/ProjectManagementSystem.Infrastructure/ src/ProjectManagementSystem.Infrastructure/
COPY src/ProjectManagementSystem.API/ src/ProjectManagementSystem.API/

# Build the application
WORKDIR /src/src/ProjectManagementSystem.API
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directory for uploads
RUN mkdir -p /app/uploads

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "ProjectManagementSystem.API.dll"]


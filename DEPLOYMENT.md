# Deployment Guide

## Docker Deployment

### Prerequisites
- Docker and Docker Compose installed
- .NET 8.0 SDK (for local development)

### Quick Start with Docker Compose

1. **Clone the repository**
   ```bash
   git clone https://github.com/dspinozz/ProjectManagementSystem.git
   cd ProjectManagementSystem
   ```

2. **Set environment variables (optional)**
   ```bash
   export JWT_SECRET_KEY="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
   ```

3. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

4. **Access the API**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - Health Check: http://localhost:8080/health

### Docker Build Only

1. **Build the image**
   ```bash
   docker build -t projectmanagement-api .
   ```

2. **Run the container**
   ```bash
   docker run -d \
     -p 8080:8080 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e ConnectionStrings__DefaultConnection="Data Source=/app/data/projectmanagement.db" \
     -e Database__Provider=Sqlite \
     -v $(pwd)/data:/app/data \
     -v $(pwd)/uploads:/app/uploads \
     --name projectmanagement-api \
     projectmanagement-api
   ```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Production` for production deployment
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Database__Provider`: Database provider (`Sqlite`, `SqlServer`, or `PostgreSQL`)
- `JwtSettings__SecretKey`: JWT signing key (minimum 32 characters)
- `JwtSettings__Issuer`: JWT issuer
- `JwtSettings__Audience`: JWT audience
- `JwtSettings__ExpirationMinutes`: Token expiration time in minutes

### Production Considerations

1. **Use a strong JWT secret key** - Generate a secure random key
2. **Configure HTTPS** - Use a reverse proxy (nginx, Traefik) with SSL certificates
3. **Database** - For production, consider PostgreSQL or SQL Server instead of SQLite
4. **File Storage** - Configure persistent storage for uploads
5. **Logging** - Set up centralized logging (Serilog, Application Insights, etc.)
6. **Monitoring** - Add health checks and monitoring tools

### Health Check

The API includes a health check endpoint:
```bash
curl http://localhost:8080/health
```

### Stopping the Service

```bash
docker-compose down
```

Or for a single container:
```bash
docker stop projectmanagement-api
docker rm projectmanagement-api
```


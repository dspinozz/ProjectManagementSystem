# Testing Guide

## Test Status

### Unit Tests ✅
- **Status**: All passing (22/22)
- **Coverage**: Services, Infrastructure components
- **Run**: `dotnet test tests/ProjectManagementSystem.UnitTests`

### Integration Tests ⚠️
- **Status**: 2/34 passing (authentication issues in test environment)
- **Note**: Integration tests have known issues with ASP.NET Core Identity password hashing in in-memory database test scenarios
- **Alternative**: Use manual testing via Swagger UI or curl (see below)

## Manual Testing

### Option 1: Swagger UI (Recommended)

1. Start the server:
   ```bash
   cd src/ProjectManagementSystem.API
   dotnet run
   ```

2. Open Swagger UI:
   - Navigate to: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)
   - All API endpoints are available for interactive testing
   - You can register users, login, and test all endpoints

### Option 2: Curl Testing

#### 1. Start the Server
```bash
cd src/ProjectManagementSystem.API
dotnet run
```

The server will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

#### 2. Register a New User
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test1234!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

Response will include a JWT token:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "...",
    "email": "testuser@example.com",
    ...
  }
}
```

#### 3. Login
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test1234!"
  }'
```

#### 4. Get Organizations (Authenticated)
```bash
# Replace YOUR_TOKEN with the token from login/register
curl -X GET "http://localhost:5000/api/organizations" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### 5. Create Organization (Admin Required)
```bash
curl -X POST "http://localhost:5000/api/organizations" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Organization",
    "description": "Organization description"
  }'
```

#### 6. Get Projects
```bash
curl -X GET "http://localhost:5000/api/projects" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### 7. Create Project
```bash
curl -X POST "http://localhost:5000/api/projects" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Project",
    "description": "Project description",
    "workspaceId": "WORKSPACE_ID_HERE"
  }'
```

#### 8. Get Tasks by Project
```bash
curl -X GET "http://localhost:5000/api/tasks/project/PROJECT_ID_HERE" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### 9. Create Task
```bash
curl -X POST "http://localhost:5000/api/tasks" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "New Task",
    "description": "Task description",
    "projectId": "PROJECT_ID_HERE",
    "status": 0,
    "priority": 1
  }'
```

#### 10. Upload File
```bash
curl -X POST "http://localhost:5000/api/files/upload/PROJECT_ID_HERE" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@/path/to/your/file.txt"
```

#### 11. Health Check
```bash
curl -X GET "http://localhost:5000/health"
```

## Complete Testing Workflow

1. **Start Server**: `dotnet run` in `src/ProjectManagementSystem.API`
2. **Register/Login**: Get authentication token
3. **Create Organization**: (Admin role required)
4. **Create Workspace**: Within organization
5. **Create Project**: Within workspace
6. **Create Tasks**: Within project
7. **Upload Files**: To project
8. **Test Authorization**: Try accessing endpoints with different roles

## Notes

- All endpoints require authentication except `/api/auth/register` and `/api/auth/login`
- JWT tokens expire after 60 minutes (configurable in `appsettings.json`)
- Swagger UI provides the easiest way to test all endpoints interactively
- For production-like testing, use a real database instead of in-memory


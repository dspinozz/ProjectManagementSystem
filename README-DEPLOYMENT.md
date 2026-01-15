# Local Deployment Guide

## Prerequisites
- .NET 8.0 SDK installed
- SQLite (included with .NET)

## Quick Start

### Option 1: Using the Deployment Script

```bash
# Make script executable (if not already)
chmod +x deploy-local.sh

# Run API only
./deploy-local.sh api

# In another terminal, run UI
./deploy-local.sh ui
```

### Option 2: Manual Deployment

#### Terminal 1 - Start API:
```bash
cd src/ProjectManagementSystem.API
dotnet run --launch-profile http
```

API will be available at:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

#### Terminal 2 - Start UI:
```bash
cd src/ProjectManagementSystem.UI
dotnet run --launch-profile http
```

UI will be available at:
- **Blazor UI**: http://localhost:5002

## Database

The SQLite database (`projectmanagement.db`) will be created automatically on first run in the API directory.

## Testing Member Management Features

1. **Register a new user** at http://localhost:5002/login
2. **Create a project** (you'll be automatically added as ProjectManager)
3. **Navigate to project details** to see the Team Members section
4. **Add members** by clicking "+ Add Member" and searching for users
5. **Update member roles** using the dropdown
6. **Remove members** using the delete button

## API Endpoints for Member Management

- `GET /api/projects/{projectId}/members` - Get all project members
- `POST /api/projects/{projectId}/members` - Add a member
- `PUT /api/projects/{projectId}/members/{userId}` - Update member role
- `DELETE /api/projects/{projectId}/members/{userId}` - Remove member
- `GET /api/users?search={query}` - Search users

## Troubleshooting

### Port Already in Use
If port 5000 or 5002 is already in use, you can change them in:
- `src/ProjectManagementSystem.API/Properties/launchSettings.json`
- `src/ProjectManagementSystem.UI/Properties/launchSettings.json`

### Database Issues
Delete `projectmanagement.db` and restart the API to recreate the database.

### CORS Issues
Ensure the API CORS settings in `appsettings.json` include `http://localhost:5002`

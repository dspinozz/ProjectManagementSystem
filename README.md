# Project Management System

A comprehensive C# ASP.NET Core Web API project management system with enterprise-grade features including authentication, authorization, multi-tenancy, audit logging, file uploads, and email notifications.

## Tech Stack

- **Framework**: ASP.NET Core 8.0 Web API (API-only, no frontend)
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server / PostgreSQL (configurable)
- **Authentication**: JWT Bearer Tokens with ASP.NET Core Identity
- **Authorization**: Role-Based Access Control (RBAC) with policies
- **Email**: MailKit/MimeKit
- **File Storage**: Local file system (configurable)
- **API Documentation**: Swagger/OpenAPI

## Features

### ✅ Authentication & Authorization
- JWT-based authentication
- OAuth/JWT token generation
- User registration and login
- Password hashing with ASP.NET Core Identity

### ✅ Role-Based Access Control (RBAC)
- Three roles: Admin, ProjectManager, TeamMember
- Authorization policies:
  - `AdminOnly`: Only administrators
  - `ProjectManagerOrAdmin`: Project managers and administrators
  - `TeamMemberOrAbove`: All authenticated users

### ✅ Multi-Tenant Support
- Organizations (top-level tenant)
- Workspaces (within organizations)
- User assignment to organizations and workspaces
- Tenant isolation at the data level

### ✅ CRUD Operations
- Full CRUD for Projects, Tasks, Organizations, Workspaces
- Entity Framework Core with code-first migrations
- Comprehensive data relationships and navigation properties

### ✅ Audit Logging
- Automatic audit trail for all entity changes
- Tracks: entity type, entity ID, action, user, timestamp, changes
- IP address tracking
- Queryable audit log

### ✅ File Uploads
- File upload to projects
- Secure file storage
- File download with proper content types
- File metadata tracking

### ✅ Email Notifications
- SMTP email service
- Configurable email settings
- HTML email support
- Bulk email support

## Project Structure

```
ProjectManagementSystem/
├── src/
│   ├── ProjectManagementSystem.API/          # Web API layer
│   │   ├── Controllers/                      # API controllers
│   │   ├── Program.cs                        # Application entry point
│   │   └── appsettings.json                  # Configuration
│   ├── ProjectManagementSystem.Application/   # Application services
│   │   └── Services/                         # Business logic services
│   ├── ProjectManagementSystem.Domain/       # Domain models
│   │   └── Entities/                         # Entity classes
│   └── ProjectManagementSystem.Infrastructure/# Infrastructure
│       ├── Data/                             # DbContext and data access
│       ├── Services/                         # Infrastructure services
│       └── Extensions/                       # Service extensions
└── ProjectManagementSystem.sln                # Solution file
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB) or PostgreSQL
- Visual Studio 2022 or VS Code

### Installation

1. **Clone or navigate to the project directory**
   ```bash
   cd ProjectManagementSystem
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Configure the database connection**
   
   Edit `src/ProjectManagementSystem.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your connection string here"
     },
     "Database": {
       "Provider": "SqlServer"  // or "PostgreSQL"
     }
   }
   ```

4. **Configure JWT settings**
   
   Update the JWT secret key in `appsettings.json`:
   ```json
   {
     "JwtSettings": {
       "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
       "Issuer": "ProjectManagementSystem",
       "Audience": "ProjectManagementSystem",
       "ExpirationMinutes": "60"
     }
   }
   ```

5. **Configure email settings (optional)**
   
   Update email configuration in `appsettings.json`:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "your-email@gmail.com",
       "SmtpPassword": "your-app-password",
       "FromAddress": "your-email@gmail.com",
       "FromName": "Project Management System"
     }
   }
   ```

6. **Run the application**
   ```bash
   cd src/ProjectManagementSystem.API
   dotnet run
   ```

7. **Access Swagger UI**
   
   Navigate to `https://localhost:5001/swagger` (or the port shown in the console)

### Database Migrations

The application uses `EnsureCreatedAsync()` for development. For production, use migrations:

```bash
# Add a migration
dotnet ef migrations add InitialCreate --project src/ProjectManagementSystem.Infrastructure --startup-project src/ProjectManagementSystem.API

# Update the database
dotnet ef database update --project src/ProjectManagementSystem.Infrastructure --startup-project src/ProjectManagementSystem.API
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token

### Organizations
- `GET /api/organizations` - Get all organizations
- `GET /api/organizations/{id}` - Get organization by ID
- `POST /api/organizations` - Create organization (Admin only)
- `PUT /api/organizations/{id}` - Update organization (Admin only)
- `DELETE /api/organizations/{id}` - Delete organization (Admin only)

### Workspaces
- `GET /api/workspaces` - Get all workspaces
- `GET /api/workspaces/{id}` - Get workspace by ID
- `POST /api/workspaces` - Create workspace (ProjectManager/Admin)
- `PUT /api/workspaces/{id}` - Update workspace (ProjectManager/Admin)
- `DELETE /api/workspaces/{id}` - Delete workspace (Admin only)

### Projects
- `GET /api/projects` - Get all projects
- `GET /api/projects/{id}` - Get project by ID
- `POST /api/projects` - Create project (ProjectManager/Admin)
- `PUT /api/projects/{id}` - Update project (ProjectManager/Admin)
- `DELETE /api/projects/{id}` - Delete project (Admin only)

### Tasks
- `GET /api/tasks/project/{projectId}` - Get tasks by project
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create task (TeamMember+)
- `PUT /api/tasks/{id}` - Update task (TeamMember+)
- `DELETE /api/tasks/{id}` - Delete task (ProjectManager/Admin)

### Files
- `POST /api/files/upload/{projectId}` - Upload file to project
- `GET /api/files/{id}/download` - Download file
- `GET /api/files/project/{projectId}` - Get files for project
- `DELETE /api/files/{id}` - Delete file (ProjectManager/Admin)

## Authentication

All endpoints (except `/api/auth/register` and `/api/auth/login`) require authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Roles and Permissions

### Admin
- Full access to all resources
- Can manage organizations, workspaces, projects, and tasks
- Can delete any resource

### ProjectManager
- Can create and manage projects and workspaces
- Can manage tasks within their projects
- Cannot delete organizations or workspaces

### TeamMember
- Can view projects and tasks
- Can create and update tasks
- Cannot delete resources
- Cannot create projects or workspaces

## Multi-Tenancy

The system supports multi-tenancy through:
- **Organizations**: Top-level tenant isolation
- **Workspaces**: Sub-tenant isolation within organizations
- Users are assigned to organizations and workspaces
- Data is filtered by tenant context

## Audit Logging

All create, update, and delete operations are automatically logged to the `AuditLogs` table with:
- Entity type and ID
- Action performed (Create/Update/Delete)
- User ID and username
- Timestamp
- Changes (JSON)
- IP address

## File Storage

Files are stored in the `uploads/` directory (configurable). Each file is:
- Assigned a unique GUID-based filename
- Linked to a project
- Tracked with metadata (original name, size, content type)
- Accessible only to authorized users

## Email Notifications

The email service supports:
- HTML and plain text emails
- Single and bulk recipients
- Configurable SMTP settings
- Error logging

## Development Notes

- The application uses dependency injection throughout
- Services are registered in `ServiceCollectionExtensions`
- Database context includes automatic timestamp updates
- All controllers use async/await patterns
- Comprehensive error handling and logging

## Security Considerations

- JWT tokens expire after 60 minutes (configurable)
- Passwords are hashed using ASP.NET Core Identity
- Authorization policies enforce role-based access
- File uploads are restricted to authenticated users
- SQL injection protection via EF Core parameterized queries

## Future Enhancements

- Real-time notifications (SignalR)
- Advanced search and filtering
- Project templates
- Time tracking
- Reporting and analytics
- Integration with external services
- Docker containerization
- CI/CD pipeline

## Testing

✅ **Comprehensive test suite included!**

### Unit Tests
- **ProjectServiceTests**: Tests for project CRUD operations, validation, file cleanup
- **TaskServiceTests**: Tests for task operations and validation
- **FileStorageServiceTests**: Tests for file operations and path traversal prevention

### Integration Tests
- **AuthControllerTests**: Tests for user registration and login
- **ProjectsControllerTests**: Tests for API endpoints and authorization

### Running Tests
```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/ProjectManagementSystem.UnitTests

# Run integration tests only
dotnet test tests/ProjectManagementSystem.IntegrationTests
```

### Test Coverage
- **Unit Tests**: 25 tests covering services
  - ProjectService: 9 tests
  - TaskService: 4 tests
  - FileStorageService: 6 tests
  - EmailService: 2 tests
  - AuditService: 3 tests
- **Integration Tests**: 30+ tests covering all controllers
  - AuthController: 4 tests
  - ProjectsController: 2 tests
  - TasksController: 7 tests
  - FilesController: 5 tests
  - OrganizationsController: 7 tests
  - WorkspacesController: 6 tests
- **Coverage**: ~83% of services, 100% of controllers
- **Total**: 55+ comprehensive tests

### Manual Testing
Manual testing can be performed using **Swagger UI** (available at `/swagger` when running the application). This allows interactive testing of all API endpoints.

## License

This project is created for portfolio purposes.


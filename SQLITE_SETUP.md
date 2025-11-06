# SQLite Setup Guide

## ✅ SQLite Compatibility

**Yes, SQLite is fully compatible with Entity Framework Core!**

The project now supports SQLite in addition to SQL Server and PostgreSQL.

## 📦 Package Added

- `Microsoft.EntityFrameworkCore.Sqlite` Version 8.0.0

## ⚙️ Configuration

To use SQLite, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=projectmanagement.db"
  },
  "Database": {
    "Provider": "Sqlite"
  }
}
```

## 🚀 Benefits of SQLite

1. **No Server Required** - File-based database
2. **Cross-Platform** - Works on Linux, macOS, Windows
3. **Perfect for Testing** - Easy to reset/clean
4. **Portable** - Database file can be included/excluded from git
5. **Fast** - Great performance for development/testing

## 📝 Connection String Examples

### Basic SQLite
```json
"DefaultConnection": "Data Source=projectmanagement.db"
```

### With Full Path
```json
"DefaultConnection": "Data Source=/path/to/projectmanagement.db"
```

### In-Memory (for testing)
```json
"DefaultConnection": "Data Source=:memory:"
```

## 🔄 Migration Commands

After switching to SQLite:

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project src/ProjectManagementSystem.Infrastructure --startup-project src/ProjectManagementSystem.API

# Apply migration
dotnet ef database update --project src/ProjectManagementSystem.Infrastructure --startup-project src/ProjectManagementSystem.API
```

## ⚠️ Notes

- SQLite has some limitations compared to SQL Server/PostgreSQL:
  - No support for some advanced SQL features
  - Single writer limitation
  - But perfect for development and testing!

- The database file will be created automatically when you run the application

## ✅ Current Status

SQLite support has been added and is ready to use. Just update `appsettings.json` and restart the server!


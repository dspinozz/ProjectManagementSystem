# Quick Start Guide

## 🚀 Start the API Server

```bash
# Option 1: Use the script
./start-server.sh

# Option 2: Manual start
cd src/ProjectManagementSystem.API
dotnet run
```

## 🌐 Access Swagger UI

Once the server is running, open your browser:

**http://localhost:5000/swagger**

or

**https://localhost:5001/swagger**

## 📝 Quick Test Workflow

1. **Open Swagger UI** in your browser
2. **Register a user**:
   - Find `POST /api/auth/register`
   - Click "Try it out"
   - Enter user data, click "Execute"
   - Copy the `token` from response

3. **Authorize**:
   - Click the **"Authorize"** button (🔓) at the top
   - Paste your token in the "Value" field
   - Click "Authorize", then "Close"

4. **Test endpoints**:
   - All endpoints are now listed and testable
   - Click "Try it out" on any endpoint
   - Fill in parameters and click "Execute"
   - See the response below

## 🎯 What You'll See

Swagger UI shows:
- ✅ All API endpoints organized by controller
- ✅ Request/response schemas
- ✅ Example data
- ✅ "Try it out" buttons for each endpoint
- ✅ Authentication support
- ✅ Response codes and examples

## 💡 Key Points

- **No frontend UI** - This is an API-only project
- **Swagger UI IS the web interface** - It's your testing/demo interface
- **Perfect for portfolio** - Shows your API works interactively
- **No additional tools needed** - Everything in the browser

## 📸 For Your Portfolio

Take screenshots of:
1. Swagger UI showing all endpoints
2. Testing an endpoint (request/response visible)
3. The authorization flow
4. Successful API responses

This demonstrates a fully functional, testable API!


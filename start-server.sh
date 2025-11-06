#!/bin/bash

# Quick start script for Project Management System API

echo "Starting Project Management System API..."
echo ""
echo "Swagger UI will be available at:"
echo "  - HTTP:  http://localhost:5000/swagger"
echo "  - HTTPS: https://localhost:5001/swagger"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

cd src/ProjectManagementSystem.API
dotnet run


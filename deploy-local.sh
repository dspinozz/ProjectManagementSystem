#!/bin/bash

# Local Deployment Script for Project Management System
# This script builds and runs the API and UI locally

set -e

echo "🚀 Starting Local Deployment..."

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to check and kill process on port
check_and_free_port() {
    local port=$1
    local service_name=$2
    
    if command -v lsof &> /dev/null; then
        local pid=$(lsof -ti:$port 2>/dev/null)
        if [ ! -z "$pid" ]; then
            echo -e "${YELLOW}⚠️  Port $port is already in use (PID: $pid)${NC}"
            read -p "Kill the process and continue? (y/n): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                kill -9 $pid 2>/dev/null || true
                sleep 1
                echo -e "${GREEN}✅ Port $port freed${NC}"
            else
                echo -e "${RED}❌ Cannot start $service_name - port $port is in use${NC}"
                exit 1
            fi
        fi
    fi
}

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install .NET 8.0 SDK first."
    exit 1
fi

echo -e "${BLUE}📦 Building solution...${NC}"
dotnet build --no-incremental

if [ $? -ne 0 ]; then
    echo "❌ Build failed. Please fix errors and try again."
    exit 1
fi

echo -e "${GREEN}✅ Build successful!${NC}"

# Create data directory if it doesn't exist
mkdir -p data
mkdir -p uploads

echo -e "${BLUE}🗄️  Database will be created automatically on first run${NC}"

# Function to run API
run_api() {
    check_and_free_port 5000 "API"
    echo -e "${YELLOW}🌐 Starting API on http://localhost:5000${NC}"
    echo -e "${YELLOW}📚 Swagger UI: http://localhost:5000/swagger${NC}"
    cd src/ProjectManagementSystem.API
    dotnet run --launch-profile http
}

# Function to run UI
run_ui() {
    check_and_free_port 5002 "UI"
    echo -e "${YELLOW}🖥️  Starting UI on http://localhost:5002${NC}"
    cd src/ProjectManagementSystem.UI
    dotnet run --launch-profile http
}

# Check command line arguments
if [ "$1" == "api" ]; then
    run_api
elif [ "$1" == "ui" ]; then
    run_ui
elif [ "$1" == "both" ]; then
    echo -e "${BLUE}Starting both API and UI...${NC}"
    echo -e "${YELLOW}Note: You'll need to run them in separate terminals${NC}"
    echo ""
    echo "Terminal 1 - API:"
    echo "  ./deploy-local.sh api"
    echo ""
    echo "Terminal 2 - UI:"
    echo "  ./deploy-local.sh ui"
else
    echo "Usage: ./deploy-local.sh [api|ui|both]"
    echo ""
    echo "Options:"
    echo "  api   - Run only the API server"
    echo "  ui    - Run only the UI server"
    echo "  both  - Show instructions for running both"
    echo ""
    echo "Examples:"
    echo "  ./deploy-local.sh api    # Start API on port 5000"
    echo "  ./deploy-local.sh ui     # Start UI on port 5002"
    echo ""
    echo "For running both simultaneously, use two terminals:"
    echo "  Terminal 1: ./deploy-local.sh api"
    echo "  Terminal 2: ./deploy-local.sh ui"
fi

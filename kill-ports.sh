#!/bin/bash

# Quick script to kill processes on ports 5000 and 5002

echo "Checking for processes on ports 5000 and 5002..."

if command -v lsof &> /dev/null; then
    for port in 5000 5002; do
        pid=$(lsof -ti:$port 2>/dev/null)
        if [ ! -z "$pid" ]; then
            echo "Killing process $pid on port $port..."
            kill -9 $pid 2>/dev/null && echo "✅ Port $port freed" || echo "❌ Failed to kill process on port $port"
        else
            echo "✅ Port $port is free"
        fi
    done
else
    echo "lsof command not found. Please install it or manually kill processes."
    echo "You can find processes using: netstat -an | grep LISTEN | grep -E '5000|5002'"
fi

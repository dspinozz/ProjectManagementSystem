#!/bin/bash

# Comprehensive curl test suite for Project Management System API
# Make sure the server is running before executing this script

BASE_URL="${1:-http://localhost:5000}"
TOKEN=""

echo "=========================================="
echo "Project Management System API - Curl Tests"
echo "=========================================="
echo "Base URL: $BASE_URL"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counter
PASSED=0
FAILED=0

test_endpoint() {
    local name="$1"
    local method="$2"
    local endpoint="$3"
    local data="$4"
    local expected_status="$5"
    
    echo -n "Testing $name... "
    
    if [ "$method" = "GET" ]; then
        if [ -z "$TOKEN" ]; then
            response=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL$endpoint")
        else
            response=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL$endpoint" -H "Authorization: Bearer $TOKEN")
        fi
    elif [ "$method" = "POST" ]; then
        if [ -z "$TOKEN" ]; then
            response=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL$endpoint" \
                -H "Content-Type: application/json" \
                -d "$data")
        else
            response=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL$endpoint" \
                -H "Authorization: Bearer $TOKEN" \
                -H "Content-Type: application/json" \
                -d "$data")
        fi
    elif [ "$method" = "PUT" ]; then
        response=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL$endpoint" \
            -H "Authorization: Bearer $TOKEN" \
            -H "Content-Type: application/json" \
            -d "$data")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "\n%{http_code}" -X DELETE "$BASE_URL$endpoint" \
            -H "Authorization: Bearer $TOKEN")
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" = "$expected_status" ]; then
        echo -e "${GREEN}✓ PASS${NC} (Status: $http_code)"
        ((PASSED++))
        return 0
    else
        echo -e "${RED}✗ FAIL${NC} (Expected: $expected_status, Got: $http_code)"
        echo "  Response: $body"
        ((FAILED++))
        return 1
    fi
}

echo "1. Testing Health Check..."
test_endpoint "Health Check" "GET" "/health" "" "200"

echo ""
echo "2. Testing Authentication Endpoints..."

# Register a new user
RANDOM_EMAIL="test$(date +%s)@example.com"
REGISTER_DATA="{\"email\":\"$RANDOM_EMAIL\",\"password\":\"Test1234!\",\"firstName\":\"Test\",\"lastName\":\"User\"}"

echo "Registering user: $RANDOM_EMAIL"
register_response=$(curl -s -X POST "$BASE_URL/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "$REGISTER_DATA")

# Extract token from response
TOKEN=$(echo "$register_response" | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo -e "${RED}Failed to register user or get token${NC}"
    echo "Response: $register_response"
    exit 1
fi

echo -e "${GREEN}✓ Registration successful${NC}"
echo "Token: ${TOKEN:0:50}..."
((PASSED++))

# Test login
LOGIN_DATA="{\"email\":\"$RANDOM_EMAIL\",\"password\":\"Test1234!\"}"
echo "Testing login..."
login_response=$(curl -s -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "$LOGIN_DATA")

login_token=$(echo "$login_response" | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -n "$login_token" ]; then
    echo -e "${GREEN}✓ Login successful${NC}"
    TOKEN="$login_token"
    ((PASSED++))
else
    echo -e "${YELLOW}⚠ Login failed, using registration token${NC}"
    ((FAILED++))
fi

echo ""
echo "3. Testing Organizations Endpoints..."
test_endpoint "Get Organizations" "GET" "/api/organizations" "" "200"
test_endpoint "Get Non-existent Organization" "GET" "/api/organizations/$(uuidgen)" "" "404"

# Create organization (Admin required - may fail if user is not admin)
ORG_DATA="{\"name\":\"Test Organization $(date +%s)\",\"description\":\"Test Description\"}"
test_endpoint "Create Organization" "POST" "/api/organizations" "$ORG_DATA" "201"

echo ""
echo "4. Testing Projects Endpoints..."
test_endpoint "Get Projects" "GET" "/api/projects" "" "200"
test_endpoint "Get Non-existent Project" "GET" "/api/projects/$(uuidgen)" "" "404"

echo ""
echo "5. Testing Tasks Endpoints..."
test_endpoint "Get Tasks (Invalid Project)" "GET" "/api/tasks/project/$(uuidgen)" "" "200"  # Returns empty array

echo ""
echo "6. Testing Files Endpoints..."
test_endpoint "Get Project Files (Invalid Project)" "GET" "/api/files/project/$(uuidgen)" "" "200"  # Returns empty array

echo ""
echo "=========================================="
echo "Test Summary"
echo "=========================================="
echo -e "${GREEN}Passed: $PASSED${NC}"
echo -e "${RED}Failed: $FAILED${NC}"
echo "Total: $((PASSED + FAILED))"
echo ""

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}All tests passed!${NC}"
    exit 0
else
    echo -e "${YELLOW}Some tests failed. This may be expected for certain endpoints.${NC}"
    exit 1
fi


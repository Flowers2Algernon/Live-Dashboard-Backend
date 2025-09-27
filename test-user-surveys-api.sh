#!/bin/bash

# 用户调查 API 测试脚本
BASE_URL="https://live-dashboard-backend-production.up.railway.app"

echo "🚀 测试用户调查 API..."
echo "📍 Base URL: $BASE_URL"
echo ""

# 颜色代码
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 测试函数
test_api() {
    local method=$1
    local endpoint=$2
    local description=$3
    local data=$4
    
    echo -e "${BLUE}测试: $description${NC}"
    echo "请求: $method $endpoint"
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "\n%{http_code}" -X $method "$BASE_URL$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data")
    else
        response=$(curl -s -w "\n%{http_code}" -X $method "$BASE_URL$endpoint" \
            -H "Content-Type: application/json")
    fi
    
    # 分离响应体和状态码
    http_code=$(echo "$response" | tail -n1)
    response_body=$(echo "$response" | head -n -1)
    
    # 格式化JSON响应
    formatted_json=$(echo "$response_body" | python3 -m json.tool 2>/dev/null || echo "$response_body")
    
    if [ "$http_code" -eq 200 ]; then
        echo -e "${GREEN}✓ 成功 (HTTP $http_code)${NC}"
        echo "响应: $formatted_json"
    else
        echo -e "${RED}✗ 失败 (HTTP $http_code)${NC}"
        echo "响应: $formatted_json"
    fi
    echo ""
}

# 示例用户ID（需要根据实际数据库中的用户来调整）
USER_ID_1="123e4567-e89b-12d3-a456-426614174001"  # 示例用户ID 1
USER_ID_2="123e4567-e89b-12d3-a456-426614174002"  # 示例用户ID 2
INVALID_USER_ID="00000000-0000-0000-0000-000000000000"  # 无效用户ID

echo "=== 用户调查 API 测试 ==="
echo ""

# 1. 测试获取用户所有调查
test_api "GET" "/api/users/$USER_ID_1/surveys" "获取用户1的所有调查"

# 2. 测试获取用户默认调查
test_api "GET" "/api/users/$USER_ID_1/surveys/default" "获取用户1的默认调查"

# 3. 测试无效用户ID
test_api "GET" "/api/users/$INVALID_USER_ID/surveys" "使用无效用户ID获取调查"

# 4. 测试空GUID
test_api "GET" "/api/users/00000000-0000-0000-0000-000000000000/surveys/default" "使用空GUID获取默认调查"

# 5. 测试另一个用户
test_api "GET" "/api/users/$USER_ID_2/surveys" "获取用户2的所有调查"

echo "=== 测试完成 ==="
echo ""
echo "📋 API 端点总结:"
echo "1. GET /api/users/{userId}/surveys - 获取用户的所有调查"
echo "2. GET /api/users/{userId}/surveys/default - 获取用户的默认调查"
echo ""
echo "💡 使用说明:"
echo "- 将上述用户ID替换为数据库中的真实用户ID"
echo "- 确保数据库中有用户和调查数据"
echo "- 检查用户是否处于活跃状态（is_active = true）"

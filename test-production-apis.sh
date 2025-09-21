#!/bin/bash

# Railway 生产环境 API 测试脚本
# 您的部署 URL: https://live-dashboard-backend-production.up.railway.app

BASE_URL="https://live-dashboard-backend-production.up.railway.app"
SURVEY_ID="8dff523d-2a46-4ee3-8017-614af3813b32"

echo "🚀 开始测试 Railway 生产环境 APIs..."
echo "📍 Base URL: $BASE_URL"
echo "📊 Survey ID: $SURVEY_ID"
echo ""

# 颜色代码
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 测试函数
test_api() {
    local name="$1"
    local url="$2"
    
    echo -e "${BLUE}🧪 测试: $name${NC}"
    echo "📡 URL: $url"
    
    response=$(curl -s -w "HTTPSTATUS:%{http_code}" "$url" -H "accept: application/json")
    http_code=$(echo $response | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    body=$(echo $response | sed -e 's/HTTPSTATUS\:.*//g')
    
    if [ "$http_code" -eq 200 ]; then
        echo -e "${GREEN}✅ 状态: $http_code OK${NC}"
        echo "$body" | jq -r '.success, .message' > /dev/null 2>&1
        if [ $? -eq 0 ]; then
            success=$(echo "$body" | jq -r '.success')
            message=$(echo "$body" | jq -r '.message')
            if [ "$success" = "true" ]; then
                echo -e "${GREEN}✅ API 响应: 成功${NC}"
            else
                echo -e "${YELLOW}⚠️  API 响应: 失败 - $message${NC}"
            fi
        else
            echo -e "${YELLOW}⚠️  响应格式: 非 JSON 或格式错误${NC}"
        fi
    else
        echo -e "${RED}❌ 状态: $http_code ERROR${NC}"
        echo -e "${RED}响应: $body${NC}"
    fi
    echo ""
}

# 1. 健康检查
test_api "Health Check" "$BASE_URL/api/health"

# 2. Response Chart API
test_api "Response Chart - 全部参与者" "$BASE_URL/api/charts/response?surveyId=$SURVEY_ID"

test_api "Response Chart - 男性参与者" "$BASE_URL/api/charts/response?surveyId=$SURVEY_ID&gender=1"

test_api "Response Chart - 组合过滤" "$BASE_URL/api/charts/response?surveyId=$SURVEY_ID&gender=1&participantType=1"

# 3. Customer Satisfaction Trend API
test_api "Customer Satisfaction Trend - 全部" "$BASE_URL/api/charts/customer-satisfaction-trend?surveyId=$SURVEY_ID"

test_api "Customer Satisfaction Trend - 女性" "$BASE_URL/api/charts/customer-satisfaction-trend?surveyId=$SURVEY_ID&gender=2"

# 4. NPS API
test_api "NPS - 全部参与者" "$BASE_URL/api/charts/nps?surveyId=$SURVEY_ID"

test_api "NPS - 男性参与者" "$BASE_URL/api/charts/nps?surveyId=$SURVEY_ID&gender=1"

test_api "NPS - 女性参与者" "$BASE_URL/api/charts/nps?surveyId=$SURVEY_ID&gender=2"

# 5. Service Attribute API
test_api "Service Attributes - 全部属性" "$BASE_URL/api/charts/service-attributes?surveyId=$SURVEY_ID"

test_api "Service Attributes - 选定属性" "$BASE_URL/api/charts/service-attributes?surveyId=$SURVEY_ID&selectedAttributes=Safety&selectedAttributes=Activities"

test_api "Service Attributes - 组合过滤" "$BASE_URL/api/charts/service-attributes?surveyId=$SURVEY_ID&gender=1&selectedAttributes=Safety&selectedAttributes=Facilities"

# 6. 错误测试
echo -e "${YELLOW}🚫 错误处理测试${NC}"
test_api "无效 Survey ID" "$BASE_URL/api/charts/response?surveyId=invalid-id"

test_api "缺少 Survey ID" "$BASE_URL/api/charts/response"

# 7. CORS 测试
echo -e "${BLUE}🌐 CORS 测试${NC}"
echo "测试 CORS 预检请求..."
cors_response=$(curl -s -w "HTTPSTATUS:%{http_code}" \
    -H "Origin: https://your-frontend.com" \
    -H "Access-Control-Request-Method: GET" \
    -H "Access-Control-Request-Headers: Content-Type" \
    -X OPTIONS \
    "$BASE_URL/api/charts/response")

cors_code=$(echo $cors_response | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
if [ "$cors_code" -eq 200 ] || [ "$cors_code" -eq 204 ]; then
    echo -e "${GREEN}✅ CORS 预检: $cors_code OK${NC}"
else
    echo -e "${RED}❌ CORS 预检: $cors_code ERROR${NC}"
fi
echo ""

echo -e "${GREEN}🎉 测试完成！${NC}"
echo ""
echo -e "${BLUE}📊 如果所有测试都通过，您的 API 已成功部署到 Railway！${NC}"
echo -e "${BLUE}🔗 生产环境地址: $BASE_URL${NC}"
echo ""
echo -e "${YELLOW}💡 前端集成提示:${NC}"
echo "const API_BASE_URL = '$BASE_URL/api';"
echo ""
echo -e "${YELLOW}🛠️  如果遇到问题，请检查:${NC}"
echo "1. Railway 部署日志"
echo "2. 环境变量配置"
echo "3. Supabase 数据库连接"
echo "4. CORS 设置"

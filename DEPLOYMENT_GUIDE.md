# C# .NET Backend Deployment Guide

## ⚠️ 重要说明：Vercel 不支持 C# .NET 

**Vercel 无法部署 C# .NET 应用**，因为它主要支持：
- Node.js 应用（Next.js, Express 等）
- 静态网站 (HTML, CSS, JS)
- Edge Functions (JavaScript/TypeScript)
- Python（有限的 serverless functions）

## 推荐的部署平台

### 1. Railway ⭐ **最佳选择**

**为什么选择 Railway：**
- 原生支持 .NET 应用
- 自动检测项目类型
- 简单的 GitHub 集成
- 免费额度充足
- 提供 HTTPS 域名

#### Railway 部署步骤：

1. **访问 [Railway.app](https://railway.app)**
2. **连接 GitHub 账户**
3. **选择您的仓库**
4. **Railway 自动检测 .NET 项目并部署**
5. **配置环境变量：**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=您的Supabase连接字符串
   PORT=8080
   ```
6. **获取部署URL**（格式：`https://your-app.railway.app`）

### 2. Render ⭐ **第二选择**

#### Render 部署步骤：

1. **访问 [Render.com](https://render.com)**
2. **连接 GitHub 仓库**
3. **选择 "Web Service"**
4. **Render 会自动检测 Dockerfile**
5. **设置环境变量**
6. **部署**

### 3. Azure App Service（微软推荐）

Microsoft 官方推荐的 .NET 部署平台。

### 4. AWS Elastic Beanstalk

AWS 的 PaaS 解决方案，支持 .NET Core。

## 部署前准备

### 1. 验证 Dockerfile
确保项目根目录有正确的 Dockerfile（已存在）：
   - `SUPABASE_ANON_KEY`: Your Supabase anon key
   - `ASPNETCORE_ENVIRONMENT`: Production

### 5. Redeploy with Environment Variables
```bash
vercel --prod
```

## Alternative Deployment Option: Railway

If you prefer Railway, here's the quick setup:
```dockerfile
# 现有的 Dockerfile 已经配置正确
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
# ... 其他配置
```

### 2. 检查 Program.cs 配置

确保包含生产环境所需的配置：

```csharp
// CORS 配置
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "https://your-frontend-domain.com"  // 添加您的前端域名
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// 生产环境端口配置
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
```

### 3. 环境变量设置

在部署平台设置以下环境变量：

```bash
# 基本配置
ASPNETCORE_ENVIRONMENT=Production
PORT=8080

# 数据库连接（Supabase）
ConnectionStrings__DefaultConnection=postgresql://[user]:[password]@[host]:[port]/[database]

# 其他配置
AllowedHosts=*
```

## Railway 部署步骤（推荐）

### 详细步骤：

1. **访问 [Railway.app](https://railway.app)**
2. **点击 "Start a New Project"**
3. **选择 "Deploy from GitHub repo"**
4. **连接您的 GitHub 账户**
5. **选择您的仓库**
6. **Railway 自动检测到 .NET 项目**
7. **在 Variables 标签页添加环境变量**
8. **点击 Deploy**

### 环境变量配置：
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=您的Supabase连接字符串
PORT=8080
```

## Render 部署步骤

1. **访问 [Render.com](https://render.com)**
2. **连接 GitHub 仓库**
3. **选择 "Web Service"**
4. **Render 自动检测 Dockerfile**
5. **设置环境变量**
6. **点击 "Create Web Service"**

## 测试部署

### 🎉 您的生产环境 URL
**Railway 部署地址**: `https://live-dashboard-backend-production.up.railway.app`

### 基础健康检查

```bash
# 测试API是否在线
curl "https://live-dashboard-backend-production.up.railway.app/api/health" \
  -H "accept: application/json"
```

### 核心 API 测试

```bash
# 1. Response Chart API
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" \
  -H "accept: application/json"

# 2. Customer Satisfaction Trend API
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" \
  -H "accept: application/json"

# 3. NPS API
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" \
  -H "accept: application/json"

# 4. Service Attribute API
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" \
  -H "accept: application/json"
```

### 过滤器测试

```bash
# 性别过滤测试
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1" \
  -H "accept: application/json"

# 组合过滤测试
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1" \
  -H "accept: application/json"

# 属性选择测试
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&selectedAttributes=Safety&selectedAttributes=Activities" \
  -H "accept: application/json"
```

### CORS 测试

```bash
# 检查 CORS 配置
curl -H "Origin: https://your-frontend.com" \
     -H "Access-Control-Request-Method: GET" \
     -X OPTIONS \
     "https://live-dashboard-backend-production.up.railway.app/api/charts/response"
```

### 错误处理测试

```bash
# 测试无效 survey ID
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=invalid-id" \
  -H "accept: application/json"

# 测试缺少必需参数
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response" \
  -H "accept: application/json"
```

## 常见问题

### 1. 端口配置错误
确保应用监听 `0.0.0.0:8080` 而不是 `localhost:5000`。

### 2. CORS 问题
添加前端域名到 CORS 策略中。

### 3. 数据库连接失败
检查 Supabase 连接字符串格式和网络访问权限。

### 4. 构建失败
确保所有项目依赖正确配置，Dockerfile 路径正确。

## 前端集成

部署完成后，在前端代码中使用：

```javascript
// 替换本地开发 URL
const API_BASE_URL = process.env.NODE_ENV === 'production' 
  ? 'https://your-app.railway.app/api'
  : 'http://localhost:5000/api';

// API 调用示例
const response = await fetch(`${API_BASE_URL}/charts/customer-satisfaction`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    filters: {
      dateRange: {
        startDate: '2024-01-01',
        endDate: '2024-12-31'
      }
    }
  })
});
```

## 平台比较总结

| 平台 | .NET 支持 | 部署难度 | 免费额度 | HTTPS | 推荐度 |
|------|-----------|----------|----------|--------|--------|
| Railway | ✅ 原生支持 | ⭐⭐⭐⭐⭐ 极简 | 充足 | ✅ | ⭐⭐⭐⭐⭐ |
| Render | ✅ Docker | ⭐⭐⭐⭐ 简单 | 有限 | ✅ | ⭐⭐⭐⭐ |
| Azure App Service | ✅ 原生支持 | ⭐⭐⭐ 中等 | 有限 | ✅ | ⭐⭐⭐ |
| AWS Elastic Beanstalk | ✅ 支持 | ⭐⭐ 复杂 | 有限 | ✅ | ⭐⭐ |
| Vercel | ❌ 不支持 | N/A | N/A | ✅ | ❌ |

## 总结

1. **Vercel 不支持 C# .NET** - 请选择其他平台
2. **推荐使用 Railway** - 最简单的 .NET 部署方案
3. **确保正确配置 CORS** - 允许前端访问
4. **使用环境变量** - 管理敏感配置
5. **测试所有 API 端点** - 确保功能正常

选择 Railway 或 Render 都能很好地支持您的 .NET API + Supabase 架构，并提供前端所需的 HTTPS 访问。

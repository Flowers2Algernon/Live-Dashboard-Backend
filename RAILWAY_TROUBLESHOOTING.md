# Railway 部署问题诊断指南

## 🚨 当前问题
- ✅ 健康检查正常 (200 OK)
- ✅ CORS 配置正确 (204 OK)
- ❌ 所有数据 API 返回 500 错误

## 🔍 问题分析

根据测试结果，问题很可能是**数据库连接配置**。所有需要数据库访问的 API 都失败了。

## 🛠️ 解决步骤

### 1. 检查 Railway 环境变量

在 Railway 控制台中确认以下环境变量已正确设置：

```bash
# 必需的环境变量
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
ConnectionStrings__DefaultConnection=你的Supabase连接字符串
```

### 2. 验证 Supabase 连接字符串格式

您的连接字符串应该类似于：
```
postgresql://postgres.[project-ref]:[password]@aws-0-[region].pooler.supabase.com:6543/postgres
```

或者：
```
postgresql://postgres.[project-ref]:[password]@db.[project-ref].supabase.co:5432/postgres
```

### 3. 在 Railway 中添加/检查环境变量

1. 登录 Railway 控制台
2. 进入您的项目
3. 点击 "Variables" 标签
4. 确认环境变量设置正确

### 4. 重新部署

环境变量更新后，触发重新部署：
```bash
# 在 Railway 控制台点击 "Deploy" 按钮
# 或者推送新的 commit 到 GitHub
```

### 5. 检查 Railway 日志

1. 在 Railway 控制台中查看 "Deployments" 
2. 点击最新的部署
3. 查看 "Logs" 标签
4. 寻找数据库连接错误信息

## 🧪 测试数据库连接

### 简单连接测试

```bash
# 测试基础 API（不需要数据库）
curl "https://live-dashboard-backend-production.up.railway.app/api/health"

# 测试数据库 API（需要数据库连接）
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32"
```

### 预期结果

- 健康检查：应该返回 200 OK
- 数据 API：如果数据库连接正常，应该返回数据或空结果，而不是 500 错误

## 📋 常见问题检查清单

### Supabase 配置
- [ ] 项目是否已暂停？（免费计划有限制）
- [ ] 网络访问是否允许？（检查 IP 白名单）
- [ ] 密码是否正确？
- [ ] 数据库是否存在？

### Railway 配置
- [ ] 环境变量是否正确设置？
- [ ] 部署是否成功完成？
- [ ] 是否有构建错误？

### 连接字符串格式
- [ ] 格式是否正确？
- [ ] 端口是否正确？（5432 或 6543）
- [ ] 项目引用是否正确？

## 🔧 修复建议

### 方案 1：使用 Supabase 直连
```
postgresql://postgres.[project-ref]:[password]@db.[project-ref].supabase.co:5432/postgres
```

### 方案 2：使用 Supabase Pooler
```
postgresql://postgres.[project-ref]:[password]@aws-0-[region].pooler.supabase.com:6543/postgres
```

### 方案 3：检查 Supabase 状态
1. 登录 Supabase 控制台
2. 确认项目状态正常
3. 检查 "Settings" > "Database" 中的连接信息

## 🚀 修复后测试

修复配置后，重新运行测试：
```bash
./test-production-apis.sh
```

## 📞 需要帮助？

如果问题持续存在，请提供：
1. Railway 部署日志
2. Supabase 项目状态
3. 环境变量配置（隐藏敏感信息）

---

## 💡 成功标志

当问题解决后，您应该看到：
- ✅ 健康检查 200 OK
- ✅ 响应图表 API 返回数据
- ✅ 客户满意度 API 返回数据  
- ✅ NPS API 返回数据
- ✅ 服务属性 API 返回数据
- ✅ CORS 204 OK

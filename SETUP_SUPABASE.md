# Supabase 环境配置说明

## 设置步骤

1. **复制环境变量文件**
   ```bash
   cp .env.example .env
   ```

2. **配置你的 Supabase 密码**
   编辑 `.env` 文件，将 `SUPABASE_PASSWORD` 替换为你的实际 Supabase 数据库密码：
   ```env
   SUPABASE_PASSWORD=你的实际密码
   ```

3. **获取 Supabase 密码**
   - 登录你的 Supabase 控制台
   - 进入项目设置 > Database
   - 查看连接字符串获取密码，或重置密码

## 环境变量说明

- `SUPABASE_URL`: Supabase 项目的 URL
- `SUPABASE_ANON_KEY`: 匿名访问密钥（用于前端）
- `SUPABASE_DB_HOST`: 数据库连接池主机地址
- `SUPABASE_DB_PORT`: 数据库连接池端口
- `SUPABASE_PASSWORD`: 数据库连接密码（敏感信息）

## 安全注意事项

- ✅ `.env` 文件已经添加到 `.gitignore` 中
- ✅ 敏感信息不会被提交到版本控制
- ✅ 团队成员需要各自配置自己的 `.env` 文件

## 连接字符串格式

应用程序会自动从环境变量构建 Supabase Transaction Pooler 连接字符串：
```
Host={SUPABASE_DB_HOST};Port={SUPABASE_DB_PORT};Database=postgres;Username=postgres.{项目引用ID};Password={SUPABASE_PASSWORD};SSL Mode=Require
```

这使用了 Supabase 的 **Transaction pooler**，适合无状态应用程序和短暂的数据库交互。

## 验证连接

启动应用程序后，可以通过以下端点验证数据库连接：

- **健康检查**: `GET http://localhost:5153/api/health`
- **数据库连接检查**: `GET http://localhost:5153/api/health/database`
- **组织 API**: `GET http://localhost:5153/api/organizations`

成功连接后，数据库健康检查应该返回：
```json
{
  "status": "Success",
  "message": "数据库连接成功",
  "organisationCount": 1,
  "timestamp": "2025-09-07T09:51:14.931193+08:00"
}
```

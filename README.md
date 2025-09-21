# LERD Backend API

A .NET Core 8 backend API for Live Employee Retention Dashboard (LERD) with Supabase PostgreSQL integration.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Supabase account
- Git

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd livedashboard-stage1-backend
   ```

2. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your Supabase credentials
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   cd LERD_Backend
   dotnet run
   ```

5. **Test the API**
   - Open `LERD_Backend/organizations-test.http` in VS Code
   - Or visit: http://localhost:5153/api/health

## 📁 Project Structure

```
├── LERD_Backend/           # 🎯 API Layer (Controllers, Startup)
├── LERD.Application/       # 🔧 Business Logic (Services, Interfaces)
├── LERD.Domain/           # 📦 Core Entities (Models, Domain Logic)
├── LERD.Infrastructure/   # 🗄️ Data Access (DbContext, Repositories)
├── LERD.Shared/          # 🔗 Shared Models (DTOs, Constants)
├── .env.example          # 📝 Environment Configuration Template
└── organizations-test.http # 🧪 API Testing File
```

## 🔌 API Endpoints

### Health Checks
- `GET /api/health` - Basic health check
- `GET /api/health/database-simple` - Database connectivity test
- `GET /api/health/debug` - Connection configuration info

### Organizations Management
- `GET /api/organizations` - List all organizations (paginated)
- `GET /api/organizations/{id}` - Get organization by ID
- `POST /api/organizations` - Create new organization
- `PATCH /api/organizations/{id}` - Update organization
- `DELETE /api/organizations/{id}` - Soft delete organization

## 🗃️ Database Configuration

### Recommended: Session Pooler (Write Operations)
```env
SUPABASE_DB_HOST=aws-1-ap-southeast-2.pooler.supabase.com
SUPABASE_DB_PORT=5432
```

### Alternative: Transaction Pooler (Read Optimized)
```env
SUPABASE_DB_HOST=aws-1-ap-southeast-2.pooler.supabase.com
SUPABASE_DB_PORT=6543
```

## 📊 API Response Format

```json
{
  "success": true,
  "message": "",
  "data": {
    // Response payload
  }
}
```

## 🧪 Testing

Use the provided HTTP test file:
```bash
# Open in VS Code with REST Client extension
code LERD_Backend/organizations-test.http
```

Or test with curl:
```bash
# Health check
curl http://localhost:5153/api/health

# Get organizations
curl http://localhost:5153/api/organizations
```

## 🛡️ Security

- Environment variables for sensitive data
- UUID-based entity identifiers
- Soft delete for data preservation
- Input validation on all endpoints

## 🔄 Development Workflow

1. **Make changes** to the code
2. **Test locally** using the HTTP test file
3. **Run health checks** to ensure database connectivity
4. **Commit and push** changes

## 📈 Performance Features

- **Smart Pagination**: Avoids expensive COUNT queries
- **Connection Pooling**: Optimized for Supabase
- **Efficient Queries**: Minimal database round trips
- **Error Handling**: Comprehensive exception management

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Test all endpoints
4. Submit a pull request

## 📋 Environment Variables

See `.env.example` for required configuration:

- `SUPABASE_URL` - Your Supabase project URL
- `SUPABASE_ANON_KEY` - Supabase anonymous key
- `SUPABASE_DB_HOST` - Database host (pooler)
- `SUPABASE_DB_PORT` - Database port (5432 or 6543)
- `SUPABASE_PASSWORD` - Database password

## 🏗️ Built With

- **.NET 8.0** - Backend framework
- **Entity Framework Core** - ORM
- **Npgsql** - PostgreSQL driver
- **Supabase** - Backend-as-a-Service
- **DotNetEnv** - Environment variable management

---

For detailed implementation notes, see [RELEASE_NOTES.md](RELEASE_NOTES.md)

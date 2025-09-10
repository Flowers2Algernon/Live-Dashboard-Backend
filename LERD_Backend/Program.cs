using LERD_Backend.Services;
using LERD.Application.Interfaces;
using LERD.Application.Services;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables from .env file (only in development)
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
    }
    else
    {
        // 尝试从当前目录加载
        var currentDirEnv = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(currentDirEnv))
        {
            Env.Load(currentDirEnv);
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>(); // 新增订阅服务
builder.Services.AddScoped<IResponseChartService, ResponseChartService>();
builder.Services.AddScoped<ICustomerSatisfactionService, CustomerSatisfactionService>();
builder.Services.AddScoped<ICustomerSatisfactionTrendService, CustomerSatisfactionTrendService>();
builder.Services.AddScoped<INPSService, NPSService>();
builder.Services.AddScoped<IServiceAttributeService, ServiceAttributeService>();

// 从环境变量或配置构建数据库连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

// 检查是否是PostgreSQL URI格式 (postgresql://...)
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    // Railway提供的PostgreSQL URI格式，转换为Npgsql格式
    try
    {
        var uri = new Uri(connectionString);
        var host = uri.Host;
        var port = uri.Port != -1 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = uri.UserInfo.Split(':')[0];
        var password = uri.UserInfo.Split(':')[1];
        
        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to parse PostgreSQL URI: {ex.Message}");
        connectionString = null; // 让它fallback到组件方式
    }
}

// Fallback to building connection string from individual components if direct connection string not available
if (string.IsNullOrEmpty(connectionString))
{
    var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
    var supabasePassword = Environment.GetEnvironmentVariable("SUPABASE_PASSWORD");
    var dbHost = Environment.GetEnvironmentVariable("SUPABASE_DB_HOST");
    var dbPort = Environment.GetEnvironmentVariable("SUPABASE_DB_PORT");

    // 从 Supabase URL 中提取项目引用 ID
    var hostName = supabaseUrl?.Replace("https://", "").Replace("http://", "");
    var projectRef = hostName?.Split('.')[0]; // 获取项目引用 ID

    // 使用环境变量中的 Supabase Transaction pooler 连接字符串格式
    connectionString = $"Host={dbHost};Port={dbPort};Database=postgres;Username=postgres.{projectRef};Password={supabasePassword};SSL Mode=Require";
}

// 确保有有效的连接字符串
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured. Please set SUPABASE_CONNECTION_STRING environment variable.");
}

// 数据库连接
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add CORS policy for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // 开发环境允许本地域名
            policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001", 
                "https://localhost:3000",
                "http://127.0.0.1:3000"
            );
        }
        else
        {
            // 生产环境从环境变量读取允许的域名
            var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
                               ?? new[] { "https://your-frontend-domain.vercel.app" };
            policy.WithOrigins(allowedOrigins);
        }
        
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 配置端口 - 云部署需要监听所有接口
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (!builder.Environment.IsDevelopment())
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
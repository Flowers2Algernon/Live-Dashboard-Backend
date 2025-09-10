using LERD_Backend.Services;
using LERD.Application.Interfaces;
using LERD.Application.Services;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using LERD.Utils;

// Load environment variables from .env file
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
    else
    {
        Env.Load();
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>(); // 新增订阅服务

builder.Services.AddScoped<JwtHelper>();
// 从环境变量构建数据库连接字符串
var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
var supabasePassword = Environment.GetEnvironmentVariable("SUPABASE_PASSWORD");
var dbHost = Environment.GetEnvironmentVariable("SUPABASE_DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("SUPABASE_DB_PORT");

// 从 appsettings.json 中读取连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 从 Supabase URL 中提取项目引用 ID
var hostName = supabaseUrl?.Replace("https://", "").Replace("http://", "");
var projectRef = hostName?.Split('.')[0]; // 获取项目引用 ID

// 使用环境变量中的 Supabase Transaction pooler 连接字符串格式
// var connectionString = $"Host={dbHost};Port={dbPort};Database=postgres;Username=postgres.{projectRef};Password={supabasePassword};SSL Mode=Require";

// 数据库连接
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
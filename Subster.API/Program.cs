using System.Net.Http.Headers;
using System.Text;

using DotNetEnv;

using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Subster.API.Clients;
using Subster.API.Filters;
using Subster.API.Jobs;
using Subster.API.Middleware;
using Subster.API.Services;
using Subster.API.Services.Implementations;
using Subster.API.Services.Interfaces;
using Subster.DAL;
using Subster.DAL.Implementations;
using Subster.DAL.Interfaces;
using Subster.DAL.Utilities;
using Subster.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Bind to Render's PORT on all interfaces
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// JWT settings and authentication
IConfigurationSection jwtSettings = builder.Configuration.GetSection("JwtSettings")
    ?? throw new Exception("JWT settings not configured");
var jwtSecret = jwtSettings["SecretKey"] ?? throw new Exception("JWT secret key not found");
var jwtSecretBytes = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt"))
                {
                    context.Token = context.Request.Cookies["jwt"];
                }
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(jwtSecretBytes),
            ValidateIssuer           = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidateAudience         = true,
            ValidAudience            = jwtSettings["Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Health checks and API behavior
builder.Services.AddHealthChecks();
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(opts =>
        opts.InvalidModelStateResponseFactory = ctx =>
        {
            IEnumerable<string> messages = ctx.ModelState!
                .SelectMany(kv => kv.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .Distinct();
            var combined = string.Join("; ", messages);

            var badRequest = new ApiError
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message    = combined
            };
            return new BadRequestObjectResult(badRequest);
        });

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Subster API",
        Version     = "v1",
        Description = "API for personal trainers and their clients"
    });
    options.EnableAnnotations();
});

// HTTP clients
builder.Services
    .AddHttpClient<IPaydayApiClient, PaydayApiClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Payday:BaseUrl"]
            ?? throw new Exception("Payday base URL not configured"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    });

builder.Services
    .AddHttpClient<ITaktikalApiClient, TaktikalApiClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Taktikal:BaseUrl"]
            ?? throw new Exception("Taktikal base URL not configured"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    });

// Dependency injection: repositories & services
builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProgramRepository, ProgramRepository>();

builder.Services.AddScoped<IPaydayService, PaydayService>();
builder.Services.AddScoped<ITaktikalAuthService, TaktikalAuthService>();

builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IClientDashboardService, ClientDashboardService>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ITokenService, PaydayTokenService>();
builder.Services.AddScoped<ISubscriptionBillingService, SubscriptionBillingService>();
builder.Services.AddSingleton<EncryptionHelper>();

builder.Services.AddMemoryCache();

// Database context
var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? throw new Exception("POSTGRES_HOST is not set");
var databasePort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? throw new Exception("POSTGRES_PORT is not set");
var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? throw new Exception("POSTGRES_DB is not set");
var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? throw new Exception("POSTGRES_USER is not set");
var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? throw new Exception("POSTGRES_PASSWORD is not set");

// assemble and register DbContext
var connString = $"Host={host};Port={databasePort};Database={database};Username={user};Password={password}";

builder.Services.AddDbContext<SubsterDbContext>(options =>
    options.UseNpgsql(
        connString
    )
);

// Data Protection
builder.Services
    .AddDataProtection()
    .SetApplicationName("Subster")
    .PersistKeysToDbContext<SubsterDbContext>();

// Hangfire configuration
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(
        bootstrap => bootstrap.UseNpgsqlConnection(connString),
        new PostgreSqlStorageOptions
        {
            SchemaName        = "hangfire",
            QueuePollInterval = TimeSpan.FromHours(1),
        }
    )
);
builder.Services.AddHangfireServer();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
	SubsterDbContext ctx = scope.ServiceProvider.GetRequiredService<SubsterDbContext>();
    ctx.Database.Migrate();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseCors(builder =>
    builder.WithOrigins("http://localhost:3000")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new AllowAllDashboardAuthorizationFilter()],
});

// Health check endpoint
app.MapHealthChecks("/");

// Recurring job
RecurringJob.AddOrUpdate<SubscriptionBillingJob>(
    "subscription-billing-job",
    job => job.ExecuteAsync(),
    Cron.Daily(hour: 12, minute: 0),
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    }
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllers();

app.Run();

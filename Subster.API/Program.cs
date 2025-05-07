using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Subster.DAL;
using Subster.DAL.Implementations;
using Subster.DAL.Interfaces;
using Subster.DAL.Utilities;
using Subster.API.Services;
using Subster.API.Services.Interfaces;
using Subster.API.Services.Implementations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Subster.API.Clients;
using System.Net.Http.Headers;
using Hangfire;
using Hangfire.PostgreSql;
using Subster.API.Jobs;
using Subster.API.Filters;
using Microsoft.AspNetCore.DataProtection;
using Subster.API.Middleware;
using Subster.Models;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]) ?? throw new Exception("Secret key not found");


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services
  .AddControllers()
  .ConfigureApiBehaviorOptions(opts =>
    opts.InvalidModelStateResponseFactory = ctx =>
    {
      // flatten all errors into one string (or adapt to a dict if you like)
      var messages = ctx.ModelState
          .SelectMany(kv => kv.Value.Errors)
          .Select(e => e.ErrorMessage)
          .Distinct();
      var combined = string.Join("; ", messages);

      var badRequest = new ApiError {
        StatusCode = StatusCodes.Status400BadRequest,
        Message    = combined
      };
      return new BadRequestObjectResult(badRequest);
    });

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddHttpClient<IPaydayApiClient, PaydayApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Payday:BaseUrl"]);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    });

builder.Services
    .AddHttpClient<ITaktikalApiClient, TaktikalApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Taktikal:BaseUrl"]!);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    });

// Dependency Injection
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
builder.Services.AddTransient<EncryptionHelper>();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient();

var connString = builder.Configuration.GetConnectionString("SubsterDb")!;

// SubsterDbContext
builder.Services.AddDbContext<SubsterDbContext>(options =>
    options.UseNpgsql(
        connString
    )
);

// Data Protection
builder.Services
    .AddDataProtection()
    .SetApplicationName("Subster")  // use a stable name across instances
    .PersistKeysToDbContext<SubsterDbContext>();

// Hangfire
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Subster API",
        Version = "v1",
        Description = "API for personal trainers and their clients"
    });
});

var app = builder.Build();

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

RecurringJob.AddOrUpdate<SubscriptionBillingJob>(
    "subscription-billing-job",
    job => job.ExecuteAsync(),
    Cron.Daily(hour: 12, minute: 0),
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    }
);

// Þessi kóði keyrir migrations í hvert skipti sem bakendinn er keyrður
using (var scoper = app.Services.CreateScope())
{
    var dbContext = scoper.ServiceProvider.GetRequiredService<SubsterDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();         // Generates JSON
    app.UseSwaggerUI();       // Enables the UI (at /swagger)
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllers();

app.Run();

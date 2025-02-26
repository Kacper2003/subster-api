using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Subster.DAL;
using Subster.DAL.Implementations;
using Subster.DAL.Interfaces;
using Subster.API.Services;
using Subster.API.Services.Interfaces;
using Subster.API.Services.Implementations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ITaktikalAuthService, TaktikalAuthService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddHttpClient();

// Bætir við 
builder.Services.AddDbContext<SubsterDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SubsterDb")
    )
);

var app = builder.Build();

app.MapControllers();

// Þessi kóði keyrir migrations í hvert skipti sem bakendinn er keyrður
using (var scoper = app.Services.CreateScope())
{
    var dbContext = scoper.ServiceProvider.GetRequiredService<SubsterDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();

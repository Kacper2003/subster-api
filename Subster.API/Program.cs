using Microsoft.EntityFrameworkCore;
using Subster.DAL;
using Subster.DAL.Implementations;
using Subster.DAL.Interfaces;
using Subster.API.Services.Interfaces;
using Subster.API.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Fyrir öll viðkvæm gögn
builder.Configuration.AddJsonFile("../env.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

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

app.UseHttpsRedirection();

app.Run();

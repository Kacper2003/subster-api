using Microsoft.EntityFrameworkCore;
using Subster.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

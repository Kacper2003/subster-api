using Microsoft.EntityFrameworkCore;
using Subster.DAL.Entities;

namespace Subster.DAL;

public class SubsterDbContext : DbContext
{
    public SubsterDbContext(DbContextOptions<SubsterDbContext> options)
         : base(options) { }

    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Program> Programs { get; set; }
    public DbSet<SubscriptionInvoice> SubscriptionInvoices { get; set; }
}

// dotnet ef migrations add MigrationName --project Subster.DAL --startup-project Subster.API

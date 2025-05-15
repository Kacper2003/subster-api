using Microsoft.EntityFrameworkCore;
using Subster.DAL.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace Subster.DAL;

public class SubsterDbContext(DbContextOptions<SubsterDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
	public DbSet<Trainer> Trainers { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Program> Programs { get; set; }
    public DbSet<SubscriptionInvoice> SubscriptionInvoices { get; set; }

    // Table for storing data protection keys
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    // Indexes because of frequent lookup by Ssn
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trainer>()
            .HasIndex(t => t.Ssn)
            .IsUnique()
            .HasDatabaseName("IX_Trainer_Ssn");

        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Ssn)
            .IsUnique()
            .HasDatabaseName("IX_Client_Ssn");
    }
}

// dotnet ef migrations add MigrationName --project Subster.DAL --startup-project Subster.API

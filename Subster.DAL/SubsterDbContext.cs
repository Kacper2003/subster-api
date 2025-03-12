using Microsoft.EntityFrameworkCore;
using Subster.DAL.Entities;

namespace Subster.DAL;

public class SubsterDbContext : DbContext
{
    public SubsterDbContext(DbContextOptions<SubsterDbContext> options)
         : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
}
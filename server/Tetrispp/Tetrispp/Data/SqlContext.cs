using Microsoft.EntityFrameworkCore;
using Tetrispp.Models.Db;

namespace Tetrispp.Data;

public class SqlContext : DbContext
{
    public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
    }
}

//Add-Migration -Name InitialMigration -Context SqlContext -OutputDir Data/Migrations
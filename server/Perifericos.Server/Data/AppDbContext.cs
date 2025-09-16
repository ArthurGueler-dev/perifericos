using Microsoft.EntityFrameworkCore;
using Perifericos.Server.Models;

namespace Perifericos.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Computer> Computers => Set<Computer>();
    public DbSet<Peripheral> Peripherals => Set<Peripheral>();
    public DbSet<PeripheralAssignment> PeripheralAssignments => Set<PeripheralAssignment>();
    public DbSet<DeviceEvent> DeviceEvents => Set<DeviceEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Peripheral>().HasIndex(p => new { p.VendorId, p.ProductId, p.SerialNumber });
        modelBuilder.Entity<PeripheralAssignment>().HasIndex(pa => new { pa.PeripheralId, pa.ComputerId }).IsUnique();
    }
}



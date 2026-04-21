using FloraAI.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FloraAI.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ConditionsDictionary> ConditionsDictionary { get; set; } = null!;
    public DbSet<UserPlant> UserPlants { get; set; } = null!;
    public DbSet<ScanHistory> ScanHistories { get; set; } = null!;
    public DbSet<PlantLookup> PlantLookups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ConditionsDictionary Configuration
        modelBuilder.Entity<ConditionsDictionary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlantType).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ConditionName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Treatment).HasMaxLength(4000);
            entity.Property(e => e.CareInstructions).HasMaxLength(4000);
            entity.HasIndex(e => new { e.PlantType, e.ConditionName }).IsUnique();
        });

        // UserPlant Configuration
        modelBuilder.Entity<UserPlant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nickname).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PlantType).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CurrentStatus).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SavedTreatment).HasMaxLength(4000);
            entity.Property(e => e.SavedCareInstructions).HasMaxLength(4000);

            // Foreign Key
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPlants)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ScanHistory Configuration
        modelBuilder.Entity<ScanHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConditionFound).IsRequired().HasMaxLength(500);

            // Foreign Key to UserPlant
            entity.HasOne(e => e.UserPlant)
                .WithMany(up => up.ScanHistories)
                .HasForeignKey(e => e.UserPlantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign Key to ConditionsDictionary
            entity.HasOne(e => e.ConditionsDictionary)
                .WithMany(cd => cd.ScanHistories)
                .HasForeignKey(e => e.ConditionsDictionaryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PlantLookup Configuration
        modelBuilder.Entity<PlantLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommonName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DefaultImage).HasMaxLength(1000);
            entity.HasIndex(e => e.CommonName).IsUnique();
        });
    }
}

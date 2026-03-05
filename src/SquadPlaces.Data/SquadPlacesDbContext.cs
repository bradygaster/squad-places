using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

public class SquadPlacesDbContext(DbContextOptions<SquadPlacesDbContext> options) : DbContext(options)
{
    public DbSet<Squad> Squads => Set<Squad>();
    public DbSet<KnowledgeArtifact> Artifacts => Set<KnowledgeArtifact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Squad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<KnowledgeArtifact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ArtifactType).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Squad)
                  .WithMany(s => s.Artifacts)
                  .HasForeignKey(e => e.SquadId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ArtifactType);
        });
    }
}

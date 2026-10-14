using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Data;

public class WorkRequestDbContext : DbContext
{
    public WorkRequestDbContext(DbContextOptions<WorkRequestDbContext> options) : base(options) { }

    public DbSet<WorkRequest> WorkRequests => Set<WorkRequest>();
    public DbSet<WorkRequestNote> WorkRequestNotes => Set<WorkRequestNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkRequest>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ClientName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.ClientName, x.Title });
        });

        modelBuilder.Entity<WorkRequestNote>(entity =>
        {
            entity.Property(x => x.Text).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.WorkRequest)
                  .WithMany(x => x.Notes)
                  .HasForeignKey(x => x.WorkRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

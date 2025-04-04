using Microsoft.EntityFrameworkCore;
using ParticipantManager.API.Models;

namespace ParticipantManager.API.Data;

public class ParticipantManagerDbContext(DbContextOptions<ParticipantManagerDbContext> options) : DbContext(options)
{
    public DbSet<Participant> Participants { get; set; }
    public DbSet<PathwayTypeEnrolment> PathwayTypeEnrolments { get; set; }
    public DbSet<Episode> Episodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships, keys, etc.
        modelBuilder.Entity<Participant>().HasKey(p => p.ParticipantId);
        modelBuilder.Entity<PathwayTypeEnrolment>().HasKey(pa => pa.EnrolmentId);
        modelBuilder.Entity<Episode>().HasKey(e => e.EpisodeId);
    }
}

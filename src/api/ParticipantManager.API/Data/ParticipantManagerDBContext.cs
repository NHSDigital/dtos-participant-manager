using Microsoft.EntityFrameworkCore;
using ParticipantManager.API.Models;

namespace ParticipantManager.API.Data;

public class ParticipantManagerDbContext : DbContext
{
  public ParticipantManagerDbContext(DbContextOptions<ParticipantManagerDbContext> options)
    : base(options)
  {
  }

  public DbSet<Participant> Participants { get; set; }
  public DbSet<PathwayTypeEnrolment> PathwayTypeEnrolments { get; set; }
  public DbSet<Episode> Episodes { get; set; }
  public DbSet<Encounter> Encounters { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Configure relationships, keys, etc.
    modelBuilder.Entity<Participant>().HasKey(p => p.ParticipantId);
    modelBuilder.Entity<PathwayTypeEnrolment>().HasKey(pa => pa.EnrolmentId);
    modelBuilder.Entity<Episode>().HasKey(e => e.EpisodeId);
    modelBuilder.Entity<Encounter>().HasKey(en => en.EncounterId);
  }
}

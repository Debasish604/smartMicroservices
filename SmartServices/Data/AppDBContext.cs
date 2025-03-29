using Microsoft.EntityFrameworkCore;
using SmartServices.models.Entity;

namespace SmartServices.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<CandidateShortlisted> CandidatesShortlisted { get; set; } // Mapping the view

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly map the table name to "Job" (case-sensitive in PostgreSQL)
            modelBuilder.Entity<Job>().ToTable("Job", schema: "ADANI_TALENT");

            // Map the view to the CandidateShortlisted entity
            modelBuilder.Entity<CandidateShortlisted>()
            .ToView("V_HM_CANDIDATE_SHORTLISTED", schema: "ADANI_TALENT")
            .HasNoKey(); ; // Indicate that this is a view with no primary key
        }
    }
}

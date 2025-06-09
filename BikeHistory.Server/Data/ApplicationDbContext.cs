using BikeHistory.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BikeHistory.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BikeFrame> BikeFrames { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<BikeType> BikeTypes { get; set; }
        public DbSet<OwnershipRecord> OwnershipRecords { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<MaintenanceDetail> MaintenanceDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<BikeFrame>()
                .HasOne(b => b.CurrentOwner)
                .WithMany(u => u.OwnedBikes)
                .HasForeignKey(b => b.CurrentOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OwnershipRecord>()
                .HasOne(o => o.PreviousOwner)
                .WithMany()
                .HasForeignKey(o => o.PreviousOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OwnershipRecord>()
                .HasOne(o => o.NewOwner)
                .WithMany(u => u.OwnershipHistory)
                .HasForeignKey(o => o.NewOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OwnershipRecord>()
                .HasOne(o => o.BikeFrame)
                .WithMany(b => b.OwnershipHistory)
                .HasForeignKey(o => o.BikeFrameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Maintenance>()
                .HasMany(m => m.MaintenanceDetails)
                .WithOne(md => md.Maintenance)
                .HasForeignKey(md => md.MaintenanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            builder.Entity<BikeFrame>()
                .HasIndex(b => b.FrameNumber)
                .IsUnique();

            // Configure Complex Key for MaintenanceDetail
            builder.Entity<MaintenanceDetail>()
                .HasKey(md => new { md.MaintenanceId, md.Seq });

        }
    }
}
using BikeHistory.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BikeHistory.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base(new DbContextOptions<ApplicationDbContext>())
        {
        }

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
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<BikeImage> BikeImages { get; set; }
        public DbSet<UserImage> UserImages { get; set; }

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

            // Configure BikeImage relationships
            builder.Entity<BikeImage>()
                .HasOne(bi => bi.BikeFrame)
                .WithMany(b => b.Images)
                .HasForeignKey(bi => bi.BikeFrameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BikeImage>()
                .HasOne(bi => bi.Uploader)
                .WithMany()
                .HasForeignKey(bi => bi.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserImage relationships
            builder.Entity<UserImage>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.Images)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            builder.Entity<BikeFrame>()
                .HasIndex(b => b.FrameNumber)
                .IsUnique();

            // Configure BikeImage indexes
            builder.Entity<BikeImage>()
                .HasIndex(bi => bi.BikeFrameId);

            builder.Entity<BikeImage>()
                .HasIndex(bi => new { bi.BikeFrameId, bi.IsPrimary });

            // Configure UserImage indexes
            builder.Entity<UserImage>()
                .HasIndex(ui => ui.UserId);

            builder.Entity<UserImage>()
                .HasIndex(ui => new { ui.UserId, ui.IsProfileImage });

            // Configure Complex Key for MaintenanceDetail
            builder.Entity<MaintenanceDetail>()
                .HasKey(md => new { md.MaintenanceId, md.Seq });

            // MaintenanceDetail decimal 속성에 대한 정밀도 설정
            builder.Entity<MaintenanceDetail>()
                .Property(md => md.LaborCost)
                .HasPrecision(18, 2); // 총 18자리, 소수점 이하 2자리

            builder.Entity<MaintenanceDetail>()
                .Property(md => md.PartPrice)
                .HasPrecision(18, 2); // 총 18자리, 소수점 이하 2자리

            // Maintenance 외래 키 설정 - 순환 참조 방지
            builder.Entity<Maintenance>()
                .HasOne(m => m.Store)
                .WithMany()
                .HasForeignKey(m => m.StoreId)
                .OnDelete(DeleteBehavior.Restrict); // CASCADE 대신 RESTRICT 사용

            builder.Entity<Maintenance>()
                .HasOne(m => m.Owner)
                .WithMany()
                .HasForeignKey(m => m.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // CASCADE 대신 RESTRICT 사용

            // BikeFrame은 CASCADE 삭제 허용
            builder.Entity<Maintenance>()
                .HasOne(m => m.BikeFrame)
                .WithMany()
                .HasForeignKey(m => m.BikeFrameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Maintenance>()
                .HasMany(m => m.MaintenanceDetails)
                .WithOne(md => md.Maintenance)
                .HasForeignKey(md => md.MaintenanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserActivityLog 설정
            builder.Entity<UserActivityLog>()
                .HasKey(u => u.Id);

            builder.Entity<UserActivityLog>()
                .Property(u => u.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}

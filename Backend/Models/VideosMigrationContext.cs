using System;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public partial class VideosMigrationContext : DbContext
    {
        public virtual DbSet<VideoIdMigration> VideoIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_unicode_ci");

            modelBuilder.Entity<VideoIdMigration>(entity =>
            {
                entity.ToTable("video_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AddDate)
                    .HasColumnType("timestamp")
                    .HasColumnName("addDate");

                entity.Property(e => e.Userid)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("userid");

                entity.Property(e => e.Videoname)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("videoname");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public VideosMigrationContext(DbContextOptions<VideosContext> options) : base(options)
        {
        }
    }
    public partial class VideoIdMigration
    {
        public string Id { get; set; }
        public ulong Userid { get; set; }
        public DateTime? AddDate { get; set; }
        public string Videoname { get; set; }
    }
}

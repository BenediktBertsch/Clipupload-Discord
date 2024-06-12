using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public partial class VideosMigrationContext : DbContext
    {
        public virtual DbSet<VideoIdMigration> VideoIds { get; set; }
        public virtual DbSet<UserMigration> User {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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

            modelBuilder.Entity<UserMigration>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Userid).HasColumnName("id");

                entity.Property(e => e.Post)
                    .HasColumnType("boolean")
                    .HasColumnName("post");
            });
        }

        public VideosMigrationContext(DbContextOptions<VideosMigrationContext> options) : base(options)
        {
            //this.Database.EnsureCreated();
        }
    }
    public partial class VideoIdMigration
    {
        [Key]
        public string Id { get; set; }
        public ulong Userid { get; set; }
        public DateTime? AddDate { get; set; }
        public string Videoname { get; set; }
        public string Videoid { get; set; }
        public string ThumbnailLink { get; set; }
    }

    public partial class UserMigration
    {
        [Key]
        public ulong Userid { get; set; }
        public bool Post { get; set; }
    }
}

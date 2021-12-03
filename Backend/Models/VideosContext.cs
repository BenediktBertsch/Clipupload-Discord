using System;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class VideosContext : DbContext
    {
        public virtual DbSet<VideoId> VideoIds { get; set; }
        public virtual DbSet<User> User { get; set; }
        public VideosContext(DbContextOptions<VideosContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_unicode_ci");

            modelBuilder.Entity<VideoId>(entity =>
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

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Userid).HasColumnName("id");

                entity.Property(e => e.Post)
                    .HasColumnType("boolean")
                    .HasColumnName("post");
            });
        }
    }
    public partial class VideoId
    {
        public string Id { get; set; }
        public ulong Userid { get; set; }
        public DateTime? AddDate { get; set; }
        public string Videoname { get; set; }
        public string Videoid { get; set; }
        public string ThumbnailLink { get; set; }
    }

    public partial class User
    {
        public ulong Userid { get; set; }
        public bool Post { get; set; }
    }
}
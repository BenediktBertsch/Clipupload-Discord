using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class VideosContext : DbContext
    {
        public virtual DbSet<Video> VideoIds { get; set; }
        public virtual DbSet<User> User { get; set; }
        public VideosContext(DbContextOptions<VideosContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_unicode_ci");

            modelBuilder.Entity<Video>(entity =>
            {
                entity.ToTable("Videos");

                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.Date)
                    .HasColumnType("timestamp")
                    .HasColumnName("Date");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("User");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("Name");

                entity.Property(e => e.Hash)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("Hash");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.Post)
                    .HasColumnType("boolean")
                    .HasColumnName("Post");
            });
        }
    }
    public partial class Video
    {
        [Key]
        public required string Id { get; set; }
        public ulong User { get; set; }
        public required DateTime Date { get; set; }
        public required string Name { get; set; }
        public required string Hash { get; set; }
    }

    public partial class User
    {
        [Key]
        public ulong Id { get; set; }
        public bool Post { get; set; }
    }
}
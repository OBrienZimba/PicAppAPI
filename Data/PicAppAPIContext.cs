using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PicAppAPI.Models;

namespace PicAppAPI.Data
{
    public class PicAppAPIContext : DbContext
    {
        public PicAppAPIContext (DbContextOptions<PicAppAPIContext> options)
            : base(options)
        {
        }

        public DbSet<PicAppAPI.Models.ImageData> ImageData { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ImageData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageBytes).HasColumnType("bytea");
                entity.Property(e => e.UploadDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.FileName).HasMaxLength(255); 
            });
        }
    }
}

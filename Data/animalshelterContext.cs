using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using vant110.AnimalShelter.Data.Models;

namespace vant110.AnimalShelter.Data
{
    public partial class animalshelterContext : DbContext
    {
        public animalshelterContext()
        {
        }

        public animalshelterContext(DbContextOptions<animalshelterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Animal> Animals { get; set; } = null!;
        public virtual DbSet<Article> Articles { get; set; } = null!;
        public virtual DbSet<Species> Species { get; set; } = null!;
        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("user id=user;password=user;server=localhost\\SQLEXPRESS;database=animal-shelter");
            }
        }
        */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>(entity =>
            {
                entity.ToTable("animals");

                entity.Property(e => e.AnimalId).HasColumnName("animal_id");

                entity.Property(e => e.ArrivalDate)
                    .HasColumnType("date")
                    .HasColumnName("arrival_date");

                entity.Property(e => e.BirthYear).HasColumnName("birth_year");

                entity.Property(e => e.ChipStatus).HasColumnName("chip_status");

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .HasColumnName("description");

                entity.Property(e => e.ImageName)
                    .HasMaxLength(30)
                    .HasColumnName("image_name");

                entity.Property(e => e.Name)
                    .HasMaxLength(18)
                    .HasColumnName("name");

                entity.Property(e => e.SpeciesId).HasColumnName("species_id");

                entity.Property(e => e.SterilizationStatus).HasColumnName("sterilization_status");

                entity.Property(e => e.VaccinationStatus).HasColumnName("vaccination_status");

                entity.HasOne(d => d.Species)
                    .WithMany(p => p.Animals)
                    .HasForeignKey(d => d.SpeciesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__animals__species__33D4B598");
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("articles");

                entity.Property(e => e.ArticleId).HasColumnName("article_id");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .HasColumnName("description");

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<Species>(entity =>
            {
                entity.ToTable("species");

                entity.Property(e => e.SpeciesId).HasColumnName("species_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .HasColumnName("name");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

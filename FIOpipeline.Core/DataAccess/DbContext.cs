
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FIOpipeline.Core.Entity;

namespace FIOpipeline.Core.DataAccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Email> Emails { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.SecondName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.BirthdayDate).IsRequired();
                entity.Property(p => p.Sex).IsRequired().HasMaxLength(1);

                entity.HasMany(p => p.Addresses)
                      .WithOne(a => a.Person)
                      .HasForeignKey(a => a.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Phones)
                      .WithOne(p => p.Person)
                      .HasForeignKey(p => p.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Emails)
                      .WithOne(e => e.Person)
                      .HasForeignKey(e => e.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Value).IsRequired().HasMaxLength(500);

                entity.HasIndex(a => a.PersonId);
            });

            modelBuilder.Entity<Phone>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Value).IsRequired().HasMaxLength(20);

                entity.HasIndex(p => p.PersonId);
            });

            modelBuilder.Entity<Email>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(255);

                entity.HasIndex(e => e.PersonId);
            });
        }
    }

}

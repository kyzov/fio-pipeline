
using FIOpipeline.Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.HasAlternateKey(p => new { p.Id, p.ValidFrom });

                entity.HasIndex(p => p.ValidFrom);
                entity.HasIndex(p => p.ValidTo);
                entity.HasIndex(p => p.IsCurrent);

                entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.SecondName).IsRequired().HasMaxLength(100);

                entity.Property(p => p.BirthdayDate)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone");

                entity.Property(p => p.Sex).IsRequired().HasMaxLength(1);

                entity.Property(p => p.ValidFrom)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(p => p.ValidTo)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("'9999-12-31 23:59:59'::timestamp");

                entity.Property(p => p.IsCurrent)
                      .IsRequired()
                      .HasDefaultValue(true);

                entity.Property(p => p.Version)
                      .IsRequired()
                      .HasDefaultValue(1);

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
                entity.Property(a => a.Id).ValueGeneratedOnAdd();
                entity.HasAlternateKey(a => new { a.Id, a.ValidFrom });
                entity.Property(a => a.Value).IsRequired().HasMaxLength(500);

                entity.Property(a => a.ValidFrom)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(a => a.ValidTo)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("'9999-12-31 23:59:59'::timestamp");

                entity.Property(a => a.IsCurrent).IsRequired().HasDefaultValue(true);
                entity.Property(a => a.Version).IsRequired().HasDefaultValue(1);

                entity.HasIndex(a => a.PersonId);
                entity.HasIndex(a => a.ValidFrom);
                entity.HasIndex(a => a.ValidTo);

                entity.HasOne(a => a.Person)
                      .WithMany(p => p.Addresses)
                      .HasForeignKey(a => a.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Phone>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.HasAlternateKey(p => new { p.Id, p.ValidFrom });
                entity.Property(p => p.Value).IsRequired().HasMaxLength(20);

                entity.Property(p => p.ValidFrom)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(p => p.ValidTo)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("'9999-12-31 23:59:59'::timestamp");

                entity.Property(p => p.IsCurrent).IsRequired().HasDefaultValue(true);
                entity.Property(p => p.Version).IsRequired().HasDefaultValue(1);

                entity.HasIndex(p => p.PersonId);
                entity.HasIndex(p => p.ValidFrom);
                entity.HasIndex(p => p.ValidTo);

                entity.HasOne(p => p.Person)
                      .WithMany(p => p.Phones)
                      .HasForeignKey(p => p.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Email>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasAlternateKey(e => new { e.Id, e.ValidFrom });
                entity.Property(e => e.Value).IsRequired().HasMaxLength(255);

                entity.Property(e => e.ValidFrom)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ValidTo)
                      .IsRequired()
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("'9999-12-31 23:59:59'::timestamp");

                entity.Property(e => e.IsCurrent).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.Version).IsRequired().HasDefaultValue(1);

                entity.HasIndex(e => e.PersonId);
                entity.HasIndex(e => e.ValidFrom);
                entity.HasIndex(e => e.ValidTo);

                entity.HasOne(e => e.Person)
                      .WithMany(p => p.Emails)
                      .HasForeignKey(e => e.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is BaseTemporalEntity);

            foreach (var entry in entries)
            {
                var entity = (BaseTemporalEntity)entry.Entity;

                if (entity.ValidFrom == DateTime.MinValue)
                {
                    entity.ValidFrom = DateTime.Now;
                }

                if (entity.ValidTo == DateTime.MinValue)
                {
                    entity.ValidTo = new DateTime(9999, 12, 31, 23, 59, 59);
                }

                entity.IsCurrent = true;
                entity.Version = 1;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task InitializeTemporalData()
        {
            if (!Persons.Any(p => p.ValidFrom != DateTime.MinValue))
            {
                var persons = await Persons.ToListAsync();
                foreach (var person in persons)
                {
                    person.ValidFrom = DateTime.Now.AddYears(-1);
                    person.ValidTo = new DateTime(9999, 12, 31, 23, 59, 59);
                    person.IsCurrent = true;
                    person.Version = 1;
                }
                await SaveChangesAsync();
            }
        }
    }

}

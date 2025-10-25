
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
            modelBuilder.Entity<Person>()
                .HasOne(p => p.Address)
                .WithOne(a => a.Person)
                .HasForeignKey<Address>(a => a.PersonId)
                .IsRequired();

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Phone)
                .WithOne(ph => ph.Person)
                .HasForeignKey<Phone>(ph => ph.PersonId)
                .IsRequired();

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Email)
                .WithOne(e => e.Person)
                .HasForeignKey<Email>(e => e.PersonId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }

}

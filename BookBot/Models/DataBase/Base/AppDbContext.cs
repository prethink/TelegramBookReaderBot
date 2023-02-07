using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace BookBot.Models.DataBase.Base
{
    public class SampleContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            return new AppDbContext();
        }
    }

    public class MysqlEntityFrameworkDesignTimeServices : IDesignTimeServices
    {

        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkMySQL();
            new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
                .TryAddCoreServices();
        }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<UserBot> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<LinkStatistic> Links { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }


        [DbFunction]
        public static long GetMyPositionRatingById(long telegramId) => throw new NotImplementedException("EFCore function MySQL");

        [DbFunction]
        public static long GetMyPositionAcitvityById(long telegramId) => throw new NotImplementedException("EFCore function MySQL");

        [DbFunction]
        public static long GetMyPositionLikesById(long telegramId) => throw new NotImplementedException("EFCore function MySQL");

        [DbFunction]
        public static long GetMyPositionViewedById(long telegramId) => throw new NotImplementedException("EFCore function MySQL");

        [DbFunction]
        public static int GetAgeFromBirthday(DateTime? birthDay) => throw new NotImplementedException("EFCore function MySQL");

        public AppDbContext()
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var database = ConfigApp.GetSettingsDB<DatabaseConfig>();

            string serverData = $"server={database.Host};" +
                                $"port={database.Port};" +
                                $"user={database.Login};" +
                                $"password={database.Password};" +
                                $"database={database.Database};";

            optionsBuilder.UseMySQL(serverData);


            optionsBuilder.LogTo(message => Debug.WriteLine(message));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(() => GetMyPositionRatingById(default));
            modelBuilder.HasDbFunction(() => GetMyPositionAcitvityById(default));
            modelBuilder.HasDbFunction(() => GetMyPositionLikesById(default));
            modelBuilder.HasDbFunction(() => GetMyPositionViewedById(default));
            modelBuilder.HasDbFunction(() => GetAgeFromBirthday(default));

            modelBuilder.Entity<UserBot>()
                .HasKey(u => u.TelegramId);

            modelBuilder.Entity<UserBot>()
            .HasOne(x => x.ParentUser)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<UserBot>()
            .HasOne(x => x.CurrentBook)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Book>()
                    .HasMany(c => c.Users)
                    .WithMany(s => s.Books)
                    .UsingEntity(j => j.ToTable("users_books"));
        }
    }


}

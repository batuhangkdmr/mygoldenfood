﻿using Microsoft.EntityFrameworkCore;
using MyGoldenFood.Models;

namespace MyGoldenFood.ApplicationDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Lazy Loading Proxy ayarı
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies(); // Lazy loading özelliğini etkinleştir
            }
        }

        // Model yapılandırmaları
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Varsayılan String uzunluğu
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string));

                foreach (var property in properties)
                {
                    property.SetMaxLength(255); // Varsayılan String uzunluğu
                }
            }

            // 📌 RecipeCategoryTranslation ile RecipeCategory ilişkisi
            modelBuilder.Entity<RecipeCategoryTranslation>()
                .ToTable("RecipeCategoryTranslations") // Tek seferde ayarla
                .HasOne(rct => rct.RecipeCategory)
                .WithMany(rc => rc.Translations) // `RecipeCategory` içinde `Translations` koleksiyonu olmalı!
                .HasForeignKey(rct => rct.RecipeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // 📌 Tabloları Ayarla
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Recipe>().ToTable("Recipes"); // Tarif tablosu
            modelBuilder.Entity<Benefit>().ToTable("Benefits"); // Fayda tablosu
            modelBuilder.Entity<ProductTranslation>().ToTable("ProductTranslations");
            modelBuilder.Entity<RecipeTranslation>().ToTable("RecipeTranslations");
            modelBuilder.Entity<BenefitTranslation>().ToTable("BenefitTranslations");
            modelBuilder.Entity<RecipeCategory>().ToTable("RecipeCategories"); // Tarif kategorileri tablosu

            base.OnModelCreating(modelBuilder);
        }

        // 📌 Veritabanı Tabloları
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RecipeCategory> RecipeCategories { get; set; } // Tarif kategorileri
        public DbSet<RecipeCategoryTranslation> RecipeCategoryTranslations { get; set; } // Tarif kategori çevirileri
        public DbSet<Recipe> Recipes { get; set; } // Tarifler
        public DbSet<Benefit> Benefits { get; set; } // Faydalar
        public DbSet<ProductTranslation> ProductTranslations { get; set; }
        public DbSet<RecipeTranslation> RecipeTranslations { get; set; }
        public DbSet<BenefitTranslation> BenefitTranslations { get; set; }

        // `Translation` kullanılmıyorsa bunu kaldırabilirsin
        public DbSet<Translation> Translations { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(p => p.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => p.Sku).IsUnique();

            builder.Property(p => p.Price)
                .HasColumnName("price")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            builder.Property(p => p.Stock)
                .HasColumnName("stock")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(p => p.Category)
                .HasColumnName("category")
                .HasMaxLength(50)
                .IsRequired(false);
        }
    }
}
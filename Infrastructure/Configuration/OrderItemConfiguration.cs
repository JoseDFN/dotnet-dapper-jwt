using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");

            builder.HasKey(o_i => o_i.Id);

            builder.Property(o_i => o_i.Id)
                .HasColumnName("id");

            builder.Property(o_i => o_i.OrderId)
                .HasColumnName("order_id");

            builder.Property(o_i => o_i.ProductId)
                .HasColumnName("product_id");

            builder.Property(o_i => o_i.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(o_i => o_i.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            builder.HasOne(o_i => o_i.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o_i => o_i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o_i => o_i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(o_i => o_i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
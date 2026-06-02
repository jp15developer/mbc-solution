using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CustomerId)
            .IsRequired();

        builder.Property(s => s.CustomerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.BranchId)
            .IsRequired();

        builder.Property(s => s.BranchName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.SaleDate)
            .IsRequired();

        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.IsCancelled)
            .IsRequired();

        builder.OwnsMany(s => s.Items, itemBuilder =>
        {
            itemBuilder.ToTable("SaleItems");

            itemBuilder.WithOwner().HasForeignKey("SaleId");
            itemBuilder.Property<Guid>("Id");
            itemBuilder.HasKey("Id");

            itemBuilder.Property(i => i.ProductId)
                .IsRequired();

            itemBuilder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(100);

            itemBuilder.Property(i => i.Quantity)
                .IsRequired();

            itemBuilder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            itemBuilder.Property(i => i.DiscountPercentage)
                .HasPrecision(5, 2)
                .IsRequired();

            itemBuilder.Property(i => i.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            itemBuilder.Property(i => i.IsCancelled)
                .IsRequired();
        });

        builder.Navigation(s => s.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}

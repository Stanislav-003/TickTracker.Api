using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TickTracker.Api.Entities;

namespace TickTracker.Api.DatabaseConfigurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Symbol).IsRequired().HasMaxLength(40);
        builder.Property(a => a.Kind).IsRequired().HasMaxLength(40);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(255);
        builder.Property(a => a.TickSize).IsRequired();
        builder.Property(a => a.Currency).IsRequired().HasMaxLength(15);
        builder.Property(a => a.BaseCurrency).IsRequired().HasMaxLength(15);
        builder.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
    }
}
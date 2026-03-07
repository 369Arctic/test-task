using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using task.Models;

namespace task.Data.Configurations;

public class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.Code);
        builder.HasIndex(o => o.CityCode);

        builder.Property(o => o.Code).HasMaxLength(50);

        builder.Property(o => o.Uuid).HasMaxLength(50);

        builder.Property(o => o.CountryCode).HasMaxLength(10).IsRequired();

        builder.Property(o => o.AddressRegion).HasMaxLength(100);

        builder.Property(o => o.AddressCity).HasMaxLength(100);

        builder.Property(o => o.AddressStreet).HasMaxLength(200);

        builder.Property(o => o.AddressHouseNumber).HasMaxLength(50);

        builder.Property(o => o.WorkTime).HasMaxLength(200);

        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(o => o.Coordinates, coord =>
        {
            coord.Property(c => c.Latitude).HasColumnName("latitude");
            coord.Property(c => c.Longitude).HasColumnName("longitude");
        });

        builder.HasMany(o => o.Phones)
               .WithOne(p => p.Office)
               .HasForeignKey(p => p.OfficeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
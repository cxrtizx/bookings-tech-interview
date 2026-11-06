using Bookings.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configurations
{
    public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasConversion(id => id.Value, value => BookingId.From(value))
                .ValueGeneratedNever();

            builder.ComplexProperty(b => b.Customer,
                customer =>
                {
                    customer.Property(c => c.Name)
                        .HasColumnName("CustomerName")
                        .HasMaxLength(Customer.NameMaxLength);

                    customer.Property(c => c.Email)
                        .HasColumnName("CustomerEmail")
                        .HasMaxLength(Email.MaxLength)
                        .HasConversion(email => email.Value, value => Email.Create(value));
                });

            builder.ComplexProperty(b => b.DateRange,
                dateRange =>
                {
                    dateRange.Property(d => d.Start)
                        .HasColumnName("StartUtc");

                    dateRange.Property(d => d.End)
                        .HasColumnName("EndUtc");
                });

            builder.Property(b => b.ResourceId)
                .HasMaxLength(ResourceId.MaxLength)
                .HasConversion(resourceId => resourceId.Value, value => ResourceId.Create(value));

            builder.Property(b => b.Status)
                .HasConversion<string>();
        }
    }
}

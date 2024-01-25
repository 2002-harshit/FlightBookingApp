using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingApp.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bookingdetail> Bookingdetails { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=mysecretpassword");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bookingdetail>(entity =>
        {
            entity.HasKey(e => e.Bookingid).HasName("bookingdetails_pkey");

            entity.ToTable("bookingdetails");

            entity.Property(e => e.Bookingid).HasColumnName("bookingid");
            entity.Property(e => e.Bookingdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("bookingdate");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.Flightid).HasColumnName("flightid");
            entity.Property(e => e.Noofpassengers).HasColumnName("noofpassengers");
            entity.Property(e => e.Seatnumbers)
                .HasMaxLength(255)
                .HasColumnName("seatnumbers");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Totalcost)
                .HasPrecision(10, 2)
                .HasColumnName("totalcost");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookingdetails)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookingdetails_customerid_fkey");

            // entity.HasOne(d => d.Flight).WithMany(p => p.Bookingdetails)
            //     .HasForeignKey(d => d.Flightid)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("bookingdetails_flightid_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Customerid).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.HasIndex(e => e.Email, "customers_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "customers_phone_key").IsUnique();

            entity.HasIndex(e => e.Email, "unique_email").IsUnique();

            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.Flightid).HasName("flights_pkey");

            entity.ToTable("flights");

            entity.Property(e => e.Flightid).HasColumnName("flightid");
            entity.Property(e => e.Arrivaltime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("arrivaltime");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Currentlybookedseats)
                .HasDefaultValue(0)
                .HasColumnName("currentlybookedseats");
            entity.Property(e => e.Departuretime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("departuretime");
            entity.Property(e => e.Destination)
                .HasMaxLength(255)
                .HasColumnName("destination");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Rate)
                .HasPrecision(10, 2)
                .HasColumnName("rate");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("source");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

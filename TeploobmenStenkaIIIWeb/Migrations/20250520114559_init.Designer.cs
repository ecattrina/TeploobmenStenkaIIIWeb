﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeploobmenStenkaIIIWeb.Services;

#nullable disable

namespace TeploobmenStenkaIIIWeb.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250520114559_init")]
    partial class init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("TeploobmenStenkaIIIWeb.Models.Entities.BioCoeff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Bio")
                        .HasColumnType("REAL");

                    b.Property<double>("M")
                        .HasColumnType("REAL");

                    b.Property<double>("Mu")
                        .HasColumnType("REAL");

                    b.Property<double>("MuSquared")
                        .HasColumnType("REAL");

                    b.Property<double>("Np")
                        .HasColumnType("REAL");

                    b.Property<double>("Pp")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("Bio")
                        .IsUnique();

                    b.ToTable("BioCoeffs");
                });
#pragma warning restore 612, 618
        }
    }
}

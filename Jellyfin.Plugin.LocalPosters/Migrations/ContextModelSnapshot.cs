﻿// <auto-generated />
using System;
using Jellyfin.Plugin.LocalPosters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Jellyfin.Plugin.LocalPosters.Migrations
{
    [DbContext(typeof(Context))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("Jellyfin.Plugin.LocalPosters.Entities.PosterRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImageType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("Primary");

                    b.Property<string>("ItemKind")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("MatchedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("_posterPath")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("PosterPath");

                    b.HasKey("Id");

                    b.ToTable("PosterRecord");
                });
#pragma warning restore 612, 618
        }
    }
}

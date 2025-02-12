﻿// <auto-generated />
using System;
using Jellyfin.Plugin.LocalPosters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Jellyfin.Plugin.LocalPosters.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20250212152920_MatchedAt")]
    partial class MatchedAt
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<DateTimeOffset>("MatchedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("_posterPath")
                        .HasColumnType("TEXT")
                        .HasColumnName("PosterPath");

                    b.HasKey("Id");

                    b.ToTable("PosterRecord");
                });
#pragma warning restore 612, 618
        }
    }
}

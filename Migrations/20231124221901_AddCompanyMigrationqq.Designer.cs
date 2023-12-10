﻿// <auto-generated />
using DataProcForWebApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataBaseForMovie.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20231124221901_AddCompanyMigrationqq")]
    partial class AddCompanyMigrationqq
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("DataProcForWebApp.Human", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Humans");
                });

            modelBuilder.Entity("DataProcForWebApp.Movie", b =>
                {
                    b.Property<string>("tittle")
                        .HasColumnType("TEXT");

                    b.Property<string>("director")
                        .HasColumnType("TEXT");

                    b.Property<string>("movieRating")
                        .HasColumnType("TEXT");

                    b.HasKey("tittle");

                    b.ToTable("Movies");
                });

            modelBuilder.Entity("HumanMovie", b =>
                {
                    b.Property<int>("actorsSetConnectionid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("currentMoviesConnectiontittle")
                        .HasColumnType("TEXT");

                    b.HasKey("actorsSetConnectionid", "currentMoviesConnectiontittle");

                    b.HasIndex("currentMoviesConnectiontittle");

                    b.ToTable("HumanMovie");
                });

            modelBuilder.Entity("HumanMovie", b =>
                {
                    b.HasOne("DataProcForWebApp.Human", null)
                        .WithMany()
                        .HasForeignKey("actorsSetConnectionid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataProcForWebApp.Movie", null)
                        .WithMany()
                        .HasForeignKey("currentMoviesConnectiontittle")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
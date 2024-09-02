﻿// <auto-generated />
using System;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    [DbContext(typeof(ClassInsightsContext))]
    [Migration("20240831201226_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Api.Models.TabClass", b =>
                {
                    b.Property<int>("ClassId")
                        .HasColumnType("integer")
                        .HasColumnName("ClassID");

                    b.Property<string>("AzureGroupId")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("AzureGroupID");

                    b.Property<string>("Head")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("ClassId")
                        .HasName("PK_tabClasses_1");

                    b.ToTable("tabClass", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabComputer", b =>
                {
                    b.Property<int>("ComputerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("ComputerID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ComputerId"));

                    b.Property<string>("IpAddress")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("LastSeen")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastUser")
                        .HasMaxLength(75)
                        .HasColumnType("character varying(75)");

                    b.Property<string>("MacAddress")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("RoomId")
                        .HasColumnType("integer")
                        .HasColumnName("RoomID");

                    b.Property<string>("Version")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("ComputerId")
                        .HasName("PK_tabComputers");

                    b.HasIndex("RoomId");

                    b.ToTable("tabComputer", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabLesson", b =>
                {
                    b.Property<int>("LessonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("LessonID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LessonId"));

                    b.Property<int>("ClassId")
                        .HasColumnType("integer")
                        .HasColumnName("ClassID");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("RoomId")
                        .HasColumnType("integer")
                        .HasColumnName("RoomID");

                    b.Property<DateTime?>("StartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SubjectId")
                        .HasColumnType("integer")
                        .HasColumnName("SubjectID");

                    b.HasKey("LessonId")
                        .HasName("PK_tabLessons_1");

                    b.HasIndex("ClassId");

                    b.HasIndex("RoomId");

                    b.HasIndex("SubjectId");

                    b.ToTable("tabLesson", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabLog", b =>
                {
                    b.Property<long>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("LogID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("LogId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("LogId");

                    b.ToTable("tabLog", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabRoom", b =>
                {
                    b.Property<int>("RoomId")
                        .HasColumnType("integer")
                        .HasColumnName("RoomID");

                    b.Property<string>("LongName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("RoomId");

                    b.ToTable("tabRoom", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabSubject", b =>
                {
                    b.Property<int>("SubjectId")
                        .HasColumnType("integer")
                        .HasColumnName("SubjectID");

                    b.Property<string>("LongName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("SubjectId")
                        .HasName("PK_tabSubjects");

                    b.ToTable("tabSubject", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabUser", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("UserID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserId"));

                    b.Property<string>("AzureUserId")
                        .HasMaxLength(75)
                        .HasColumnType("character varying(75)")
                        .HasColumnName("AzureUserID");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("LastSeen")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RefreshToken")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("UserId")
                        .HasName("PK_tabUsers");

                    b.ToTable("tabUser", (string)null);
                });

            modelBuilder.Entity("Api.Models.TabComputer", b =>
                {
                    b.HasOne("Api.Models.TabRoom", "Room")
                        .WithMany("TabComputers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_tabComputers_tabRooms");

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Api.Models.TabLesson", b =>
                {
                    b.HasOne("Api.Models.TabClass", "Class")
                        .WithMany("TabLessons")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_tabLessons_tabClasses");

                    b.HasOne("Api.Models.TabRoom", "Room")
                        .WithMany("TabLessons")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_tabLessons_tabRooms");

                    b.HasOne("Api.Models.TabSubject", "Subject")
                        .WithMany("TabLessons")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_tabLessons_tabSubjects");

                    b.Navigation("Class");

                    b.Navigation("Room");

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("Api.Models.TabClass", b =>
                {
                    b.Navigation("TabLessons");
                });

            modelBuilder.Entity("Api.Models.TabRoom", b =>
                {
                    b.Navigation("TabComputers");

                    b.Navigation("TabLessons");
                });

            modelBuilder.Entity("Api.Models.TabSubject", b =>
                {
                    b.Navigation("TabLessons");
                });
#pragma warning restore 612, 618
        }
    }
}

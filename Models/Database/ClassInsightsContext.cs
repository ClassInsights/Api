using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Api.Models.Database;

public partial class ClassInsightsContext : DbContext
{
    public ClassInsightsContext(DbContextOptions<ClassInsightsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Computer> Computers { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK_tabClasses_1");

            entity.ToTable("class");

            entity.Property(e => e.ClassId)
                .ValueGeneratedNever()
                .HasColumnName("class_id");
            entity.Property(e => e.AzureGroupId)
                .HasMaxLength(50)
                .HasColumnName("azure_group_id");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(20)
                .HasColumnName("display_name");
        });

        modelBuilder.Entity<Computer>(entity =>
        {
            entity.HasKey(e => e.ComputerId).HasName("PK_tabComputers");

            entity.ToTable("computer");

            entity.HasIndex(e => e.RoomId, "IX_tabComputer_RoomID");

            entity.Property(e => e.ComputerId).HasColumnName("computer_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.LastSeen).HasColumnName("last_seen");
            entity.Property(e => e.LastUser)
                .HasMaxLength(75)
                .HasColumnName("last_user");
            entity.Property(e => e.MacAddress)
                .HasMaxLength(50)
                .HasColumnName("mac_address");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.Version)
                .HasMaxLength(20)
                .HasColumnName("version");

            entity.HasOne(d => d.Room).WithMany(p => p.Computers)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_tabComputers_tabRooms");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK_tabLessons_1");

            entity.ToTable("lesson");

            entity.HasIndex(e => e.ClassId, "IX_tabLesson_ClassID");

            entity.HasIndex(e => e.RoomId, "IX_tabLesson_RoomID");

            entity.HasIndex(e => e.SubjectId, "IX_tabLesson_SubjectID");

            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.End).HasColumnName("end");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.Start).HasColumnName("start");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");

            entity.HasOne(d => d.Class).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_tabLessons_tabClasses");

            entity.HasOne(d => d.Room).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_tabLessons_tabRooms");

            entity.HasOne(d => d.Subject).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_tabLessons_tabSubjects");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_tabLog");

            entity.ToTable("log");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Message)
                .HasMaxLength(50)
                .HasColumnName("message");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK_tabRoom");

            entity.ToTable("room");

            entity.Property(e => e.RoomId)
                .ValueGeneratedNever()
                .HasColumnName("room_id");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.Regex)
                .HasMaxLength(100)
                .HasColumnName("regex");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK_tabSubjects");

            entity.ToTable("subject");

            entity.Property(e => e.SubjectId)
                .ValueGeneratedNever()
                .HasColumnName("subject_id");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_tabUsers");

            entity.ToTable("user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AzureUserId)
                .HasMaxLength(75)
                .HasColumnName("azure_user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.LastSeen).HasColumnName("last_seen");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(200)
                .HasColumnName("refresh_token");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

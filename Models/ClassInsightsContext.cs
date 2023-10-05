using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Api.Models;

public partial class ClassInsightsContext : DbContext
{
    public ClassInsightsContext(DbContextOptions<ClassInsightsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TabClass> TabClasses { get; set; }

    public virtual DbSet<TabComputer> TabComputers { get; set; }

    public virtual DbSet<TabLesson> TabLessons { get; set; }

    public virtual DbSet<TabLog> TabLogs { get; set; }

    public virtual DbSet<TabRoom> TabRooms { get; set; }

    public virtual DbSet<TabSubject> TabSubjects { get; set; }

    public virtual DbSet<TabUser> TabUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<TabClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK_tabClasses_1");

            entity.ToTable("tabClass");

            entity.Property(e => e.ClassId)
                .ValueGeneratedNever()
                .HasColumnName("ClassID");
            entity.Property(e => e.AzureGroupId)
                .HasMaxLength(50)
                .HasColumnName("AzureGroupID");
            entity.Property(e => e.Head).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<TabComputer>(entity =>
        {
            entity.HasKey(e => e.ComputerId).HasName("PK_tabComputers");

            entity.ToTable("tabComputer");

            entity.Property(e => e.ComputerId).HasColumnName("ComputerID");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
            entity.Property(e => e.LastUser).HasMaxLength(75);
            entity.Property(e => e.MacAddress).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.RoomId).HasColumnName("RoomID");

            entity.HasOne(d => d.Room).WithMany(p => p.TabComputers)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_tabComputers_tabRooms");
        });

        modelBuilder.Entity<TabLesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK_tabLessons_1");

            entity.ToTable("tabLesson");

            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.Class).WithMany(p => p.TabLessons)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_tabLessons_tabClasses");

            entity.HasOne(d => d.Room).WithMany(p => p.TabLessons)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_tabLessons_tabRooms");

            entity.HasOne(d => d.Subject).WithMany(p => p.TabLessons)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_tabLessons_tabSubjects");
        });

        modelBuilder.Entity<TabLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("tabLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<TabRoom>(entity =>
        {
            entity.HasKey(e => e.RoomId);

            entity.ToTable("tabRoom");

            entity.Property(e => e.RoomId)
                .ValueGeneratedNever()
                .HasColumnName("RoomID");
            entity.Property(e => e.LongName).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<TabSubject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK_tabSubjects");

            entity.ToTable("tabSubject");

            entity.Property(e => e.SubjectId)
                .ValueGeneratedNever()
                .HasColumnName("SubjectID");
            entity.Property(e => e.LongName).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TabUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_tabUsers");

            entity.ToTable("tabUser");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AzureUserId)
                .HasMaxLength(75)
                .HasColumnName("AzureUserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
            entity.Property(e => e.RefreshToken).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

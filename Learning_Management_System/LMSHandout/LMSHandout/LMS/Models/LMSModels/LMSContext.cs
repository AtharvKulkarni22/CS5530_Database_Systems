using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.3-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FName)
                    .HasMaxLength(100)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .HasMaxLength(100)
                    .HasColumnName("lName");
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasIndex(e => e.AssignedIn, "AssignedIn");

                entity.HasIndex(e => new { e.AName, e.AssignedIn }, "UniqueAssignment")
                    .IsUnique();

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("AssignmentID");

                entity.Property(e => e.AContents)
                    .HasMaxLength(8192)
                    .HasColumnName("aContents");

                entity.Property(e => e.AName)
                    .HasMaxLength(100)
                    .HasColumnName("aName");

                entity.Property(e => e.AssignedIn).HasColumnType("int(10) unsigned");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.MaxPoints).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.AssignedInNavigation)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.AssignedIn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasIndex(e => e.CategoryOf, "CategoryOf");

                entity.HasIndex(e => new { e.AcName, e.CategoryOf }, "UniqueAssignmentCategory")
                    .IsUnique();

                entity.Property(e => e.AssignmentCategoryId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("AssignmentCategoryID");

                entity.Property(e => e.AcName)
                    .HasMaxLength(100)
                    .HasColumnName("acName");

                entity.Property(e => e.CategoryOf).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Weight).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.CategoryOfNavigation)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.CategoryOf)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.OfferingOf, "OfferingOf");

                entity.HasIndex(e => e.ProfessorId, "ProfessorID");

                entity.HasIndex(e => new { e.SemesterYear, e.SemesterSeason, e.OfferingOf }, "UniqueClass")
                    .IsUnique();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("ClassID");

                entity.Property(e => e.EndTime).HasColumnType("time");

                entity.Property(e => e.Location).HasMaxLength(100);

                entity.Property(e => e.OfferingOf).HasColumnType("int(10) unsigned");

                entity.Property(e => e.ProfessorId)
                    .HasMaxLength(8)
                    .HasColumnName("ProfessorID")
                    .IsFixedLength();

                entity.Property(e => e.SemesterSeason).HasColumnType("enum('Spring','Fall','Summer')");

                entity.Property(e => e.SemesterYear).HasColumnType("int(10) unsigned");

                entity.Property(e => e.StartTime).HasColumnType("time");

                entity.HasOne(d => d.OfferingOfNavigation)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.OfferingOf)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_1");

                entity.HasOne(d => d.Professor)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.ProfessorId)
                    .HasConstraintName("Classes_ibfk_2");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.Listing, "Listing");

                entity.HasIndex(e => new { e.Number, e.Listing }, "UniqueCourse")
                    .IsUnique();

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("CourseID");

                entity.Property(e => e.CName)
                    .HasMaxLength(100)
                    .HasColumnName("cName");

                entity.Property(e => e.Listing).HasMaxLength(4);

                entity.Property(e => e.Number).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.ListingNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.Listing)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Subject)
                    .HasName("PRIMARY");

                entity.Property(e => e.Subject).HasMaxLength(4);

                entity.Property(e => e.DName)
                    .HasMaxLength(100)
                    .HasColumnName("dName");
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.ClassId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.ClassId, "ClassID");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(8)
                    .HasColumnName("StudentID")
                    .IsFixedLength();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("ClassID");

                entity.Property(e => e.Grade).HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_2");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_1");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.WorksIn, "WorksIn");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FName)
                    .HasMaxLength(100)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .HasMaxLength(100)
                    .HasColumnName("lName");

                entity.Property(e => e.WorksIn).HasMaxLength(4);

                entity.HasOne(d => d.WorksInNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.WorksIn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Major, "Major");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FName)
                    .HasMaxLength(100)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .HasMaxLength(100)
                    .HasColumnName("lName");

                entity.Property(e => e.Major).HasMaxLength(4);

                entity.HasOne(d => d.MajorNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.Major)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.AssignmentId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.AssignmentId, "AssignmentID");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(8)
                    .HasColumnName("StudentID")
                    .IsFixedLength();

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("AssignmentID");

                entity.Property(e => e.SContents)
                    .HasMaxLength(8192)
                    .HasColumnName("sContents");

                entity.Property(e => e.Score).HasColumnType("int(10) unsigned");

                entity.Property(e => e.SubmissionTime).HasColumnType("datetime");

                entity.HasOne(d => d.Assignment)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AssignmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submissions_ibfk_1");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submissions_ibfk_2");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

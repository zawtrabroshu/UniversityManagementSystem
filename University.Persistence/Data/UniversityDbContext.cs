using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using University.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace University.Persistence.Data
{
    public class UniversityDbContext : DbContext
    {
        public UniversityDbContext(
            DbContextOptions<UniversityDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Subject>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Grade>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<Department>()
                .HasKey(d => d.Id);

            var studentSubjectComparer = new ValueComparer<List<Guid>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                a => a == null ? 0 : a.Aggregate(0, (hash, id) => HashCode.Combine(hash, id)),
                a => a == null ? new List<Guid>() : a.ToList()
            );

            var teacherSubjectComparer = new ValueComparer<List<Guid>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                a => a == null ? 0 : a.Aggregate(0, (hash, id) => HashCode.Combine(hash, id)),
                a => a == null ? new List<Guid>() : a.ToList()
            );

            var departmentTeacherComparer = new ValueComparer<List<Guid>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                a => a == null ? 0 : a.Aggregate(0, (hash, id) => HashCode.Combine(hash, id)),
                a => a == null ? new List<Guid>() : a.ToList()
            );

            modelBuilder.Entity<Student>()
                .Property(s => s.SubjectIds)
                .HasConversion(
                    value => JsonSerializer.Serialize(value, (JsonSerializerOptions)null),
                    value => JsonSerializer.Deserialize<List<Guid>>(value, (JsonSerializerOptions)null) ?? new List<Guid>()
                )
                .Metadata.SetValueComparer(studentSubjectComparer);

            modelBuilder.Entity<Teacher>()
                .Property(t => t.SubjectIds)
                .HasConversion(
                    value => JsonSerializer.Serialize(value, (JsonSerializerOptions)null),
                    value => JsonSerializer.Deserialize<List<Guid>>(value, (JsonSerializerOptions)null) ?? new List<Guid>()
                )
                .Metadata.SetValueComparer(teacherSubjectComparer);

            modelBuilder.Entity<Department>()
                .Property(d => d.TeacherIds)
                .HasConversion(
                    value => JsonSerializer.Serialize(value, (JsonSerializerOptions)null),
                    value => JsonSerializer.Deserialize<List<Guid>>(value, (JsonSerializerOptions)null) ?? new List<Guid>()
                )
                .Metadata.SetValueComparer(departmentTeacherComparer);
        }
    }
}

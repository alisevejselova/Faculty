using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Faculty.Models;

namespace Faculty.Models
{
    public class FacultyContext : DbContext
    {
        public FacultyContext (DbContextOptions<FacultyContext> options)
            : base(options)
        {
        }

        public DbSet<Faculty.Models.Course> Course { get; set; }

        public DbSet<Faculty.Models.Student> Student { get; set; }

        public DbSet<Faculty.Models.Teacher> Teacher { get; set; }

        public DbSet<Faculty.Models.Enrollment> Enrollment { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Enrollment>().HasOne<Student>(p => p.Student)
                .WithMany(p => p.Courses)
                .HasForeignKey(p => p.StudentId);

            builder.Entity<Enrollment>().HasOne<Course>(p => p.Course)
                .WithMany(p => p.Students)
                .HasForeignKey(p => p.CourseId);



            builder.Entity<Course>().HasOne(m => m.FirstTeacher)
                                 .WithMany(m => m.FirstCourses)
                                 .HasForeignKey(m => m.FirstTeacherID);

            builder.Entity<Course>().HasOne(m => m.SecondTeacher)
                                          .WithMany(m => m.SecondCourses)
                                          .HasForeignKey(m => m.SecondTeacherID);
        }
    }
}

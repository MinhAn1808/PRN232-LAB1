using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Repositories.Data
{
    public class LMSDbContext : DbContext
    {
        public LMSDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Semester> Semesters => Set<Semester>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    }
}

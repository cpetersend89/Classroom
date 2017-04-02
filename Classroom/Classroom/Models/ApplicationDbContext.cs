using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Classroom.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<VirtualClassroom> VirtualClassrooms { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentFileDetail> AssignmentFileDetails { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestFileDetail> TestFileDetails { get; set; }
        public DbSet<Syllabus> Syllabi { get; set; }
        public DbSet<CompletedAssignment> CompletedAssignments { get; set; }
        public DbSet<CompletedAssignmentFileDetails> CompletedAssignmentFileDetails { get; set; }
        public DbSet<CompletedTest> CompletedTests { get; set; }
        public DbSet<CompletedTestFileDetails> CompletedTestFileDetails { get; set; }

        public DbSet<Grade> Grades { get; set; }



        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
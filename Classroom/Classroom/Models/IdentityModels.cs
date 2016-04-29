using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Classroom.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual Student Student { get; set; }
        public virtual Instructor Instructor { get; set; }
        public virtual Admin Admin { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AssignmentFileDetail> AssignmentFileDetails { get; set; }
        public DbSet<TestFileDetails> TestFileDetails { get; set; }
        public DbSet<SyllabusFileDetails> SyllabusFileDetails { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

        public DbSet<Semester> Semesters { get; set; }

        public System.Data.Entity.DbSet<Classroom.Models.VirtualClassroom> VirtualClassrooms { get; set; }
    }
}
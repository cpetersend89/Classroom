using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class VirtualClassroom
    {
        public int Id { get; set; }

        public Admin Administrator { get; set; }

        [Required]
        public string AdministratorId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Classroom Title")]
        public string ClassroomName { get; set; }


        public Course Course { get; set; }

        [Required]

        public int CourseId { get; set; }

        public Semester Semester { get; set; }

        [Required]
        public int SemesterId { get; set; }

        public virtual ICollection<Instructor> Instructors { get; set; }

        public virtual ICollection<Student> Students { get; set; }

        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Test> Tests { get; set; } 
    }
}
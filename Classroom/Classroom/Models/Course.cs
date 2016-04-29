using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Web;

namespace Classroom.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CourseId { get; set; }
        [Display(Name = "Course Title")]
        public string CourseTitle { get; set; }
        [Display(Name = "Course Description")]
        public string CourseDescription { get; set; }
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }
        public virtual ICollection<Semester> Semesters { get; set; } 
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Instructor> Instructors { get; set; }

    }
}
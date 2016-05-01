using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string DepartmentName { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Instructor> Instructors { get; set; }

    }
}
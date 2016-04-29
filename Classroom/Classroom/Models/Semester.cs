using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }
        [Display(Name = "Semester Title")]
        public string SemesterTitle { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SemesterStartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SemesterEndDate { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<VirtualClassroom> VirtualClassrooms { get; set; } 
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class Student : UserProfile
    {
        [Key]
        [Display(Name = "Student Id")]
        public int StudentId { get; set; }

        //[Display(Name = "Enrollment Date")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        //public DateTime EnrollmentDate { get; set; }

        public virtual ICollection<VirtualClassroom> VirtualClassrooms { get; set; }

    }
}
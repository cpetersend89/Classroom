using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class Instructor : UserProfile
    {
        [Key]
        [Display(Name = "Instructor Id")]
        public int InstructorId { get; set; }

        //[Display(Name = "Hire Date")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        //public DateTime HireDate { get; set; }
        public virtual ICollection<VirtualClassroom> VirtualClassrooms { get; set; }
    }
}
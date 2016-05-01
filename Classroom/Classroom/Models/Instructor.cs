using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Classroom.Models
{
    public class Instructor : UserProfileViewModel
    {
        [Key]
        [Display(Name = "Instructor Id")]
        public int InstructorId { get; set; }

        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime HireDate { get; set; }
        //public int DepartmentId { get; set; }
        public virtual ICollection<Department> Department { get; set; }
        public virtual ICollection<VirtualClassroom> VirtualClassrooms { get; set; }
    }
}
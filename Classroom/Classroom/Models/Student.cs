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
    public class Student : UserProfileViewModel
    {
        [Key]
        [Display(Name = "Student Id")]
        public int StudentId { get; set; }

        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EnrollmentDate { get; set; }

        //public int GradeId { get; set; }

        public virtual Grade Grade { get; set; }

        public virtual ICollection<VirtualClassroom> VirtualClassrooms { get; set; }

        public virtual ICollection<Assignment> CompletedAssignments { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using Classroom.Models;

namespace Classroom.ViewModels
{
    public class StudentTaskViewModel
    {
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public bool TaskAvailable { get; set; }
        [Display(Name = "Available Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AvailableDate { get; set; }
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }
        public virtual Grade Grade { get; set; }
    }
}
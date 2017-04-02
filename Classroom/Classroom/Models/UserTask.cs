using System;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class UserTask
    {
        [Required]
        [Display(Name="Task Title")]
        public string TaskTitle { get; set; }

        [Required]
        public string TaskDescription { get; set; }

        public bool TaskAvailable { get; set; }

        [Required]
        [Display(Name = "Available Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AvailableDate { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name = "Points Worth")]
        public int PointsWorth { get; set; }
    }
}
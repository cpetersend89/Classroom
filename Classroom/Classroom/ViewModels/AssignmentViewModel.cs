using Classroom.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class AssignmentViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Available { get; set; }
        [Display(Name = "Available Date")]
        public DateTime? AvailableDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Points Worth")]
        public int PointsWorth { get; set; }

        public ICollection<Assignment> Assignments { get; set; }

        [Display(Name = "Completed Assignments")]
        public ICollection<CompletedAssignment> CompletedAssignments { get; set; }

        [Display(Name = "Classrooms")]
        public int ClassroomId { get; set; }

        public IEnumerable<VirtualClassroom> VirtualClassrooms { get; set; }
    }
}
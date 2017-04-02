using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Classroom.Models
{
    public class CompletedAssignment
    {
        [Key]
        [Column(Order = 1)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Submitted { get; set; } = false;
        [ForeignKey("Assignment")]
        [Column(Order = 2)]
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }
        [ForeignKey("VirtualClassroom")]
        [Column(Order = 3)]
        public int VirtualClassroomId { get; set; }
        public virtual VirtualClassroom VirtualClassroom { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public Guid GradeId { get; set; }
        public Grade Grade { get; set; }
        public bool Graded { get; set; } = false;
        [Display(Name = "Date & Time Completed")]
        public DateTime? CompletedDateTime { get; set; }
        public virtual ICollection<CompletedAssignmentFileDetails>  CompletedAssignmentFileDetails { get; set; }

    }
}
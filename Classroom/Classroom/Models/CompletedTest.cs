using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Classroom.Models
{
    public class CompletedTest
    {
        [Key]
        [Column(Order = 1)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Submitted { get; set; } = false;
        [ForeignKey("Test")]
        [Column(Order = 2)]
        public int TestId { get; set; }
        public virtual Test Test { get; set; }
        [ForeignKey("VirtualClassroom")]
        [Column(Order = 3)]
        public int VirtualClassroomId { get; set; }
        public virtual VirtualClassroom VirtualClassroom { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public Guid GradeId { get; set; }
        public Grade Grade { get; set; }
        [Display(Name = "Date & Time Completed")]
        public DateTime? CompletedDateTime { get; set; }
        public virtual ICollection<CompletedTestFileDetails> CompletedTestFileDetails { get; set; }
        public bool Graded { get; set; }
    }
}
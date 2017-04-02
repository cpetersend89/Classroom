using Classroom.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class VirtualClassroomViewModel
    {
        [Display(Name = "Classrooms")]
        public int ClassroomId { get; set; }
        public VirtualClassroom VirtualClassroom { get; set; }
        public IEnumerable<VirtualClassroom> VirtualClassrooms { get; set; }

        public Assignment Assignment { get; set; }
        public Test Test { get; set; }

        public CompletedAssignment CompletedAssignment { get; set; }
        public CompletedTest CompletedTest { get; set; }

        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<Test> Tests { get; set; }
        public ICollection<CompletedAssignmentFileDetails> CompletedAssignmentFileDetails { get; set; }
        public ICollection<CompletedTestFileDetails> CompletedTestFileDetails { get; set; }
        public ICollection<CompletedAssignment> CompletedAssignments { get; set; }
        public ICollection<CompletedTest> CompletedTests { get; set; }
        public int StudentId { get; set; }

        public GradeBookViewModel GradeBookViewModel { get; set; }
        public AssignmentViewModel AssignmentViewModel { get; set; }
    }
}
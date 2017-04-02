using Classroom.Models;
using System.Collections.Generic;

namespace Classroom.ViewModels
{
    public class GradeBookViewModel
    {
        public VirtualClassroom VirtualClassroom { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<CompletedAssignment> CompletedAssignments { get; set; }
        public ICollection<Test> Tests { get; set; }
        public ICollection<CompletedTest> CompletedTests { get; set; }
    }
}
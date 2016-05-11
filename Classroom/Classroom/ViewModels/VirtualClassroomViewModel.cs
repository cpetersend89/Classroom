using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classroom.Models;

namespace Classroom.ViewModels
{
    public class VirtualClassroomViewModel
    {
        public virtual VirtualClassroom VirtualClassroom { get; set; }
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public int CompletedAssignmentId { get; set; }
        public virtual CompletedAssignment CompletedAssignment { get; set; }
        public virtual ICollection<CompletedAssignment> CompletedAssignments { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
        public virtual ICollection<CompletedTest> CompletedTests { get; set; }
        public virtual ICollection<Syllabus> Syllabus { get; set; }

    }
}
using System.Collections.Generic;

namespace Classroom.Models
{
    public class Assignment : UserTask
    {
        public int Id { get; set; }
        public virtual ICollection<AssignmentFileDetail> FileDetails { get; set; }
        public virtual  ICollection<VirtualClassroom> Classrooms { get; set; }
        public virtual ICollection<CompletedAssignment> CompletedAssignments { get; set; }
        public virtual ICollection<CompletedAssignmentFileDetails> CompletedAssignmentFileDetails { get; set; }
    }
}
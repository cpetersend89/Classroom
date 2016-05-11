using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class CompletedAssignment
    {
        public int CompletedAssignmentId { get; set; }
        public DateTime CompletedDateTime { get; set; } = DateTime.Now;
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment{ get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public virtual ICollection<CompletedAssignmentFileDetails> CompletedAssignmentsFileDetails { get; set; }

    }
}
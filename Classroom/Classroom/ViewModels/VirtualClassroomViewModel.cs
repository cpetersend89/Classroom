using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classroom.Models;

namespace Classroom.ViewModels
{
    public class VirtualClassroomViewModel
    {
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<CompletedAssignment> CompletedAssignments { get; set; }
    }
}
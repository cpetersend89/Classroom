using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.ViewModels
{
    public class AssignedAssignmentData
    {
        public int AssignmentId { get; set; }
        public string TaskTitle { get; set; }
        public bool Assigned { get; set; }
    }
}
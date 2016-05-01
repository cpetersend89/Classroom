using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.ViewModels
{
    public class AssignedClassroomData
    {
        public int VirtualClassroomId { get; set; }
        public string ClassroomTitle { get; set; }
        public bool Assigned { get; set; }
    }
}
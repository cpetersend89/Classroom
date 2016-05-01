using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.ViewModels
{
    public class AssignedStudentData
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public bool Assigned { get; set; }
    }
}
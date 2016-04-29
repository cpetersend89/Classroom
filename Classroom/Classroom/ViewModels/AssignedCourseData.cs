using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.ViewModels
{
    public class AssignedCourseData
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public bool Assigned { get; set; }
    }
}
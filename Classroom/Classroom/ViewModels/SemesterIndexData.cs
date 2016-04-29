using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classroom.Models;

namespace Classroom.ViewModels
{
    public class SemesterIndexData
    {
        public IEnumerable<Semester> Semesters { get; set; }
        public IEnumerable<Course> Courses { get; set; }
    }
}
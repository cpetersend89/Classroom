using Classroom.Models;
using System.Collections.Generic;

namespace Classroom.ViewModels
{
    public class TasksViewModel
    {
        public VirtualClassroom Classroom { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }
    }
}
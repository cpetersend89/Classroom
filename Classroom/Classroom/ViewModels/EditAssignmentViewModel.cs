using Classroom.Models;

namespace Classroom.ViewModels
{
    public class EditAssignmentViewModel
    {
        public CompletedAssignment CompletedAssignment { get; set; }
        public Assignment Assignment { get; set; }

        public VirtualClassroom VirtualClassroom { get; set; }
    }
}
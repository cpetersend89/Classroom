using Classroom.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class VirtualClassroomFormViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public string ClassroomName { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Term")]
        public int SemesterId { get; set; }

        public IEnumerable<Course> Courses { get; set; }

        public IEnumerable<Semester> Semesters { get; set; }

        public IEnumerable<Instructor> Instructors { get; set; }

        public IEnumerable<Student> Students { get; set; }
    }
}
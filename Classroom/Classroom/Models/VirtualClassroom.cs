using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class VirtualClassroom
    {
        [Key]
        public int VirtualClassroomId { get; set; }
        public string ClassroomTitle { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int SemesterId { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual ICollection<Instructor> Instructors { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Syllabus> Syllabus { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Test> Tests { get; set; }


    }
}
using Classroom.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class UserViewModel
    {
        public ApplicationUser User { get; set; }

        [Display(Name = "Classrooms")]
        public int ClassroomId { get; set; }

        [Display(Name = "User Photo")]
        public byte[] UserPhoto { get; set; }

        public VirtualClassroom VirtualClassroom { get; set; }

        public ICollection<VirtualClassroom> VirtualClassrooms { get; set; }
    }
}
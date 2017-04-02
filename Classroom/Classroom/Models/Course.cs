using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
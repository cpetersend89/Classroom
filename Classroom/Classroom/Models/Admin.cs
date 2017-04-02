using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class Admin : UserProfile
    {
        [Key]
        [Display(Name = "Admin Id")]
        public int AdminId { get; set; }
    }
}
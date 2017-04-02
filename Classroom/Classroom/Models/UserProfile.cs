using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class UserProfile
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Name")]
        public string FullName => $"{LastName}, {FirstName}";

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
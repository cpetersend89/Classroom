using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
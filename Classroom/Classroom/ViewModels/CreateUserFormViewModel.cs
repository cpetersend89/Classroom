using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class CreateUserFormViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public string RoleId { get; set; }


        [Display(Name = "User Role")]
        public IEnumerable<IdentityRole> UserRoles { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Classroom.ViewModels;

namespace Classroom.Models
{
    public class Admin : UserProfileViewModel
    {
        [Key]
        [Display(Name = "Admin Id")]
        public int AdminId { get; set; }
    }
}
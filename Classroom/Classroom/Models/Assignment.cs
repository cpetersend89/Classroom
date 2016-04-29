using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Classroom.ViewModels;

namespace Classroom.Models
{
    public class Assignment : StudentTaskViewModel
    {
        [Key]
        public int AssignmentId { get; set; }

        public virtual ICollection<AssignmentFileDetail> FileDetails { get; set; }
    }
}
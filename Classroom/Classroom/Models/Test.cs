using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Classroom.ViewModels;

namespace Classroom.Models
{
    public class Test : StudentTaskViewModel
    {
        [Key]
        public int TestId { get; set; }
        public virtual ICollection<TestFileDetails> FileDetails { get; set; }
    }
}
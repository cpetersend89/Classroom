using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class CompletedTestFileDetails
    {
        [Key]
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public int TestId { get; set; }
        public virtual Test Test { get; set; }
    }
}
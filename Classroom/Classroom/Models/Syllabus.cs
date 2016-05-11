using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class Syllabus
    {
        [Key]
        public int SyllabusId { get; set; }
        public string SyllabusTitle { get; set; }
        public string SyllabusDescription { get; set; }
        public virtual ICollection<SyllabusFileDetails> FileDetails { get; set; }
        public virtual ICollection<VirtualClassroom> VirtualClassrooms { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }
        public double GradePoint { get; set; }
        public double GradePercentage { get; set; }
        public double Weight { get; set; }
        public double Average { get; set; }
    }
}
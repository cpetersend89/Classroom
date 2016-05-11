using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.Models
{
    public class CompletedTest
    {
        public int CompletedTestId { get; set; }
        public DateTime CompletedDateTime { get; set; } = DateTime.Now;
        public int TestId { get; set; }
        public virtual Test Test { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public virtual ICollection<CompletedTestFileDetails> CompletedTestsFileDetails { get; set; }
    }
}
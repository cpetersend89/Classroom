using System.Collections.Generic;

namespace Classroom.Models
{
    public class Test : UserTask
    {
        public int Id { get; set; }
        public virtual ICollection<TestFileDetail> FileDetails { get; set; }
        public virtual ICollection<VirtualClassroom> Classrooms { get; set; }
        public virtual ICollection<CompletedTest> CompletedTests { get; set; }
        public virtual ICollection<CompletedTestFileDetails> CompletedTestFileDetails { get; set; }

    }
}
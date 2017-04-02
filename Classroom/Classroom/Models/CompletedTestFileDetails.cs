using System;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class CompletedTestFileDetails : FileDetail
    {
        [Key]
        public Guid FileId { get; set; }
        public int CompletedTestId { get; set; }
        public CompletedTest CompletedTest { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class TestFileDetail : FileDetail
    {
        [Key]
        public Guid FileId { get; set; }
        public int TestId { get; set; }
        public virtual Test Test { get; set; }
    }
}
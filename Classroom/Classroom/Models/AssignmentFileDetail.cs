using System;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class AssignmentFileDetail : FileDetail
    {
        [Key]
        public Guid FileId { get; set; }
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class CompletedAssignmentFileDetails : FileDetail
    {
        [Key]
        public Guid FileId { get; set; }
        public int CompletedAssignmentId { get; set; }
        public CompletedAssignment CompletedAssignment { get; set; }
    }
}
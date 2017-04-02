using System;

namespace Classroom.Models
{
    public class UserImage : FileDetail
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

    }
}
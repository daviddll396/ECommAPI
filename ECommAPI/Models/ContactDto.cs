﻿using System.ComponentModel.DataAnnotations;

namespace ECommAPI.Models
{
    public class ContactDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string? Phone { get; set; } = "";

        public int SubjectId { get; set; }

        [Required, MinLength(20), MaxLength(100)]
        public string Message { get; set; } = "";
    }
}

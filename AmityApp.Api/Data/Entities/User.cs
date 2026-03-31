using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AmityApp.Api.Data.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(40)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public string? PhotoPath { get; set; }
        public string? PhotoUrl { get; set; }
    }
}

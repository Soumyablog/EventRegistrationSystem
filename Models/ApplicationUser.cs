using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Registration> Registrations { get; set; }
            = new List<Registration>();

    }
}

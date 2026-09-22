using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models
{
    public class Registration
    {  
            public int Id { get; set; }

            [Required]
            public string UserId { get; set; } = string.Empty;

            public ApplicationUser User { get; set; } = null!;

            [Required]
            public int EventId { get; set; }

            public Event Event { get; set; } = null!;

            public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

            public RegistrationStatus Status { get; set; }
                = RegistrationStatus.Confirmed;
        }

        public enum RegistrationStatus
        {
            Confirmed = 1,
            Cancelled = 2
        }

}

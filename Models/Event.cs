using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(1, 100000)]
        public int Capacity { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDeadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public ICollection<Registration> Registrations { get; set; }
            = new List<Registration>();

    }
}

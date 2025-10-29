using System.ComponentModel.DataAnnotations;

namespace EventTicketingSystem.Models.ViewModels
{
    public class EventViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Event Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Event Date")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Registration Deadline")]
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}
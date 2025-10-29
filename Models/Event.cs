namespace EventTicketingSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public decimal Price { get; set; }
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
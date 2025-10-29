namespace EventTicketingSystem.Models
{
    public class TicketEmail
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        public Registration Registration { get; set; } = null!;
    }
}
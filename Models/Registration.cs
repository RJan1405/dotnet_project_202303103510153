namespace EventTicketingSystem.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? TransactionId { get; set; }
        
        public ApplicationUser User { get; set; } = null!;
        public Event Event { get; set; } = null!;
    }
}
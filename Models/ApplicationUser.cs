using Microsoft.AspNetCore.Identity;

namespace EventTicketingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Registration> Registrations { get; set; }
    }
}
using EventTicketingSystem.Data;
using EventTicketingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketingSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            
            if (eventItem == null)
            {
                return NotFound();
            }
            
            return View(eventItem);
        }

        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Challenge();
            }
            
            var registrations = await _context.Registrations
                .Include(r => r.Event)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();
                
            return View(registrations);
        }
    }
}
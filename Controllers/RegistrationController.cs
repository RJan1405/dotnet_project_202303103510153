using EventTicketingSystem.Data;
using EventTicketingSystem.Models;
using EventTicketingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace EventTicketingSystem.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        
        public RegistrationController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }
        
        public async Task<IActionResult> Register(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }
            
            if (DateTime.Now > eventItem.RegistrationDeadline)
            {
                TempData["ErrorMessage"] = "Registration for this event has closed.";
                return RedirectToAction("Details", "Event", new { id = id });
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Challenge();
            }
            
            // Check if already registered
            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == user.Id);
                
            if (existingRegistration != null)
            {
                TempData["ErrorMessage"] = "You are already registered for this event.";
                return RedirectToAction("Details", "Event", new { id = id });
            }
            
            ViewBag.Event = eventItem;
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id, string returnUrl = null)
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Challenge();
            }
            
            // Create registration
            var registration = new Registration
            {
                UserId = user.Id,
                EventId = id,
                PaymentStatus = "Pending"
            };
            
            _context.Add(registration);
            await _context.SaveChangesAsync();
            
            // Redirect to payment
            return RedirectToAction(nameof(Payment), new { id = registration.Id });
        }
        
        public async Task<IActionResult> Payment(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null || registration.UserId != user.Id)
            {
                return Challenge();
            }
            
            ViewBag.RazorpayKey = _configuration["PaymentSettings:RazorpayKeyId"];
            
            return View(registration);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id, string transactionId)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null || registration.UserId != user.Id)
            {
                return Challenge();
            }
            
            // Update registration
            registration.PaymentStatus = "Success";
            registration.TransactionId = transactionId;
            
            _context.Update(registration);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(PaymentSuccess), new { id = registration.Id });
        }
        
        public async Task<IActionResult> PaymentSuccess(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            return View(registration);
        }
        
        public async Task<IActionResult> SendConfirmationEmail(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null || registration.UserId != user.Id)
            {
                return Challenge();
            }
            
            // Send confirmation email
            string subject = "Event Registration Confirmation";
            string message = $"<h1>Registration Confirmation</h1>" +
                            $"<p>Hi {registration.User.Name},</p>" +
                            $"<p>Your ticket for <strong>{registration.Event.Title}</strong> is confirmed!</p>" +
                            $"<p>Date: {registration.Event.Date.ToString("dddd, dd MMMM yyyy")}</p>" +
                            $"<p>Thank you for registering!</p>";
                            
            await _emailService.SendEmailAsync(registration.User.Email, subject, message);

            _context.TicketEmails.Add(new TicketEmail
            {
                UserId = registration.UserId,
                RegistrationId = registration.Id,
                Subject = subject,
                BodyHtml = message,
                SentAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Confirmation email sent successfully!";
            return RedirectToAction("EmailTickets");
        }
        
        // Mail inbox: list ticket emails for the current user
        [Authorize]
        public async Task<IActionResult> EmailTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
        
            var emails = await _context.TicketEmails
                .Include(te => te.Registration)
                .ThenInclude(r => r.Event)
                .Where(te => te.UserId == user.Id)
                .OrderByDescending(te => te.SentAt)
                .ToListAsync();
        
            return View(emails);
        }

        [Authorize]
        public async Task<IActionResult> DownloadTicket(int emailId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var email = await _context.TicketEmails
                .Include(te => te.Registration).ThenInclude(r => r.Event)
                .Include(te => te.Registration).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(te => te.Id == emailId && te.UserId == user.Id);

            if (email == null) return NotFound();

            var registration = email.Registration;
            if (registration.PaymentStatus != "Success")
            {
                return BadRequest("Payment not completed for this ticket.");
            }

            try
            {
                var pdfBytes = await new Rotativa.AspNetCore.ViewAsPdf("TicketPdf", registration)
                {
                    FileName = $"Ticket_{registration.Event.Title}_{registration.Id}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A5,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
                }.BuildFile(ControllerContext);

                return File(pdfBytes, "application/pdf",
                    $"Ticket_{registration.Event.Title}_{registration.Id}.pdf");
            }
            catch
            {
                return View("TicketPdf", registration);
            }
        }
    }
}
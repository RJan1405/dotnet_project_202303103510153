using EventTicketingSystem.Data;
using EventTicketingSystem.Models;
using EventTicketingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketingSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;

        public PaymentController(
            AppDbContext context,
            IPaymentService paymentService,
            IEmailService emailService)
        {
            _context = context;
            _paymentService = paymentService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Process(string orderId, decimal amount)
        {
            // Extract registration ID from order ID
            var registrationIdStr = orderId.Replace("ORDER_", "");
            if (!int.TryParse(registrationIdStr, out int registrationId))
            {
                return BadRequest("Invalid order ID");
            }

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (registration == null)
            {
                return NotFound();
            }

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.Event = registration.Event;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(string orderId)
        {
            // Extract registration ID from order ID
            var registrationIdStr = orderId.Replace("ORDER_", "");
            if (!int.TryParse(registrationIdStr, out int registrationId))
            {
                return BadRequest("Invalid order ID");
            }

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (registration == null)
            {
                return NotFound();
            }

            // Update payment status
            registration.PaymentStatus = "Success";
            registration.TransactionId = _paymentService.GenerateTransactionId();
            await _context.SaveChangesAsync();

            // Send confirmation email
            var subject = $"Ticket Confirmation for {registration.Event.Title}";
            var message = $"<h1>Ticket Confirmation</h1>" +
                          $"<p>Hi {registration.User.Name},</p>" +
                          $"<p>Your ticket for <strong>{registration.Event.Title}</strong> is confirmed!</p>" +
                          $"<p>Event Date: {registration.Event.Date.ToString("dddd, dd MMMM yyyy")}</p>" +
                          $"<p>Transaction ID: {registration.TransactionId}</p>" +
                          $"<p>Thank you for registering!</p>";

            await _emailService.SendEmailAsync(registration.User.Email, subject, message);

            TempData["SuccessMessage"] = "Payment successful! A confirmation email has been sent.";
            return RedirectToAction("MyEvents", "Event");
        }
    }
}
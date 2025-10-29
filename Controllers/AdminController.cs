using EventTicketingSystem.Data;
using EventTicketingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Rotativa.AspNetCore;
using System.IO;

namespace EventTicketingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
        }

        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(Event eventItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        public async Task<IActionResult> EditEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }
            
            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventItem);
        }

        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }
            
            return View(eventItem);
        }

        [HttpPost, ActionName("DeleteEvent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEventConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EventRegistrations(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }
            
            var registrations = await _context.Registrations
                .Include(r => r.User)
                .Where(r => r.EventId == id)
                .ToListAsync();

            ViewBag.EventTitle = eventItem.Title;
            ViewBag.EventId = eventItem.Id;
            ViewBag.EventDate = eventItem.Date;
            ViewBag.TotalRegistrations = registrations.Count;

            return View(registrations);
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ExportPdf(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            var registrations = await _context.Registrations
                .Include(r => r.User)
                .Where(r => r.EventId == eventId)
                .ToListAsync();

            ViewBag.EventTitle = eventItem.Title;
            ViewBag.EventDate = eventItem.Date;
            ViewBag.TotalRegistrations = registrations.Count;

            return new ViewAsPdf("EventReportPdf", registrations)
            {
                FileName = $"Event_{eventItem.Title}_Registrations.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }

        public async Task<IActionResult> ExportExcel(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            var registrations = await _context.Registrations
                .Include(r => r.User)
                .Where(r => r.EventId == eventId)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Registrations");
                ws.Cell(1, 1).Value = "Event:";
                ws.Cell(1, 2).Value = eventItem.Title;
                ws.Cell(2, 1).Value = "Date:";
                ws.Cell(2, 2).Value = eventItem.Date.ToString("yyyy-MM-dd");
                ws.Cell(3, 1).Value = "Total Registrations:";
                ws.Cell(3, 2).Value = registrations.Count;
        
                ws.Cell(5, 1).Value = "User Name";
                ws.Cell(5, 2).Value = "Email";
                ws.Cell(5, 3).Value = "Phone";
                ws.Cell(5, 4).Value = "Payment Status";
                ws.Cell(5, 5).Value = "Transaction ID";
        
                int row = 6;
                foreach (var reg in registrations)
                {
                    ws.Cell(row, 1).Value = reg.User.Name;
                    ws.Cell(row, 2).Value = reg.User.Email;
                    ws.Cell(row, 3).Value = reg.User.PhoneNumber ?? "N/A";
                    ws.Cell(row, 4).Value = reg.PaymentStatus;
                    ws.Cell(row, 5).Value = string.IsNullOrEmpty(reg.TransactionId) ? "N/A" : reg.TransactionId;
                    row++;
                }
        
                var table = ws.Range(5, 1, row - 1, 5).CreateTable();
                ws.Columns().AdjustToContents();
        
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Event_{eventItem.Title}_Registrations.xlsx");
                }
            }
        }
    }
}
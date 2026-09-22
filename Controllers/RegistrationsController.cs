//using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EventRegistrationSystem.Controllers
{
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --------------------------------------------
        // POST: /Registrations/Register
        // --------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var eventItem = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (!eventItem.IsActive)
            {
                TempData["Error"] =
                    "This event is no longer active.";

                return RedirectToAction(
                    "Details",
                    "Event",
                    new { id = eventId });
            }

            // ----------------------------------------
            // Registration deadline
            // ----------------------------------------

            if (DateTime.Now >
                eventItem.RegistrationDeadline)
            {
                TempData["Error"] =
                    "Registration deadline has passed.";

                return RedirectToAction(
                    "Details",
                    "Event",
                    new { id = eventId });
            }

            // ----------------------------------------
            // Check if event has already occurred
            // ----------------------------------------

            if (eventItem.EventDate.Date < DateTime.Today)
            {
                TempData["Error"] =
                    "This event has already taken place.";

                return RedirectToAction(
                    "Details",
                    "Event",
                    new { id = eventId });
            }

            // ----------------------------------------
            // Check existing registration
            // ----------------------------------------

            var existingRegistration =
                await _context.Registrations
                    .FirstOrDefaultAsync(r =>
                        r.EventId == eventId &&
                        r.UserId == userId);

            if (existingRegistration != null)
            {
                if (existingRegistration.Status ==
                    RegistrationStatus.Confirmed)
                {
                    TempData["Error"] =
                        "You are already registered for this event.";

                    return RedirectToAction(
                        "Details",
                        "Event",
                        new { id = eventId });
                }

                // Reactivate cancelled registration
                var confirmedCount =
                    await _context.Registrations.CountAsync(r =>
                        r.EventId == eventId &&
                        r.Status ==
                        RegistrationStatus.Confirmed);

                if (confirmedCount >= eventItem.Capacity)
                {
                    TempData["Error"] =
                        "This event is currently full.";

                    return RedirectToAction(
                        "Details",
                        "Event",
                        new { id = eventId });
                }

                existingRegistration.Status =
                    RegistrationStatus.Confirmed;

                existingRegistration.RegisteredAt =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Your registration has been confirmed.";

                return RedirectToAction(
                    "MyRegistrations");
            }

            // ----------------------------------------
            // Check capacity
            // ----------------------------------------

            var currentRegistrations =
                await _context.Registrations
                    .CountAsync(r =>
                        r.EventId == eventId &&
                        r.Status ==
                        RegistrationStatus.Confirmed);

            if (currentRegistrations >= eventItem.Capacity)
            {
                TempData["Error"] =
                    "This event is currently full.";

                return RedirectToAction(
                    "Details",
                    "Event",
                    new { id = eventId });
            }

            // ----------------------------------------
            // Create registration
            // ----------------------------------------

            var registration = new Registration
            {
                UserId = userId,
                EventId = eventId,
                RegisteredAt = DateTime.UtcNow,
                Status = RegistrationStatus.Confirmed
            };

            _context.Registrations.Add(registration);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "You may already be registered for this event.";

                return RedirectToAction(
                    "Details",
                    "Event",
                    new { id = eventId });
            }

            TempData["Success"] =
                "Registration completed successfully.";

            return RedirectToAction(
                "MyRegistrations");
        }

        // --------------------------------------------
        // GET: /Registrations/MyRegistrations
        // --------------------------------------------

        [HttpGet]
        public async Task<IActionResult> MyRegistrations()
        {
            var userId =
                _userManager.GetUserId(User);

            var registrations =
                await _context.Registrations
                    .Include(r => r.Event)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RegisteredAt)
                    .ToListAsync();

            return View(registrations);
        }

        // --------------------------------------------
        // POST: /Registrations/Cancel
        // --------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId =
                _userManager.GetUserId(User);

            var registration =
                await _context.Registrations
                    .Include(r => r.Event)
                    .FirstOrDefaultAsync(r =>
                        r.Id == id &&
                        r.UserId == userId);

            if (registration == null)
            {
                return NotFound();
            }

            if (registration.Status ==
                RegistrationStatus.Cancelled)
            {
                TempData["Error"] =
                    "Registration has already been cancelled.";

                return RedirectToAction(
                    nameof(MyRegistrations));
            }

            registration.Status =
                RegistrationStatus.Cancelled;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your registration has been cancelled.";

            return RedirectToAction(
                nameof(MyRegistrations));
        }

        // --------------------------------------------
        // GET: /Registrations/Participants/5
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Participants(int eventId)
        {
            var eventItem =
                await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                return NotFound();
            }

            var registrations =
                await _context.Registrations
                    .Include(r => r.User)
                    .Where(r =>
                        r.EventId == eventId &&
                        r.Status ==
                        RegistrationStatus.Confirmed)
                    .OrderBy(r => r.RegisteredAt)
                    .ToListAsync();

            ViewBag.Event = eventItem;

            return View(registrations);

        }
    }
}

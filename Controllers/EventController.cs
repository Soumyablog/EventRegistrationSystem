//using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EventRegistrationSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------
        // GET: /Events
        // --------------------------------------------

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Registrations)
                .Where(e => e.IsActive)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();

            return View(events);
        }

        // --------------------------------------------
        // GET: /Events/Details/5
        // --------------------------------------------

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // --------------------------------------------
        // GET: /Event/Create
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // --------------------------------------------
        // POST: /Event/Create
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (model.RegistrationDeadline > model.EventDate)
            {
                ModelState.AddModelError(
                    "RegistrationDeadline",
                    "Registration deadline must be before the event date.");
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be after start time.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;

            _context.Events.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Event created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------
        // GET: /Events/Edit/5
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem =
                await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // --------------------------------------------
        // POST: /Events/Edit/5
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Event model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var existingEvent =
                await _context.Events
                    .Include(e => e.Registrations)
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            var confirmedRegistrations =
                existingEvent.Registrations
                    .Count(r => r.Status ==
                                RegistrationStatus.Confirmed);

            if (model.Capacity < confirmedRegistrations)
            {
                ModelState.AddModelError(
                    "Capacity",
                    $"Capacity cannot be less than the current number of confirmed registrations ({confirmedRegistrations}).");
            }

            if (model.RegistrationDeadline > model.EventDate)
            {
                ModelState.AddModelError(
                    "RegistrationDeadline",
                    "Registration deadline must be before the event date.");
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be after start time.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            existingEvent.Title = model.Title;
            existingEvent.Description = model.Description;
            existingEvent.Venue = model.Venue;
            existingEvent.EventDate = model.EventDate;
            existingEvent.StartTime = model.StartTime;
            existingEvent.EndTime = model.EndTime;
            existingEvent.Capacity = model.Capacity;
            existingEvent.RegistrationDeadline =
                model.RegistrationDeadline;
            existingEvent.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Event updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------
        // GET: /Events/Delete/5
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // --------------------------------------------
        // POST: /Events/Delete/5
        // --------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem =
                await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Event deactivated successfully.";

            return RedirectToAction(nameof(Index));

        }
    }
}

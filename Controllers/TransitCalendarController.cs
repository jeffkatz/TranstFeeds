using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using TransitFeeds.Models;

namespace TransitFeeds.Controllers
{
    public class TransitCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransitCalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransitCalendar
        public async Task<IActionResult> Index()
        {
            return View(await _context.TransitCalendars.ToListAsync());
        }

        // GET: TransitCalendar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transitCalendar = await _context.TransitCalendars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transitCalendar == null)
            {
                return NotFound();
            }

            return View(transitCalendar);
        }

        // GET: TransitCalendar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransitCalendar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsServiceId,StartDate,EndDate,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday")] TransitCalendar transitCalendar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transitCalendar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transitCalendar);
        }

        // GET: TransitCalendar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transitCalendar = await _context.TransitCalendars.FindAsync(id);
            if (transitCalendar == null)
            {
                return NotFound();
            }
            return View(transitCalendar);
        }

        // POST: TransitCalendar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsServiceId,StartDate,EndDate,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday")] TransitCalendar transitCalendar)
        {
            if (id != transitCalendar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transitCalendar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransitCalendarExists(transitCalendar.Id))
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
            return View(transitCalendar);
        }

        // GET: TransitCalendar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transitCalendar = await _context.TransitCalendars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transitCalendar == null)
            {
                return NotFound();
            }

            return View(transitCalendar);
        }

        // POST: TransitCalendar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transitCalendar = await _context.TransitCalendars.FindAsync(id);
            if (transitCalendar != null)
            {
                _context.TransitCalendars.Remove(transitCalendar);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TransitCalendarExists(int id)
        {
            return _context.TransitCalendars.Any(e => e.Id == id);
        }

        // POST: TransitCalendar/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var calendarsToDelete = await _context.TransitCalendars
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (calendarsToDelete.Any())
            {
                _context.TransitCalendars.RemoveRange(calendarsToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {calendarsToDelete.Count} calendars.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

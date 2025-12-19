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
    public class CalendarDatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarDatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CalendarDates
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CalendarDates.Include(c => c.TransitCalendar);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CalendarDates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var calendarDate = await _context.CalendarDates
                .Include(c => c.TransitCalendar)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendarDate == null) return NotFound();

            return View(calendarDate);
        }

        // GET: CalendarDates/Create
        public IActionResult Create()
        {
            ViewData["TransitCalendarId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId");
            return View();
        }

        // POST: CalendarDates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsServiceId,Date,ExceptionType,TransitCalendarId")] CalendarDate calendarDate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(calendarDate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TransitCalendarId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId", calendarDate.TransitCalendarId);
            return View(calendarDate);
        }

        // GET: CalendarDates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var calendarDate = await _context.CalendarDates.FindAsync(id);
            if (calendarDate == null) return NotFound();
            ViewData["TransitCalendarId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId", calendarDate.TransitCalendarId);
            return View(calendarDate);
        }

        // POST: CalendarDates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsServiceId,Date,ExceptionType,TransitCalendarId")] CalendarDate calendarDate)
        {
            if (id != calendarDate.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(calendarDate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalendarDateExists(calendarDate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TransitCalendarId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId", calendarDate.TransitCalendarId);
            return View(calendarDate);
        }

        // GET: CalendarDates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var calendarDate = await _context.CalendarDates
                .Include(c => c.TransitCalendar)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendarDate == null) return NotFound();

            return View(calendarDate);
        }

        // POST: CalendarDates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calendarDate = await _context.CalendarDates.FindAsync(id);
            if (calendarDate != null)
            {
                _context.CalendarDates.Remove(calendarDate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalendarDateExists(int id)
        {
            return _context.CalendarDates.Any(e => e.Id == id);
        }
    }
}

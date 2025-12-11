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
    public class StopTimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StopTimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StopTimes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StopTimes.Include(s => s.Stop).Include(s => s.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: StopTimes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stopTime = await _context.StopTimes
                .Include(s => s.Stop)
                .Include(s => s.Trip)!.ThenInclude(t => t.StopTimes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stopTime == null)
            {
                return NotFound();
            }

            return View(stopTime);
        }

        // GET: StopTimes/Create
        public IActionResult Create()
        {
            ViewData["StopId"] = new SelectList(_context.Stops, "Id", "StopName");
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "GtfsTripId");
            return View();
        }

        // POST: StopTimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TripId,StopId,StopSequence,ArrivalTime,DepartureTime,StopHeadsign,PickupType,DropOffType,ShapeDistTraveled")] StopTime stopTime)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stopTime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StopId"] = new SelectList(_context.Stops, "Id", "StopName", stopTime.StopId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "GtfsTripId", stopTime.TripId);
            return View(stopTime);
        }

        // GET: StopTimes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stopTime = await _context.StopTimes.FindAsync(id);
            if (stopTime == null)
            {
                return NotFound();
            }
            ViewData["StopId"] = new SelectList(_context.Stops, "Id", "StopName", stopTime.StopId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "GtfsTripId", stopTime.TripId);
            return View(stopTime);
        }

        // POST: StopTimes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TripId,StopId,StopSequence,ArrivalTime,DepartureTime,StopHeadsign,PickupType,DropOffType,ShapeDistTraveled")] StopTime stopTime)
        {
            if (id != stopTime.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stopTime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StopTimeExists(stopTime.Id))
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
            ViewData["StopId"] = new SelectList(_context.Stops, "Id", "StopName", stopTime.StopId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "GtfsTripId", stopTime.TripId);
            return View(stopTime);
        }

        // GET: StopTimes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stopTime = await _context.StopTimes
                .Include(s => s.Stop)
                .Include(s => s.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stopTime == null)
            {
                return NotFound();
            }

            return View(stopTime);
        }

        // POST: StopTimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stopTime = await _context.StopTimes.FindAsync(id);
            if (stopTime != null)
            {
                _context.StopTimes.Remove(stopTime);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StopTimeExists(int id)
        {
            return _context.StopTimes.Any(e => e.Id == id);
        }

        // POST: StopTimes/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var stopTimesToDelete = await _context.StopTimes
                .Where(st => ids.Contains(st.Id))
                .ToListAsync();

            if (stopTimesToDelete.Any())
            {
                _context.StopTimes.RemoveRange(stopTimesToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {stopTimesToDelete.Count} stop times.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

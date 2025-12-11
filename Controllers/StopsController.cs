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
    public class StopsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StopsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stops
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Stops.Include(s => s.ParentStation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Stops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stop = await _context.Stops
                .Include(s => s.ParentStation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stop == null)
            {
                return NotFound();
            }

            return View(stop);
        }

        // GET: Stops/Create
        public IActionResult Create()
        {
            ViewData["ParentStationId"] = new SelectList(_context.Stops, "Id", "StopName");
            return View();
        }

        // POST: Stops/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsStopId,StopCode,StopName,StopDesc,StopLat,StopLon,ZoneId,StopUrl,LocationType,WheelchairBoarding,ParentStationId,StopTimezone")] Stop stop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentStationId"] = new SelectList(_context.Stops, "Id", "StopName", stop.ParentStationId);
            return View(stop);
        }

        // GET: Stops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stop = await _context.Stops.FindAsync(id);
            if (stop == null)
            {
                return NotFound();
            }
            ViewData["ParentStationId"] = new SelectList(_context.Stops, "Id", "StopName", stop.ParentStationId);
            return View(stop);
        }

        // POST: Stops/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsStopId,StopCode,StopName,StopDesc,StopLat,StopLon,ZoneId,StopUrl,LocationType,WheelchairBoarding,ParentStationId,StopTimezone")] Stop stop)
        {
            if (id != stop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StopExists(stop.Id))
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
            ViewData["ParentStationId"] = new SelectList(_context.Stops, "Id", "StopName", stop.ParentStationId);
            return View(stop);
        }

        // GET: Stops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stop = await _context.Stops
                .Include(s => s.ParentStation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stop == null)
            {
                return NotFound();
            }

            return View(stop);
        }

        // POST: Stops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stop = await _context.Stops.FindAsync(id);
            if (stop != null)
            {
                _context.Stops.Remove(stop);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    private bool StopExists(int id)
        {
            return _context.Stops.Any(e => e.Id == id);
        }

        // POST: Stops/DeleteInvalid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvalid()
        {
            var invalidStops = await _context.Stops
                .Where(s => s.StopLat == 0m && s.StopLon == 0m)
                .ToListAsync();

            if (invalidStops.Any())
            {
                _context.Stops.RemoveRange(invalidStops);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {invalidStops.Count} stops with invalid coordinates.";
            }
            else
            {
                TempData["InfoMessage"] = "No stops found with invalid coordinates (0, 0).";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Stops/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var stopsToDelete = await _context.Stops
                .Where(s => ids.Contains(s.Id))
                .ToListAsync();

            if (stopsToDelete.Any())
            {
                _context.Stops.RemoveRange(stopsToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {stopsToDelete.Count} stops.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

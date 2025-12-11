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
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trips
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Trips
                .Include(t => t.ShapesMaster)
                .Include(t => t.TransitCalendar)
                .Include(t => t.TransitRoute);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Trips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.ShapesMaster)
                .Include(t => t.TransitCalendar)
                .Include(t => t.TransitRoute)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // GET: Trips/Create
        public IActionResult Create()
        {
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId");
            ViewData["ServiceId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId");
            ViewData["TransitRouteId"] = new SelectList(_context.TransitRoutes, "Id", "GtfsRouteId");
            return View();
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsTripId,TransitRouteId,ServiceId,ShapeId,TripHeadsign,TripShortName,DirectionId,WheelchairAccessible,BlockId")] Trip trip)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId", trip.ShapeId);
            ViewData["ServiceId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId", trip.ServiceId);
            ViewData["TransitRouteId"] = new SelectList(_context.TransitRoutes, "Id", "GtfsRouteId", trip.TransitRouteId);
            return View(trip);
        }

        // GET: Trips/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId", trip.ShapeId);
            ViewData["ServiceId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId", trip.ServiceId);
            ViewData["TransitRouteId"] = new SelectList(_context.TransitRoutes, "Id", "GtfsRouteId", trip.TransitRouteId);
            return View(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsTripId,TransitRouteId,ServiceId,ShapeId,TripHeadsign,TripShortName,DirectionId,WheelchairAccessible,BlockId")] Trip trip)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TripExists(trip.Id))
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
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId", trip.ShapeId);
            ViewData["ServiceId"] = new SelectList(_context.TransitCalendars, "Id", "GtfsServiceId", trip.ServiceId);
            ViewData["TransitRouteId"] = new SelectList(_context.TransitRoutes, "Id", "GtfsRouteId", trip.TransitRouteId);
            return View(trip);
        }

        // GET: Trips/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.ShapesMaster)
                .Include(t => t.TransitCalendar)
                .Include(t => t.TransitRoute)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }

        // POST: Trips/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var tripsToDelete = await _context.Trips
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();

            if (tripsToDelete.Any())
            {
                _context.Trips.RemoveRange(tripsToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {tripsToDelete.Count} trips.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

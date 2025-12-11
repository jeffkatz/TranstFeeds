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
    public class TransitRoutesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransitRoutesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransitRoutes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TransitRoutes.Include(t => t.Agency);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TransitRoutes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transitRoute = await _context.TransitRoutes
                .Include(t => t.Agency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transitRoute == null)
            {
                return NotFound();
            }

            return View(transitRoute);
        }

        // GET: TransitRoutes/Create
        public IActionResult Create()
        {
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "AgencyName");
            return View();
        }

        // POST: TransitRoutes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsRouteId,AgencyId,RouteShortName,RouteLongName,RouteType,RouteTextColor,RouteColor,RouteUrl,RouteDesc")] TransitRoute transitRoute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transitRoute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "AgencyName", transitRoute.AgencyId);
            return View(transitRoute);
        }

        // GET: TransitRoutes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transitRoute = await _context.TransitRoutes.FindAsync(id);
            if (transitRoute == null)
            {
                return NotFound();
            }
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "AgencyName", transitRoute.AgencyId);
            return View(transitRoute);
        }

        // POST: TransitRoutes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsRouteId,AgencyId,RouteShortName,RouteLongName,RouteType,RouteTextColor,RouteColor,RouteUrl,RouteDesc")] TransitRoute transitRoute)
        {
            if (id != transitRoute.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transitRoute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransitRouteExists(transitRoute.Id))
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
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "AgencyName", transitRoute.AgencyId);
            return View(transitRoute);
        }

        // GET: TransitRoutes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transitRoute = await _context.TransitRoutes
                .Include(t => t.Agency)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transitRoute == null)
            {
                return NotFound();
            }

            return View(transitRoute);
        }

        // POST: TransitRoutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transitRoute = await _context.TransitRoutes.FindAsync(id);
            if (transitRoute != null)
            {
                _context.TransitRoutes.Remove(transitRoute);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TransitRouteExists(int id)
        {
            return _context.TransitRoutes.Any(e => e.Id == id);
        }

        // POST: TransitRoutes/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var routesToDelete = await _context.TransitRoutes
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            if (routesToDelete.Any())
            {
                _context.TransitRoutes.RemoveRange(routesToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {routesToDelete.Count} routes.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

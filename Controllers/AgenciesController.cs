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
    public class AgenciesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgenciesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Agencies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Agencies.ToListAsync());
        }

        // GET: Agencies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agency = await _context.Agencies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (agency == null)
            {
                return NotFound();
            }

            return View(agency);
        }

        // GET: Agencies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Agencies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsAgencyId,AgencyName,AgencyUrl,AgencyTimezone,AgencyPhone,AgencyLang")] Agency agency)
        {
            if (ModelState.IsValid)
            {
                _context.Add(agency);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(agency);
        }

        // GET: Agencies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agency = await _context.Agencies.FindAsync(id);
            if (agency == null)
            {
                return NotFound();
            }
            return View(agency);
        }

        // POST: Agencies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsAgencyId,AgencyName,AgencyUrl,AgencyTimezone,AgencyPhone,AgencyLang")] Agency agency)
        {
            if (id != agency.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(agency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgencyExists(agency.Id))
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
            return View(agency);
        }

        // GET: Agencies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agency = await _context.Agencies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (agency == null)
            {
                return NotFound();
            }

            return View(agency);
        }

        // POST: Agencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var agency = await _context.Agencies.FindAsync(id);
            if (agency != null)
            {
                _context.Agencies.Remove(agency);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AgencyExists(int id)
        {
            return _context.Agencies.Any(e => e.Id == id);
        }

        // POST: Agencies/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var agenciesToDelete = await _context.Agencies
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            if (agenciesToDelete.Any())
            {
                _context.Agencies.RemoveRange(agenciesToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {agenciesToDelete.Count} agencies.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

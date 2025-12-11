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
    public class ShapesMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShapesMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ShapesMaster
        public async Task<IActionResult> Index()
        {
            return View(await _context.ShapesMasters.ToListAsync());
        }

        // GET: ShapesMaster/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shapesMaster = await _context.ShapesMasters
                .Include(s => s.Shapes)
                .Include(s => s.Trips)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shapesMaster == null)
            {
                return NotFound();
            }

            return View(shapesMaster);
        }

        // GET: ShapesMaster/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ShapesMaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GtfsShapeId")] ShapesMaster shapesMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shapesMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shapesMaster);
        }

        // GET: ShapesMaster/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shapesMaster = await _context.ShapesMasters.FindAsync(id);
            if (shapesMaster == null)
            {
                return NotFound();
            }
            return View(shapesMaster);
        }

        // POST: ShapesMaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GtfsShapeId")] ShapesMaster shapesMaster)
        {
            if (id != shapesMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shapesMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShapesMasterExists(shapesMaster.Id))
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
            return View(shapesMaster);
        }

        // GET: ShapesMaster/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shapesMaster = await _context.ShapesMasters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shapesMaster == null)
            {
                return NotFound();
            }

            return View(shapesMaster);
        }

        // POST: ShapesMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shapesMaster = await _context.ShapesMasters.FindAsync(id);
            if (shapesMaster != null)
            {
                _context.ShapesMasters.Remove(shapesMaster);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ShapesMasterExists(int id)
        {
            return _context.ShapesMasters.Any(e => e.Id == id);
        }

        // POST: ShapesMaster/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var shapeMastersToDelete = await _context.ShapesMasters
                .Where(sm => ids.Contains(sm.Id))
                .ToListAsync();

            if (shapeMastersToDelete.Any())
            {
                _context.ShapesMasters.RemoveRange(shapeMastersToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {shapeMastersToDelete.Count} shape masters.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

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
    public class ShapesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShapesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shapes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Shapes.Include(s => s.ShapesMaster);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Shapes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shape = await _context.Shapes
                .Include(s => s.ShapesMaster)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shape == null)
            {
                return NotFound();
            }

            return View(shape);
        }

        // GET: Shapes/Create
        public IActionResult Create()
        {
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId");
            return View();
        }

        // POST: Shapes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ShapeId,ShapePtSequence,ShapePtLat,ShapePtLon,ShapeDistTraveled")] Shape shape)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shape);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId", shape.ShapeId);
            return View(shape);
        }

        // GET: Shapes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shape = await _context.Shapes.FindAsync(id);
            if (shape == null)
            {
                return NotFound();
            }
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId", shape.ShapeId);
            return View(shape);
        }

        // POST: Shapes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ShapeId,ShapePtSequence,ShapePtLat,ShapePtLon,ShapeDistTraveled")] Shape shape)
        {
            if (id != shape.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shape);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShapeExists(shape.Id))
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
            ViewData["ShapeId"] = new SelectList(_context.ShapesMasters, "Id", "GtfsShapeId", shape.ShapeId);
            return View(shape);
        }

        // GET: Shapes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shape = await _context.Shapes
                .Include(s => s.ShapesMaster)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shape == null)
            {
                return NotFound();
            }

            return View(shape);
        }

        // POST: Shapes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shape = await _context.Shapes.FindAsync(id);
            if (shape != null)
            {
                _context.Shapes.Remove(shape);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ShapeExists(int id)
        {
            return _context.Shapes.Any(e => e.Id == id);
        }

        // POST: Shapes/DeleteInvalid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvalid()
        {
            var invalidShapes = await _context.Shapes
                .Where(s => s.ShapePtLat == 0m && s.ShapePtLon == 0m)
                .ToListAsync();

            if (invalidShapes.Any())
            {
                _context.Shapes.RemoveRange(invalidShapes);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {invalidShapes.Count} shape points with invalid coordinates.";
            }
            else
            {
                TempData["InfoMessage"] = "No shape points found with invalid coordinates (0, 0).";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Shapes/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var shapesToDelete = await _context.Shapes
                .Where(s => ids.Contains(s.Id))
                .ToListAsync();

            if (shapesToDelete.Any())
            {
                _context.Shapes.RemoveRange(shapesToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {shapesToDelete.Count} shape points.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

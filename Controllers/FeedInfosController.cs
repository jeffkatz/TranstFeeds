using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using TransitFeeds.Models;

namespace TransitFeeds.Controllers
{
    public class FeedInfosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedInfosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FeedInfos
        public async Task<IActionResult> Index()
        {
            return View(await _context.FeedInfos.ToListAsync());
        }

        // GET: FeedInfos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var feedInfo = await _context.FeedInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedInfo == null) return NotFound();

            return View(feedInfo);
        }

        // GET: FeedInfos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FeedInfos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FeedPublisherName,FeedPublisherUrl,FeedLang,FeedStartDate,FeedEndDate,FeedVersion,FeedContactEmail,FeedContactUrl")] FeedInfo feedInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(feedInfo);
        }

        // GET: FeedInfos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var feedInfo = await _context.FeedInfos.FindAsync(id);
            if (feedInfo == null) return NotFound();
            return View(feedInfo);
        }

        // POST: FeedInfos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FeedPublisherName,FeedPublisherUrl,FeedLang,FeedStartDate,FeedEndDate,FeedVersion,FeedContactEmail,FeedContactUrl")] FeedInfo feedInfo)
        {
            if (id != feedInfo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedInfoExists(feedInfo.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(feedInfo);
        }

        // GET: FeedInfos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var feedInfo = await _context.FeedInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedInfo == null) return NotFound();

            return View(feedInfo);
        }

        // POST: FeedInfos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedInfo = await _context.FeedInfos.FindAsync(id);
            if (feedInfo != null)
            {
                _context.FeedInfos.Remove(feedInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedInfoExists(int id)
        {
            return _context.FeedInfos.Any(e => e.Id == id);
        }
    }
}

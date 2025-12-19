using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Services;
using TransitFeeds.Data;
using TransitFeeds.Models;

namespace TransitFeeds.Controllers
{
    public class ImportController : Controller
    {
        private readonly GtfsImporter _importer;
        private readonly GtfsExporterService _exporter;
        private readonly GtfsComplianceService _compliance;
        private readonly ApplicationDbContext _context;

        public ImportController(GtfsImporter importer, GtfsExporterService exporter, GtfsComplianceService compliance, ApplicationDbContext context)
        {
            _importer = importer;
            _exporter = exporter;
            _compliance = compliance;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadZip(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            if (!Path.GetExtension(file.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Please upload a valid .zip file.");

            // Create a temporary folder
            var tempFolder = Path.Combine(Path.GetTempPath(), "GtfsImport_" + Guid.NewGuid());
            Directory.CreateDirectory(tempFolder);

            var tempZipPath = Path.Combine(tempFolder, "upload.zip");

            try
            {
                // Save ZIP
                using (var stream = new FileStream(tempZipPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract
                ZipFile.ExtractToDirectory(tempZipPath, tempFolder);

                // Run Import
                await _importer.ImportGtfsAsync(tempFolder);
                
                TempData["Success"] = "GTFS Feed imported successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Import failed: {ex.Message}";
                // Log exception in real app
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearDatabase()
        {
            try
            {
                // Materialize lists to ensure RemoveRange works as expected on tracked entities
                
                // 1. Child tables
                _context.StopTimes.RemoveRange(_context.StopTimes.ToList());
                _context.Shapes.RemoveRange(_context.Shapes.ToList());
                await _context.SaveChangesAsync();

                // 2. Trips
                _context.Trips.RemoveRange(_context.Trips.ToList());
                await _context.SaveChangesAsync();

                // 3. Middle Tier
                _context.TransitRoutes.RemoveRange(_context.TransitRoutes.ToList());
                _context.TransitCalendars.RemoveRange(_context.TransitCalendars.ToList());
                _context.ShapesMasters.RemoveRange(_context.ShapesMasters.ToList());
                
                // 4. Stops
                await _context.Database.ExecuteSqlRawAsync("UPDATE Stops SET parent_station = NULL");
                _context.Stops.RemoveRange(_context.Stops.ToList());

                // 5. Agencies
                _context.Agencies.RemoveRange(_context.Agencies.ToList());

                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Database cleared and reset successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to clear database: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportZip()
        {
            var zipBytes = await _exporter.ExportGtfsToZipAsync();
            return File(zipBytes, "application/zip", "gtfs_export.zip");
        }

        [HttpGet]
        public async Task<IActionResult> Audit()
        {
            var reports = new List<string>();
            
            // 1. Check Routes for missing names
            var routes = await _context.TransitRoutes.ToListAsync();
            foreach(var r in routes)
            {
                var errors = _compliance.ValidateRoute(r);
                if (errors.Any()) reports.AddRange(errors.Select(e => $"Route {r.GtfsRouteId}: {e}"));
            }

            // 2. Check for missing Feed Info
            if (!await _context.FeedInfos.AnyAsync()) reports.Add("Warning: feed_info.txt is missing from dataset.");

            // 3. Count Entities
            reports.Add($"Agencies: {await _context.Agencies.CountAsync()}");
            reports.Add($"Routes: {await _context.TransitRoutes.CountAsync()}");
            reports.Add($"Stops: {await _context.Stops.CountAsync()}");
            reports.Add($"Trips: {await _context.Trips.CountAsync()}");
            reports.Add($"Calendar Rules: {await _context.TransitCalendars.CountAsync()}");
            reports.Add($"Calendar Exceptions: {await _context.CalendarDates.CountAsync()}");
            
            ViewBag.Reports = reports;
            return View();
        }
    }
}

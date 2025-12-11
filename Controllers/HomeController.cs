using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using TransitFeeds.Models;

namespace TransitFeeds.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get statistics for dashboard
            ViewBag.AgenciesCount = await _context.Agencies.CountAsync();
            ViewBag.RoutesCount = await _context.TransitRoutes.CountAsync();
            ViewBag.StopsCount = await _context.Stops.CountAsync();
            ViewBag.TripsCount = await _context.Trips.CountAsync();
            ViewBag.CalendarCount = await _context.TransitCalendars.CountAsync();
            ViewBag.StopTimesCount = await _context.StopTimes.CountAsync();
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

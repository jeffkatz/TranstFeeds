using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using TransitFeeds.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TransitFeeds.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Map/GetStopsInBounds?minLat=...
        [HttpGet]
        public async Task<IActionResult> GetStopsInBounds(decimal minLat, decimal maxLat, decimal minLon, decimal maxLon)
        {
            // Limit query to viewport
            var stops = await _context.Stops
                .Where(s => s.StopLat >= minLat && s.StopLat <= maxLat &&
                            s.StopLon >= minLon && s.StopLon <= maxLon)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.StopName,
                    lat = s.StopLat,
                    lon = s.StopLon,
                    gtfsId = s.GtfsStopId
                })
                .Take(2000) // Safety cap
                .ToListAsync();

            return Json(stops);
        }

        // GET: /Map/GetRouteList
        [HttpGet]
        public async Task<IActionResult> GetRouteList()
        {
            var routes = await _context.TransitRoutes
                .AsNoTracking()
                .Select(r => new
                {
                    id = r.Id,
                    shortName = r.RouteShortName,
                    longName = r.RouteLongName,
                    color = r.RouteColor,
                    textColor = r.RouteTextColor
                })
                .OrderBy(r => r.shortName)
                .ToListAsync();

            return Json(routes);
        }

        // GET: /Map/GetRouteShapes?routeId=...
        [HttpGet]
        public async Task<IActionResult> GetRouteShapes(int routeId)
        {
            // 1. Get Trips for this route
            // 2. Get Distinct Shape IDs used by these trips
            // 3. Get Points for those shapes

            // This approach avoids loading useless shapes not used by the route.
            var shapeIds = await _context.Trips
                .Where(t => t.TransitRouteId == routeId && t.ShapeId != null)
                .Select(t => t.ShapeId.Value) // Select Value strictly
                .Distinct()
                .ToListAsync();

            if (!shapeIds.Any()) return Json(new List<object>());

            var shapes = await _context.Shapes
                .Where(s => shapeIds.Contains(s.ShapeId))
                .OrderBy(s => s.ShapeId)
                .ThenBy(s => s.ShapePtSequence)
                .Select(s => new
                {
                    shapeId = s.ShapeId,
                    lat = s.ShapePtLat,
                    lon = s.ShapePtLon
                })
                .ToListAsync();

            return Json(shapes);
        }
    }
}

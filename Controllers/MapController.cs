using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TransitFeeds.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Map
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Map/GetStops - returns all stops as JSON for map plotting
        [HttpGet]
        public async Task<IActionResult> GetStops()
        {
            var stops = await _context.Stops
                .Select(s => new
                {
                    id = s.Id,
                    name = s.StopName,
                    lat = s.StopLat,
                    lon = s.StopLon,
                    gtfsId = s.GtfsStopId
                })
                .ToListAsync();

            return Json(stops);
        }

        // GET: /Map/GetRoutes - returns route shapes as JSON
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _context.TransitRoutes
                .Include(r => r.Agency)
                .Select(r => new
                {
                    id = r.Id,
                    name = r.RouteLongName,
                    shortName = r.RouteShortName,
                    color = r.RouteColor,
                    agencyName = r.Agency != null ? r.Agency.AgencyName : ""
                })
                .ToListAsync();

            return Json(routes);
        }

        // GET: /Map/GetShapes - returns shape points for drawing route paths
        [HttpGet]
        public async Task<IActionResult> GetShapes()
        {
            var shapes = await _context.Shapes
                .OrderBy(s => s.ShapeId)
                .ThenBy(s => s.ShapePtSequence)
                .Select(s => new
                {
                    shapeId = s.ShapeId,
                    lat = s.ShapePtLat,
                    lon = s.ShapePtLon,
                    sequence = s.ShapePtSequence
                })
                .ToListAsync();

            return Json(shapes);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using TransitFeeds.Data;
using TransitFeeds.Models;
using Microsoft.EntityFrameworkCore;

namespace TransitFeeds.Controllers
{
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Import
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Import/Stops
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadStops(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            using var reader = new StreamReader(file.OpenReadStream());
            var header = await reader.ReadLineAsync(); // skip header
            var stops = new List<Stop>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                // Expected columns: gtfs_stop_id,stop_name,stop_lat,stop_lon,location_type
                if (parts.Length < 5) continue;
                var stop = new Stop
                {
                    GtfsStopId = parts[0].Trim(),
                    StopName = parts[1].Trim(),
                    StopLat = decimal.TryParse(parts[2], out var lat) ? lat : 0m,
                    StopLon = decimal.TryParse(parts[3], out var lon) ? lon : 0m,
                    LocationType = (byte?)(int.TryParse(parts[4], out var loc) ? loc : 0)
                };
                stops.Add(stop);
            }
            _context.Stops.AddRange(stops);
            await _context.SaveChangesAsync();
            return Content($"Uploaded {stops.Count} stops.");
        }

        // POST: /Import/Routes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadRoutes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            using var reader = new StreamReader(file.OpenReadStream());
            var header = await reader.ReadLineAsync(); // skip header
            var routes = new List<TransitRoute>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                // Expected columns: gtfs_route_id,agency_id,route_short_name,route_long_name,route_type,route_color,route_text_color
                if (parts.Length < 7) continue;
                var agencyGtfsId = parts[1].Trim();
                var agency = _context.Agencies.FirstOrDefault(a => a.GtfsAgencyId == agencyGtfsId);
                if (agency == null) continue; // skip if agency not found
                var route = new TransitRoute
                {
                    GtfsRouteId = parts[0].Trim(),
                    AgencyId = agency.Id,
                    RouteShortName = parts[2].Trim(),
                    RouteLongName = parts[3].Trim(),
                    RouteType = int.TryParse(parts[4], out var rt) ? rt : 0,
                    RouteColor = parts[5].Trim(),
                    RouteTextColor = parts[6].Trim()
                };
                routes.Add(route);
            }
            _context.TransitRoutes.AddRange(routes);
            await _context.SaveChangesAsync();
            return Content($"Uploaded {routes.Count} routes.");
        }

        // POST: /Import/Trips
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadTrips(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            using var reader = new StreamReader(file.OpenReadStream());
            var header = await reader.ReadLineAsync(); // skip header
            var trips = new List<Trip>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                // Expected columns: route_id,service_id,trip_id,trip_headsign,trip_short_name,direction_id,block_id,shape_id,wheelchair_accessible
                if (parts.Length < 9) continue;
                var routeGtfsId = parts[0].Trim();
                var serviceGtfsId = parts[1].Trim();
                var route = _context.TransitRoutes.FirstOrDefault(r => r.GtfsRouteId == routeGtfsId);
                var service = _context.TransitCalendars.FirstOrDefault(s => s.GtfsServiceId == serviceGtfsId);
                if (route == null || service == null) continue;
                var trip = new Trip
                {
                    GtfsTripId = parts[2].Trim(),
                    TransitRouteId = route.Id,
                    ServiceId = service.Id,
                    TripHeadsign = parts[3].Trim(),
                    TripShortName = parts[4].Trim(),
                    DirectionId = byte.TryParse(parts[5], out var dir) ? (byte?)dir : null,
                    BlockId = parts[6].Trim(),
                    ShapeId = int.TryParse(parts[7], out var shapeId) ? (int?)shapeId : null,
                    WheelchairAccessible = byte.TryParse(parts[8], out var wc) ? (byte?)wc : null
                };
                trips.Add(trip);
            }
            _context.Trips.AddRange(trips);
            await _context.SaveChangesAsync();
            return Content($"Uploaded {trips.Count} trips.");
        }
        // GET: /Import/ExportStops
        [HttpGet]
        public async Task<IActionResult> ExportStops()
        {
            var stops = await _context.Stops.ToListAsync();
            var builder = new StringBuilder();
            builder.AppendLine("gtfs_stop_id,stop_name,stop_lat,stop_lon,location_type");
            
            foreach (var stop in stops)
            {
                builder.AppendLine($"{stop.GtfsStopId},{stop.StopName},{stop.StopLat},{stop.StopLon},{stop.LocationType}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "stops.txt");
        }

        // GET: /Import/ExportRoutes
        [HttpGet]
        public async Task<IActionResult> ExportRoutes()
        {
            var routes = await _context.TransitRoutes.Include(r => r.Agency).ToListAsync();
            var builder = new StringBuilder();
            builder.AppendLine("gtfs_route_id,agency_id,route_short_name,route_long_name,route_type,route_color,route_text_color");

            foreach (var route in routes)
            {
                var agencyId = route.Agency?.GtfsAgencyId ?? "";
                builder.AppendLine($"{route.GtfsRouteId},{agencyId},{route.RouteShortName},{route.RouteLongName},{route.RouteType},{route.RouteColor},{route.RouteTextColor}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "routes.txt");
        }

        // GET: /Import/ExportTrips
        [HttpGet]
        public async Task<IActionResult> ExportTrips()
        {
            var trips = await _context.Trips
                .Include(t => t.TransitRoute)
                .Include(t => t.TransitCalendar)
                .ToListAsync();
            
            var builder = new StringBuilder();
            builder.AppendLine("route_id,service_id,trip_id,trip_headsign,trip_short_name,direction_id,block_id,shape_id,wheelchair_accessible");

            foreach (var trip in trips)
            {
                var routeId = trip.TransitRoute?.GtfsRouteId ?? "";
                var serviceId = trip.TransitCalendar?.GtfsServiceId ?? "";
                builder.AppendLine($"{routeId},{serviceId},{trip.GtfsTripId},{trip.TripHeadsign},{trip.TripShortName},{trip.DirectionId},{trip.BlockId},{trip.ShapeId},{trip.WheelchairAccessible}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "trips.txt");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using TransitFeeds.Models;

namespace TransitFeeds.Services
{
    public class GtfsImporter
    {
        private readonly ApplicationDbContext _context;

        public GtfsImporter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ImportGtfsAsync(string gtfsDirectoryPath)
        {
            // Maps to store GtfsId -> InternalId
            var agencyMap = new Dictionary<string, int>();
            var calendarMap = new Dictionary<string, int>();
            var shapeMap = new Dictionary<string, int>();
            var stopMap = new Dictionary<string, int>();
            var routeMap = new Dictionary<string, int>();
            var tripMap = new Dictionary<string, int>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace("_", "").Replace(" ", "")
            };

            // 1. Agencies
            await ImportAgencies(gtfsDirectoryPath, agencyMap, config);

            // 2. Calendars
            await ImportCalendars(gtfsDirectoryPath, calendarMap, config);

            // 3. Shapes (Master & Points)
            // Note: We need to populate ShapesMaster first, then the points
            await ImportShapes(gtfsDirectoryPath, shapeMap, config);

            // 4. Stops
            await ImportStops(gtfsDirectoryPath, stopMap, config);

            // 5. Routes
            await ImportRoutes(gtfsDirectoryPath, agencyMap, routeMap, config);

            // 6. Trips
            await ImportTrips(gtfsDirectoryPath, routeMap, calendarMap, shapeMap, tripMap, config);

            // 7. StopTimes
            await ImportStopTimes(gtfsDirectoryPath, tripMap, stopMap, config);
        }

        private async Task ImportAgencies(string path, Dictionary<string, int> map, CsvConfiguration config)
        {
            var file = Path.Combine(path, "agency.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);

            // Read as dynamic to handle flexible columns manually or map to a DTO
            var records = csv.GetRecords<dynamic>();

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var gtfsId = GetValue(dict, "agency_id"); // May be null if only 1 agency
                
                var agency = new Agency
                {
                    GtfsAgencyId = gtfsId,
                    AgencyName = GetValue(dict, "agency_name") ?? "Unknown",
                    AgencyUrl = GetValue(dict, "agency_url") ?? "",
                    AgencyTimezone = GetValue(dict, "agency_timezone") ?? "",
                    AgencyPhone = GetValue(dict, "agency_phone"),
                    AgencyLang = GetValue(dict, "agency_lang")
                };

                _context.Agencies.Add(agency);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(agency.GtfsAgencyId))
                {
                    map[agency.GtfsAgencyId] = agency.Id;
                }
                else
                {
                    // Fallback for single agency feeds without ID
                    if (!map.ContainsKey("DEFAULT")) map["DEFAULT"] = agency.Id;
                }
            }
        }

        private async Task ImportCalendars(string path, Dictionary<string, int> map, CsvConfiguration config)
        {
            var file = Path.Combine(path, "calendar.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<dynamic>();

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var serviceId = GetValue(dict, "service_id");
                if (string.IsNullOrEmpty(serviceId)) continue;

                var cal = new TransitCalendar
                {
                    GtfsServiceId = serviceId,
                    StartDate = ParseDate(GetValue(dict, "start_date")),
                    EndDate = ParseDate(GetValue(dict, "end_date")),
                    Monday = GetValue(dict, "monday") == "1",
                    Tuesday = GetValue(dict, "tuesday") == "1",
                    Wednesday = GetValue(dict, "wednesday") == "1",
                    Thursday = GetValue(dict, "thursday") == "1",
                    Friday = GetValue(dict, "friday") == "1",
                    Saturday = GetValue(dict, "saturday") == "1",
                    Sunday = GetValue(dict, "sunday") == "1"
                };

                _context.TransitCalendars.Add(cal);
                await _context.SaveChangesAsync();
                map[serviceId] = cal.Id;
            }
        }

        private async Task ImportShapes(string path, Dictionary<string, int> map, CsvConfiguration config)
        {
            var file = Path.Combine(path, "shapes.txt");
            if (!File.Exists(file)) return;

            // We need to read the file twice or stream carefully:
            // 1. To get distinct shape_ids and create Master records
            // 2. To add the points

            // Pass 1: Master Records
            var distinctShapeIds = new HashSet<string>();
            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var r in records)
                {
                    var dict = (IDictionary<string, object>)r;
                    var sid = GetValue(dict, "shape_id");
                    if (!string.IsNullOrEmpty(sid)) distinctShapeIds.Add(sid);
                }
            }

            foreach (var shapeId in distinctShapeIds)
            {
                var master = new ShapesMaster { GtfsShapeId = shapeId };
                _context.ShapesMasters.Add(master);
                await _context.SaveChangesAsync();
                map[shapeId] = master.Id;
            }

            // Pass 2: Points
            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>();
                var batch = new List<Shape>();
                int count = 0;

                foreach (var r in records)
                {
                    var dict = (IDictionary<string, object>)r;
                    var sid = GetValue(dict, "shape_id");
                    
                    if (string.IsNullOrEmpty(sid) || !map.ContainsKey(sid)) continue;

                    var shape = new Shape
                    {
                        ShapeId = map[sid],
                        ShapePtSequence = ParseInt(GetValue(dict, "shape_pt_sequence")) ?? 0,
                        ShapePtLat = ParseDecimal(GetValue(dict, "shape_pt_lat")),
                        ShapePtLon = ParseDecimal(GetValue(dict, "shape_pt_lon")),
                        ShapeDistTraveled = ParseDecimal(GetValue(dict, "shape_dist_traveled"))
                    };

                    batch.Add(shape);
                    count++;

                    if (count >= 1000)
                    {
                        _context.Shapes.AddRange(batch);
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                        batch.Clear();
                        count = 0;
                    }
                }
                if (batch.Any())
                {
                    _context.Shapes.AddRange(batch);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                }
            }
        }

        private async Task ImportStops(string path, Dictionary<string, int> map, CsvConfiguration config)
        {
            var file = Path.Combine(path, "stops.txt");
            if (!File.Exists(file)) return;

            var stopsToUpdateParent = new List<(int InternalId, string ParentGtfsId)>();

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var r in records)
                {
                    var dict = (IDictionary<string, object>)r;
                    var gtfsId = GetValue(dict, "stop_id");
                    if (string.IsNullOrEmpty(gtfsId)) continue;

                    var stop = new Stop
                    {
                        GtfsStopId = gtfsId,
                        StopCode = GetValue(dict, "stop_code"),
                        StopName = GetValue(dict, "stop_name") ?? "Unknown",
                        StopDesc = GetValue(dict, "stop_desc"),
                        StopLat = ParseDecimal(GetValue(dict, "stop_lat")),
                        StopLon = ParseDecimal(GetValue(dict, "stop_lon")),
                        ZoneId = GetValue(dict, "zone_id"),
                        StopUrl = GetValue(dict, "stop_url"),
                        LocationType = ParseByte(GetValue(dict, "location_type")),
                        WheelchairBoarding = ParseByte(GetValue(dict, "wheelchair_boarding")),
                        StopTimezone = GetValue(dict, "stop_timezone")
                    };

                    _context.Stops.Add(stop);
                    await _context.SaveChangesAsync();
                    map[gtfsId] = stop.Id;

                    var parentId = GetValue(dict, "parent_station");
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        stopsToUpdateParent.Add((stop.Id, parentId));
                    }
                }
            }

            // Update Parent Stations
            foreach (var item in stopsToUpdateParent)
            {
                if (map.TryGetValue(item.ParentGtfsId, out int parentInternalId))
                {
                    var stop = new Stop { Id = item.InternalId, ParentStationId = parentInternalId };
                    _context.Stops.Attach(stop);
                    _context.Entry(stop).Property(x => x.ParentStationId).IsModified = true;
                }
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        private async Task ImportRoutes(string path, Dictionary<string, int> agencyMap, Dictionary<string, int> routeMap, CsvConfiguration config)
        {
            var file = Path.Combine(path, "routes.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var gtfsId = GetValue(dict, "route_id");
                if (string.IsNullOrEmpty(gtfsId)) continue;

                var agencyGtfsId = GetValue(dict, "agency_id");
                int? agencyId = null;

                if (!string.IsNullOrEmpty(agencyGtfsId) && agencyMap.ContainsKey(agencyGtfsId))
                {
                    agencyId = agencyMap[agencyGtfsId];
                }
                else if (agencyMap.Count == 1) // Fallback for 1 agency
                {
                    agencyId = agencyMap.Values.First();
                }
                else if (agencyMap.ContainsKey("DEFAULT"))
                {
                    agencyId = agencyMap["DEFAULT"];
                }

                var route = new TransitRoute
                {
                    GtfsRouteId = gtfsId,
                    AgencyId = agencyId,
                    RouteShortName = GetValue(dict, "route_short_name"),
                    RouteLongName = GetValue(dict, "route_long_name"),
                    RouteType = ParseInt(GetValue(dict, "route_type")),
                    RouteTextColor = GetValue(dict, "route_text_color"),
                    RouteColor = GetValue(dict, "route_color"),
                    RouteUrl = GetValue(dict, "route_url"),
                    RouteDesc = GetValue(dict, "route_desc")
                };

                _context.TransitRoutes.Add(route);
                await _context.SaveChangesAsync();
                routeMap[gtfsId] = route.Id;
            }
        }

        private async Task ImportTrips(string path, Dictionary<string, int> routeMap, Dictionary<string, int> serviceMap, Dictionary<string, int> shapeMap, Dictionary<string, int> tripMap, CsvConfiguration config)
        {
            var file = Path.Combine(path, "trips.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var gtfsId = GetValue(dict, "trip_id");
                if (string.IsNullOrEmpty(gtfsId)) continue;

                var routeId = GetValue(dict, "route_id");
                var serviceId = GetValue(dict, "service_id");
                var shapeId = GetValue(dict, "shape_id");

                if (!routeMap.ContainsKey(routeId) || !serviceMap.ContainsKey(serviceId)) continue;

                var trip = new Trip
                {
                    GtfsTripId = gtfsId,
                    TransitRouteId = routeMap[routeId],
                    ServiceId = serviceMap[serviceId],
                    ShapeId = !string.IsNullOrEmpty(shapeId) && shapeMap.ContainsKey(shapeId) ? shapeMap[shapeId] : (int?)null,
                    TripHeadsign = GetValue(dict, "trip_headsign"),
                    TripShortName = GetValue(dict, "trip_short_name"),
                    DirectionId = ParseByte(GetValue(dict, "direction_id")),
                    WheelchairAccessible = ParseByte(GetValue(dict, "wheelchair_accessible")),
                    BlockId = GetValue(dict, "block_id")
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                tripMap[gtfsId] = trip.Id;
            }
        }

        private async Task ImportStopTimes(string path, Dictionary<string, int> tripMap, Dictionary<string, int> stopMap, CsvConfiguration config)
        {
            var file = Path.Combine(path, "stop_times.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<dynamic>();
            var batch = new List<StopTime>();
            int count = 0;

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var tripId = GetValue(dict, "trip_id");
                var stopId = GetValue(dict, "stop_id");

                if (!tripMap.ContainsKey(tripId) || !stopMap.ContainsKey(stopId)) continue;

                var stopTime = new StopTime
                {
                    TripId = tripMap[tripId],
                    StopId = stopMap[stopId],
                    StopSequence = ParseInt(GetValue(dict, "stop_sequence")) ?? 0,
                    ArrivalTime = ParseTime(GetValue(dict, "arrival_time")),
                    DepartureTime = ParseTime(GetValue(dict, "departure_time")),
                    StopHeadsign = GetValue(dict, "stop_headsign"),
                    PickupType = ParseByte(GetValue(dict, "pickup_type")),
                    DropOffType = ParseByte(GetValue(dict, "drop_off_type")),
                    ShapeDistTraveled = ParseDouble(GetValue(dict, "shape_dist_traveled"))
                };

                batch.Add(stopTime);
                count++;

                if (count >= 1000)
                {
                    _context.StopTimes.AddRange(batch);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                    batch.Clear();
                    count = 0;
                }
            }
            if (batch.Any())
            {
                _context.StopTimes.AddRange(batch);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
            }
        }

        // --- Helpers ---

        private string? GetValue(IDictionary<string, object> dict, string key)
        {
            var normalizedKey = key.Replace("_", "").Replace(" ", "").ToLower();
            
            foreach (var k in dict.Keys)
            {
                var normalizedDictKey = k.Replace("_", "").Replace(" ", "").ToLower();
                if (normalizedDictKey == normalizedKey)
                {
                    var val = dict[k]?.ToString()?.Trim();
                    return string.IsNullOrEmpty(val) ? null : val;
                }
            }
            return null;
        }

        private decimal ParseDecimal(string? val) => decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0;
        private double? ParseDouble(string? val) => double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (double?)null;
        private int? ParseInt(string? val) => int.TryParse(val, out var i) ? i : (int?)null;
        private byte? ParseByte(string? val) => byte.TryParse(val, out var b) ? b : (byte?)null;
        private DateTime ParseDate(string? val)
        {
            if (val != null && val.Length == 8 && DateTime.TryParseExact(val, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return DateTime.MinValue;
        }
        private TimeSpan? ParseTime(string? val)
        {
            if (string.IsNullOrEmpty(val)) return null;
            var parts = val.Split(':');
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
                {
                    int s = parts.Length > 2 && int.TryParse(parts[2], out int sec) ? sec : 0;
                    return new TimeSpan(h, m, s);
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            // 1. Agencies
            await ImportAgencies(gtfsDirectoryPath, agencyMap);

            // 2. Calendars
            await ImportCalendars(gtfsDirectoryPath, calendarMap);

            // 3. Shapes (Master)
            await ImportShapesMaster(gtfsDirectoryPath, shapeMap);

            // 4. Stops
            await ImportStops(gtfsDirectoryPath, stopMap);

            // 5. Routes
            await ImportRoutes(gtfsDirectoryPath, agencyMap, routeMap);

            // 6. Trips
            await ImportTrips(gtfsDirectoryPath, routeMap, calendarMap, shapeMap, tripMap);

            // 7. StopTimes
            await ImportStopTimes(gtfsDirectoryPath, tripMap, stopMap);

            // 8. Shape Points
            await ImportShapePoints(gtfsDirectoryPath, shapeMap);
        }

        private async Task ImportAgencies(string path, Dictionary<string, int> map)
        {
            var file = Path.Combine(path, "agency.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            var headers = GetHeaders(reader);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = ParseCsvLine(line);
                
                var agency = new Agency
                {
                    GtfsAgencyId = GetValue(headers, values, "agency_id"),
                    AgencyName = GetValue(headers, values, "agency_name") ?? "Unknown",
                    AgencyUrl = GetValue(headers, values, "agency_url") ?? "",
                    AgencyTimezone = GetValue(headers, values, "agency_timezone") ?? "",
                    AgencyPhone = GetValue(headers, values, "agency_phone"),
                    AgencyLang = GetValue(headers, values, "agency_lang")
                };

                _context.Agencies.Add(agency);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(agency.GtfsAgencyId))
                {
                    map[agency.GtfsAgencyId] = agency.Id;
                }
            }
        }

        private async Task ImportCalendars(string path, Dictionary<string, int> map)
        {
            var file = Path.Combine(path, "calendar.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            var headers = GetHeaders(reader);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = ParseCsvLine(line);

                var serviceId = GetValue(headers, values, "service_id");
                if (string.IsNullOrEmpty(serviceId)) continue;

                var cal = new TransitCalendar
                {
                    GtfsServiceId = serviceId,
                    StartDate = ParseDate(GetValue(headers, values, "start_date")),
                    EndDate = ParseDate(GetValue(headers, values, "end_date")),
                    Monday = GetValue(headers, values, "monday") == "1",
                    Tuesday = GetValue(headers, values, "tuesday") == "1",
                    Wednesday = GetValue(headers, values, "wednesday") == "1",
                    Thursday = GetValue(headers, values, "thursday") == "1",
                    Friday = GetValue(headers, values, "friday") == "1",
                    Saturday = GetValue(headers, values, "saturday") == "1",
                    Sunday = GetValue(headers, values, "sunday") == "1"
                };

                _context.TransitCalendars.Add(cal);
                await _context.SaveChangesAsync();
                map[serviceId] = cal.Id;
            }
        }

        private async Task ImportShapesMaster(string path, Dictionary<string, int> map)
        {
            var file = Path.Combine(path, "shapes.txt");
            if (!File.Exists(file)) return;

            // Shapes file contains points, we need distinct shape_ids first
            var distinctShapeIds = new HashSet<string>();
            using (var reader = new StreamReader(file))
            {
                var headers = GetHeaders(reader);
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = ParseCsvLine(line);
                    var shapeId = GetValue(headers, values, "shape_id");
                    if (!string.IsNullOrEmpty(shapeId)) distinctShapeIds.Add(shapeId);
                }
            }

            foreach (var shapeId in distinctShapeIds)
            {
                var master = new ShapesMaster { GtfsShapeId = shapeId };
                _context.ShapesMasters.Add(master);
                await _context.SaveChangesAsync();
                map[shapeId] = master.Id;
            }
        }

        private async Task ImportStops(string path, Dictionary<string, int> map)
        {
            var file = Path.Combine(path, "stops.txt");
            if (!File.Exists(file)) return;

            // Pass 1: Create Stops
            var stopsToUpdateParent = new List<(int InternalId, string ParentGtfsId)>();

            using (var reader = new StreamReader(file))
            {
                var headers = GetHeaders(reader);
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = ParseCsvLine(line);
                    var gtfsId = GetValue(headers, values, "stop_id");
                    if (string.IsNullOrEmpty(gtfsId)) continue;

                    var stop = new Stop
                    {
                        GtfsStopId = gtfsId,
                        StopCode = GetValue(headers, values, "stop_code"),
                        StopName = GetValue(headers, values, "stop_name") ?? "Unknown",
                        StopDesc = GetValue(headers, values, "stop_desc"),
                        StopLat = ParseDecimal(GetValue(headers, values, "stop_lat")),
                        StopLon = ParseDecimal(GetValue(headers, values, "stop_lon")),
                        ZoneId = GetValue(headers, values, "zone_id"),
                        StopUrl = GetValue(headers, values, "stop_url"),
                        LocationType = ParseByte(GetValue(headers, values, "location_type")),
                        WheelchairBoarding = ParseByte(GetValue(headers, values, "wheelchair_boarding")),
                        StopTimezone = GetValue(headers, values, "stop_timezone")
                    };

                    _context.Stops.Add(stop);
                    await _context.SaveChangesAsync();
                    map[gtfsId] = stop.Id;

                    var parentId = GetValue(headers, values, "parent_station");
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        stopsToUpdateParent.Add((stop.Id, parentId));
                    }
                }
            }

            // Pass 2: Update Parent Stations
            foreach (var item in stopsToUpdateParent)
            {
                if (map.TryGetValue(item.ParentGtfsId, out int parentInternalId))
                {
                    var stop = await _context.Stops.FindAsync(item.InternalId);
                    if (stop != null)
                    {
                        stop.ParentStationId = parentInternalId;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task ImportRoutes(string path, Dictionary<string, int> agencyMap, Dictionary<string, int> routeMap)
        {
            var file = Path.Combine(path, "routes.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            var headers = GetHeaders(reader);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = ParseCsvLine(line);
                var gtfsId = GetValue(headers, values, "route_id");
                if (string.IsNullOrEmpty(gtfsId)) continue;

                var agencyGtfsId = GetValue(headers, values, "agency_id");
                int? agencyId = null;
                if (!string.IsNullOrEmpty(agencyGtfsId) && agencyMap.ContainsKey(agencyGtfsId))
                {
                    agencyId = agencyMap[agencyGtfsId];
                }
                else if (agencyMap.Count == 1) // Fallback if only 1 agency exists
                {
                    agencyId = agencyMap.Values.First();
                }

                var route = new TransitRoute
                {
                    GtfsRouteId = gtfsId,
                    AgencyId = agencyId,
                    RouteShortName = GetValue(headers, values, "route_short_name"),
                    RouteLongName = GetValue(headers, values, "route_long_name"),
                    RouteType = ParseInt(GetValue(headers, values, "route_type")),
                    RouteTextColor = GetValue(headers, values, "route_text_color"),
                    RouteColor = GetValue(headers, values, "route_color"),
                    RouteUrl = GetValue(headers, values, "route_url"),
                    RouteDesc = GetValue(headers, values, "route_desc")
                };

                _context.TransitRoutes.Add(route);
                await _context.SaveChangesAsync();
                routeMap[gtfsId] = route.Id;
            }
        }

        private async Task ImportTrips(string path, Dictionary<string, int> routeMap, Dictionary<string, int> serviceMap, Dictionary<string, int> shapeMap, Dictionary<string, int> tripMap)
        {
            var file = Path.Combine(path, "trips.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            var headers = GetHeaders(reader);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = ParseCsvLine(line);
                var gtfsId = GetValue(headers, values, "trip_id");
                if (string.IsNullOrEmpty(gtfsId)) continue;

                var routeGtfsId = GetValue(headers, values, "route_id");
                var serviceGtfsId = GetValue(headers, values, "service_id");
                var shapeGtfsId = GetValue(headers, values, "shape_id");

                if (!routeMap.ContainsKey(routeGtfsId) || !serviceMap.ContainsKey(serviceGtfsId)) continue;

                var trip = new Trip
                {
                    GtfsTripId = gtfsId,
                    TransitRouteId = routeMap[routeGtfsId],
                    ServiceId = serviceMap[serviceGtfsId],
                    ShapeId = !string.IsNullOrEmpty(shapeGtfsId) && shapeMap.ContainsKey(shapeGtfsId) ? shapeMap[shapeGtfsId] : (int?)null,
                    TripHeadsign = GetValue(headers, values, "trip_headsign"),
                    TripShortName = GetValue(headers, values, "trip_short_name"),
                    DirectionId = ParseByte(GetValue(headers, values, "direction_id")),
                    WheelchairAccessible = ParseByte(GetValue(headers, values, "wheelchair_accessible")),
                    BlockId = GetValue(headers, values, "block_id")
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                tripMap[gtfsId] = trip.Id;
            }
        }

        private async Task ImportStopTimes(string path, Dictionary<string, int> tripMap, Dictionary<string, int> stopMap)
        {
            var file = Path.Combine(path, "stop_times.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            var headers = GetHeaders(reader);

            var batch = new List<StopTime>();
            int count = 0;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = ParseCsvLine(line);

                var tripGtfsId = GetValue(headers, values, "trip_id");
                var stopGtfsId = GetValue(headers, values, "stop_id");

                if (!tripMap.ContainsKey(tripGtfsId) || !stopMap.ContainsKey(stopGtfsId)) continue;

                var stopTime = new StopTime
                {
                    TripId = tripMap[tripGtfsId],
                    StopId = stopMap[stopGtfsId],
                    StopSequence = ParseInt(GetValue(headers, values, "stop_sequence")) ?? 0,
                    ArrivalTime = ParseTime(GetValue(headers, values, "arrival_time")),
                    DepartureTime = ParseTime(GetValue(headers, values, "departure_time")),
                    StopHeadsign = GetValue(headers, values, "stop_headsign"),
                    PickupType = ParseByte(GetValue(headers, values, "pickup_type")),
                    DropOffType = ParseByte(GetValue(headers, values, "drop_off_type")),
                    ShapeDistTraveled = ParseDouble(GetValue(headers, values, "shape_dist_traveled"))
                };

                batch.Add(stopTime);
                count++;

                if (count >= 1000)
                {
                    _context.StopTimes.AddRange(batch);
                    await _context.SaveChangesAsync();
                    batch.Clear();
                    count = 0;
                }
            }
            if (batch.Any())
            {
                _context.StopTimes.AddRange(batch);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ImportShapePoints(string path, Dictionary<string, int> shapeMap)
        {
            var file = Path.Combine(path, "shapes.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            var headers = GetHeaders(reader);

            var batch = new List<Shape>();
            int count = 0;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = ParseCsvLine(line);
                var shapeGtfsId = GetValue(headers, values, "shape_id");

                if (string.IsNullOrEmpty(shapeGtfsId) || !shapeMap.ContainsKey(shapeGtfsId)) continue;

                var shape = new Shape
                {
                    ShapeId = shapeMap[shapeGtfsId],
                    ShapePtSequence = ParseInt(GetValue(headers, values, "shape_pt_sequence")) ?? 0,
                    ShapePtLat = ParseDecimal(GetValue(headers, values, "shape_pt_lat")),
                    ShapePtLon = ParseDecimal(GetValue(headers, values, "shape_pt_lon")),
                    ShapeDistTraveled = ParseDecimal(GetValue(headers, values, "shape_dist_traveled"))
                };

                batch.Add(shape);
                count++;

                if (count >= 1000)
                {
                    _context.Shapes.AddRange(batch);
                    await _context.SaveChangesAsync();
                    batch.Clear();
                    count = 0;
                }
            }
            if (batch.Any())
            {
                _context.Shapes.AddRange(batch);
                await _context.SaveChangesAsync();
            }
        }

        // Helpers
        private HeaderMap GetHeaders(StreamReader reader)
        {
            var line = reader.ReadLine();
            var map = new Dictionary<string, int>();
            if (line == null) return new HeaderMap(map);

            var parts = ParseCsvLine(line);
            for (int i = 0; i < parts.Length; i++)
            {
                map[parts[i].Trim().ToLower()] = i;
            }
            return new HeaderMap(map);
        }

        private string? GetValue(HeaderMap headers, string[] values, string key)
        {
            var index = headers.GetIndex(key);
            if (index.HasValue && index.Value < values.Length)
            {
                var val = values[index.Value].Trim();
                return string.IsNullOrEmpty(val) ? null : val;
            }
            return null;
        }

        private string[] ParseCsvLine(string line)
        {
            // Simple split for now, robust CSV parsing is complex
            return line.Split(','); 
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
                int h = int.Parse(parts[0]);
                int m = int.Parse(parts[1]);
                int s = parts.Length > 2 ? int.Parse(parts[2]) : 0;
                return new TimeSpan(h, m, s);
            }
            return null;
        }

        // Intelligent Header Matching
        private class HeaderMap
        {
            private readonly Dictionary<string, int> _map;
            private readonly Dictionary<string, int> _cache = new();

            private static readonly Dictionary<string, string[]> ColumnAliases = new()
            {
                { "agency_id", new[] { "agency_code", "agencyid" } },
                { "agency_name", new[] { "agencyname", "name" } },
                { "stop_id", new[] { "stopid", "id" } },
                { "stop_code", new[] { "stopcode", "code" } },
                { "stop_name", new[] { "stopname", "name" } },
                { "stop_lat", new[] { "stop_latitude", "latitude", "lat" } },
                { "stop_lon", new[] { "stop_longitude", "longitude", "lon", "lng" } },
                { "route_id", new[] { "routeid", "id" } },
                { "route_short_name", new[] { "route_short", "short_name", "route_name" } },
                { "route_long_name", new[] { "route_long", "long_name" } },
                { "trip_id", new[] { "tripid", "id" } },
                { "service_id", new[] { "serviceid", "service" } },
                { "shape_id", new[] { "shapeid", "shape" } },
                { "arrival_time", new[] { "arrival", "arrivaltime" } },
                { "departure_time", new[] { "departure", "departuretime" } },
                { "stop_sequence", new[] { "stop_seq", "sequence", "seq" } },
                { "shape_pt_lat", new[] { "shape_lat", "lat", "latitude" } },
                { "shape_pt_lon", new[] { "shape_lon", "lon", "lng", "longitude" } },
                { "shape_pt_sequence", new[] { "shape_seq", "sequence", "seq", "pt_sequence" } },
                { "shape_dist_traveled", new[] { "shape_dist", "dist_traveled", "dist" } }
            };

            public HeaderMap(Dictionary<string, int> map)
            {
                _map = map;
            }

            public int? GetIndex(string key)
            {
                if (_cache.ContainsKey(key)) return _cache[key];

                // 1. Exact match
                if (_map.ContainsKey(key))
                {
                    _cache[key] = _map[key];
                    return _map[key];
                }

                // 2. Alias match
                if (ColumnAliases.ContainsKey(key))
                {
                    foreach (var alias in ColumnAliases[key])
                    {
                        if (_map.ContainsKey(alias))
                        {
                            _cache[key] = _map[alias];
                            return _map[alias];
                        }
                    }
                }

                // 3. Fuzzy / Contains match
                // Find the key in _map that is "closest"
                var bestMatch = FindBestMatch(key, _map.Keys);
                if (bestMatch != null)
                {
                    _cache[key] = _map[bestMatch];
                    return _map[bestMatch];
                }

                return null;
            }

            private string? FindBestMatch(string target, IEnumerable<string> candidates)
            {
                string? bestCandidate = null;
                int bestDistance = int.MaxValue;

                foreach (var candidate in candidates)
                {
                    // Simple contains check first
                    if (candidate.Contains(target) || target.Contains(candidate))
                    {
                        return candidate;
                    }

                    // Levenshtein distance
                    int dist = ComputeLevenshteinDistance(target, candidate);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestCandidate = candidate;
                    }
                }

                // Only return if distance is small enough (e.g. <= 2)
                return bestDistance <= 2 ? bestCandidate : null;
            }

            private int ComputeLevenshteinDistance(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                if (n == 0) return m;
                if (m == 0) return n;

                for (int i = 0; i <= n; d[i, 0] = i++) { }
                for (int j = 0; j <= m; d[0, j] = j++) { }

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                return d[n, m];
            }
        }
    }
}

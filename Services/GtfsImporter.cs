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
        public List<string> ImportLogs { get; } = new List<string>();

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

            // 7. CalendarDates (Exceptions)
            await ImportCalendarDates(gtfsDirectoryPath, calendarMap, config);

            // 8. StopTimes
            await ImportStopTimes(gtfsDirectoryPath, tripMap, stopMap, config);

            // 9. Frequencies
            await ImportFrequencies(gtfsDirectoryPath, tripMap, config);

            // 10. Transfers
            await ImportTransfers(gtfsDirectoryPath, stopMap, config);

            // 11. FeedInfo
            await ImportFeedInfo(gtfsDirectoryPath, config);
        }

        private async Task ImportAgencies(string path, Dictionary<string, int> map, CsvConfiguration config)
        {
            var file = Path.Combine(path, "agency.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);

            // Read as dynamic to handle flexible columns manually or map to a DTO
            var agencies = csv.GetRecords<dynamic>();

            foreach (var r in agencies)
            {
                var dict = (IDictionary<string, object>)r;
                var gtfsId = GetValue(dict, "agency_id");
                
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
                await _context.SaveChangesAsync(); // We need ID immediately for the map

                if (!string.IsNullOrEmpty(agency.GtfsAgencyId))
                {
                    map[agency.GtfsAgencyId] = agency.Id;
                }
                else
                {
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
                await _context.SaveChangesAsync(); // Need ID for map
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
                        LocationType = ParseEnum<LocationType>(GetValue(dict, "location_type")),
                        WheelchairBoarding = ParseEnum<WheelchairBoarding>(GetValue(dict, "wheelchair_boarding")),
                        StopTimezone = GetValue(dict, "stop_timezone"),
                        TtsStopName = GetValue(dict, "tts_stop_name"),
                        PlatformCode = GetValue(dict, "platform_code")
                    };

                    _context.Stops.Add(stop);
                    await _context.SaveChangesAsync(); // Still needed individual saves for parent references in current logic, or we batch stops then update parents.
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
                    if (IsCircularStation(item.InternalId, parentInternalId))
                    {
                        ImportLogs.Add($"Error: Circular reference detected for stop ID {item.InternalId} and parent {parentInternalId}. Skipping parent link.");
                        continue;
                    }

                    var stop = new Stop { Id = item.InternalId, ParentStationId = parentInternalId };
                    _context.Stops.Attach(stop);
                    _context.Entry(stop).Property(x => x.ParentStationId).IsModified = true;
                }
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        private bool IsCircularStation(int stopId, int parentId)
        {
            var current = _context.Stops.Find(parentId);
            int depth = 0;
            while (current?.ParentStationId != null && depth < 50)
            {
                if (current.ParentStationId == stopId) return true;
                current = _context.Stops.Find(current.ParentStationId);
                depth++;
            }
            return false;
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
                else if (agencyMap.Count == 1)
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
                    RouteType = ParseEnum<RouteType>(GetValue(dict, "route_type")) ?? RouteType.Bus,
                    RouteTextColor = CleanColor(GetValue(dict, "route_text_color")),
                    RouteColor = CleanColor(GetValue(dict, "route_color")),
                    RouteUrl = GetValue(dict, "route_url"),
                    RouteDesc = GetValue(dict, "route_desc"),
                    ContinuousPickup = ParseEnum<ContinuousStopping>(GetValue(dict, "continuous_pickup")),
                    ContinuousDropOff = ParseEnum<ContinuousStopping>(GetValue(dict, "continuous_drop_off")),
                    RouteSortOrder = ParseInt(GetValue(dict, "route_sort_order"))
                };

                _context.TransitRoutes.Add(route);
                await _context.SaveChangesAsync(); // Need ID for map
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

                if (string.IsNullOrEmpty(routeId) || !routeMap.ContainsKey(routeId) || 
                    string.IsNullOrEmpty(serviceId) || !serviceMap.ContainsKey(serviceId)) continue;

                var trip = new Trip
                {
                    GtfsTripId = gtfsId,
                    TransitRouteId = routeMap[routeId],
                    ServiceId = serviceMap[serviceId],
                    ShapeId = !string.IsNullOrEmpty(shapeId) && shapeMap.ContainsKey(shapeId) ? shapeMap[shapeId] : (int?)null,
                    TripHeadsign = GetValue(dict, "trip_headsign"),
                    TripShortName = GetValue(dict, "trip_short_name"),
                    DirectionId = ParseEnum<DirectionId>(GetValue(dict, "direction_id")),
                    WheelchairAccessible = ParseEnum<WheelchairBoarding>(GetValue(dict, "wheelchair_accessible")),
                    BikesAllowed = ParseEnum<BikesAllowed>(GetValue(dict, "bikes_allowed")),
                    BlockId = GetValue(dict, "block_id")
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync(); // Need ID for map
                tripMap[gtfsId] = trip.Id;
            }
        }

        private async Task ImportCalendarDates(string path, Dictionary<string, int> calendarMap, CsvConfiguration config)
        {
            var file = Path.Combine(path, "calendar_dates.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();
            var batch = new List<CalendarDate>();
            int count = 0;

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var serviceId = GetValue(dict, "service_id");
                if (string.IsNullOrEmpty(serviceId)) continue;

                var dateStr = GetValue(dict, "date");
                var excTypeStr = GetValue(dict, "exception_type");

                if (string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(excTypeStr)) continue;

                var calDate = new CalendarDate
                {
                    GtfsServiceId = serviceId,
                    Date = ParseDate(dateStr),
                    ExceptionType = ParseEnum<ExceptionType>(excTypeStr) ?? ExceptionType.Added,
                    TransitCalendarId = calendarMap.ContainsKey(serviceId) ? calendarMap[serviceId] : (int?)null
                };

                batch.Add(calDate);
                count++;
                if (count >= 1000)
                {
                    _context.CalendarDates.AddRange(batch);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                    batch.Clear();
                    count = 0;
                }
            }
            if (batch.Any())
            {
                _context.CalendarDates.AddRange(batch);
                await _context.SaveChangesAsync();
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
                    PickupType = ParseEnum<PickupDropOffType>(GetValue(dict, "pickup_type")),
                    DropOffType = ParseEnum<PickupDropOffType>(GetValue(dict, "drop_off_type")),
                    ShapeDistTraveled = ParseDouble(GetValue(dict, "shape_dist_traveled")),
                    Timepoint = ParseEnum<TimepointType>(GetValue(dict, "timepoint")),
                    ContinuousPickup = ParseEnum<ContinuousStopping>(GetValue(dict, "continuous_pickup")),
                    ContinuousDropOff = ParseEnum<ContinuousStopping>(GetValue(dict, "continuous_drop_off"))
                };

                batch.Add(stopTime);
                count++;

                if (count >= 2000) // Increased batch size
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

        private async Task ImportFrequencies(string path, Dictionary<string, int> tripMap, CsvConfiguration config)
        {
            var file = Path.Combine(path, "frequencies.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();
            var batch = new List<Frequency>();
            int count = 0;

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var gtfsTripId = GetValue(dict, "trip_id");
                if (string.IsNullOrEmpty(gtfsTripId) || !tripMap.ContainsKey(gtfsTripId)) continue;

                var freq = new Frequency
                {
                    TripId = tripMap[gtfsTripId],
                    StartTime = ParseGtfsTime(GetValue(dict, "start_time")) ?? TimeSpan.Zero,
                    EndTime = ParseGtfsTime(GetValue(dict, "end_time")) ?? TimeSpan.Zero,
                    HeadwaySecs = ParseInt(GetValue(dict, "headway_secs")) ?? 0,
                    ExactTimes = ParseByte(GetValue(dict, "exact_times"))
                };

                batch.Add(freq);
                count++;
                if (count >= 1000)
                {
                    _context.Frequencies.AddRange(batch);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                    batch.Clear();
                    count = 0;
                }
            }
            if (batch.Any())
            {
                _context.Frequencies.AddRange(batch);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ImportTransfers(string path, Dictionary<string, int> stopMap, CsvConfiguration config)
        {
            var file = Path.Combine(path, "transfers.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();
            var batch = new List<Transfer>();
            int count = 0;

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var fromId = GetValue(dict, "from_stop_id");
                var toId = GetValue(dict, "to_stop_id");

                if (string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(toId) || 
                    !stopMap.ContainsKey(fromId) || !stopMap.ContainsKey(toId)) continue;

                var trans = new Transfer
                {
                    FromStopId = stopMap[fromId],
                    ToStopId = stopMap[toId],
                    TransferType = ParseByte(GetValue(dict, "transfer_type")) ?? 0,
                    MinTransferTime = ParseInt(GetValue(dict, "min_transfer_time"))
                };
                
                batch.Add(trans);
                count++;
                if (count >= 1000)
                {
                    _context.Transfers.AddRange(batch);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                    batch.Clear();
                    count = 0;
                }
            }
            if (batch.Any())
            {
                _context.Transfers.AddRange(batch);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ImportFeedInfo(string path, CsvConfiguration config)
        {
            var file = Path.Combine(path, "feed_info.txt");
            if (!File.Exists(file)) return;

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();

            foreach (var r in records)
            {
                var dict = (IDictionary<string, object>)r;
                var info = new FeedInfo
                {
                    FeedPublisherName = GetValue(dict, "feed_publisher_name") ?? "Unknown",
                    FeedPublisherUrl = GetValue(dict, "feed_publisher_url") ?? "",
                    FeedLang = GetValue(dict, "feed_lang") ?? "",
                    FeedStartDate = ParseDateOptional(GetValue(dict, "feed_start_date")),
                    FeedEndDate = ParseDateOptional(GetValue(dict, "feed_end_date")),
                    FeedVersion = GetValue(dict, "feed_version"),
                    FeedContactEmail = GetValue(dict, "feed_contact_email"),
                    FeedContactUrl = GetValue(dict, "feed_contact_url")
                };
                _context.FeedInfos.Add(info);
            }
            await _context.SaveChangesAsync();
        }

        private static TimeSpan? ParseGtfsTime(string? v)
        {
            if (string.IsNullOrEmpty(v)) return null;
            var parts = v.Split(':');
            if (parts.Length < 2) return null;

            if (int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
            {
                int s = parts.Length > 2 && int.TryParse(parts[2], out int sec) ? sec : 0;
                return new TimeSpan(h, m, s);
            }
            return null;
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

        private T? ParseEnum<T>(string? val) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(val)) return null;
            if (Enum.TryParse<T>(val, true, out var result)) return result;
            
            // Try numeric parse if it's a number string
            if (int.TryParse(val, out int numericVal) && Enum.IsDefined(typeof(T), numericVal))
            {
                return (T)(object)numericVal;
            }

            ImportLogs.Add($"Warning: Invalid value '{val}' for enum {typeof(T).Name}");
            return null;
        }

        private string? CleanColor(string? val)
        {
            if (string.IsNullOrEmpty(val)) return null;
            var clean = val.Replace("#", "").Trim().ToUpper();
            if (clean.Length == 6) return clean;
            ImportLogs.Add($"Warning: Invalid color format '{val}'. Colors must be 6-digit hex.");
            return null;
        }

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
        private DateTime? ParseDateOptional(string? val)
        {
            if (val != null && val.Length == 8 && DateTime.TryParseExact(val, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return null;
        }
    }
}

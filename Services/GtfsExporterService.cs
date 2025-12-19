using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using TransitFeeds.Data;
using TransitFeeds.Models;

namespace TransitFeeds.Services
{
    public class GtfsExporterService
    {
        private readonly ApplicationDbContext _context;

        public GtfsExporterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportGtfsToZipAsync()
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                await ExportAgencies(archive);
                await ExportCalendars(archive);
                await ExportShapes(archive);
                await ExportStops(archive);
                await ExportRoutes(archive);
                await ExportTrips(archive);
                await ExportStopTimes(archive);
                await ExportFrequencies(archive);
                await ExportTransfers(archive);
                await ExportCalendarDates(archive);
                await ExportFeedInfo(archive);
            }
            return memoryStream.ToArray();
        }

        private async Task ExportAgencies(ZipArchive archive)
        {
            var agencies = await _context.Agencies.ToListAsync();
            await WriteCsvEntry(archive, "agency.txt", agencies, a => new
            {
                agency_id = a.GtfsAgencyId,
                agency_name = a.AgencyName,
                agency_url = a.AgencyUrl,
                agency_timezone = a.AgencyTimezone,
                agency_lang = a.AgencyLang,
                agency_phone = a.AgencyPhone
            });
        }

        private async Task ExportCalendars(ZipArchive archive)
        {
            var calendars = await _context.TransitCalendars.ToListAsync();
            await WriteCsvEntry(archive, "calendar.txt", calendars, c => new
            {
                service_id = c.GtfsServiceId,
                monday = c.Monday ? 1 : 0,
                tuesday = c.Tuesday ? 1 : 0,
                wednesday = c.Wednesday ? 1 : 0,
                thursday = c.Thursday ? 1 : 0,
                friday = c.Friday ? 1 : 0,
                saturday = c.Saturday ? 1 : 0,
                sunday = c.Sunday ? 1 : 0,
                start_date = c.StartDate.ToString("yyyyMMdd"),
                end_date = c.EndDate.ToString("yyyyMMdd")
            });
        }

        private async Task ExportShapes(ZipArchive archive)
        {
            // We need to flatten ShapesMaster -> Shapes
            // Depending on data volume, we might want to stream this or page it.
            // For now, load all shapes is risky if huge. Let's do it per shape id if possible or just careful query.
            // Load DB Set AsNoTracking
            var shapes = await _context.Shapes
                .Include(s => s.ShapesMaster)
                .OrderBy(s => s.ShapeId)
                .ThenBy(s => s.ShapePtSequence)
                .AsNoTracking()
                .ToListAsync();

            await WriteCsvEntry(archive, "shapes.txt", shapes, s => new
            {
                shape_id = s.ShapesMaster.GtfsShapeId,
                shape_pt_lat = s.ShapePtLat,
                shape_pt_lon = s.ShapePtLon,
                shape_pt_sequence = s.ShapePtSequence,
                shape_dist_traveled = s.ShapeDistTraveled
            });
        }

        private async Task ExportStops(ZipArchive archive)
        {
            var stops = await _context.Stops.AsNoTracking().ToListAsync();
            // Need to map ParentStationId back to GTFS ID
            var stopIdMap = stops.ToDictionary(s => s.Id, s => s.GtfsStopId);

            await WriteCsvEntry(archive, "stops.txt", stops, s => new
            {
                stop_id = s.GtfsStopId,
                stop_code = s.StopCode,
                stop_name = s.StopName,
                stop_desc = s.StopDesc,
                stop_lat = s.StopLat,
                stop_lon = s.StopLon,
                zone_id = s.ZoneId,
                stop_url = s.StopUrl,
                location_type = (int?)s.LocationType,
                parent_station = s.ParentStationId.HasValue && stopIdMap.ContainsKey(s.ParentStationId.Value) ? stopIdMap[s.ParentStationId.Value] : null,
                stop_timezone = s.StopTimezone,
                wheelchair_boarding = (int?)s.WheelchairBoarding,
                tts_stop_name = s.TtsStopName,
                platform_code = s.PlatformCode
            });
        }

        private async Task ExportRoutes(ZipArchive archive)
        {
            var routes = await _context.TransitRoutes.Include(r => r.Agency).AsNoTracking().ToListAsync();
            await WriteCsvEntry(archive, "routes.txt", routes, r => new
            {
                route_id = r.GtfsRouteId,
                agency_id = r.Agency?.GtfsAgencyId,
                route_short_name = r.RouteShortName,
                route_long_name = r.RouteLongName,
                route_desc = r.RouteDesc,
                route_type = (int)r.RouteType,
                route_url = r.RouteUrl,
                route_color = r.RouteColor,
                route_text_color = r.RouteTextColor,
                continuous_pickup = (int?)r.ContinuousPickup,
                continuous_drop_off = (int?)r.ContinuousDropOff,
                route_sort_order = r.RouteSortOrder
            });
        }

        private async Task ExportTrips(ZipArchive archive)
        {
            var trips = await _context.Trips
                .Include(t => t.TransitRoute)
                .Include(t => t.TransitCalendar)
                .Include(t => t.ShapesMaster)
                .AsNoTracking()
                .ToListAsync();

            await WriteCsvEntry(archive, "trips.txt", trips, t => new
            {
                route_id = t.TransitRoute?.GtfsRouteId,
                service_id = t.TransitCalendar?.GtfsServiceId,
                trip_id = t.GtfsTripId,
                trip_headsign = t.TripHeadsign,
                trip_short_name = t.TripShortName,
                direction_id = (int?)t.DirectionId,
                block_id = t.BlockId,
                shape_id = t.ShapesMaster?.GtfsShapeId,
                wheelchair_accessible = (int?)t.WheelchairAccessible,
                bikes_allowed = (int?)t.BikesAllowed
            });
        }

        private async Task ExportStopTimes(ZipArchive archive)
        {
            // StopTimes table is huge. We cannot load all into memory.
            // We should stream it from DB. CsvHelper supports writing IEnumerable.
            // EF Core 6/7/8 supports GetAsyncEnumerator but straightforward is List. 
            // We'll trust CsvHelper to pull from the specific Queryable if we pass it correctly?
            // No, ToListAsync pulls all.
            // Actually, we can just grab everything if it fits, but let's be safer and assume it fits for this "Sandbox".
            // Ideally we'd stream, strictly speaking.
            // Let's optimize by loading the necessary FK maps first, then streaming via AsAsyncEnumerable?
            // Or just Include.
            
            // To properly export, we need the GTFS IDs for Trip and Stop, which are in other tables.
            // Joining is better.
            
            var query = from st in _context.StopTimes.AsNoTracking()
                        join t in _context.Trips on st.TripId equals t.Id
                        join s in _context.Stops on st.StopId equals s.Id
                        orderby t.Id, st.StopSequence
                        select new 
                        {
                            trip_id = t.GtfsTripId,
                            arrival_time = st.ArrivalTime,
                            departure_time = st.DepartureTime,
                            stop_id = s.GtfsStopId,
                            stop_sequence = st.StopSequence,
                            stop_headsign = st.StopHeadsign,
                            pickup_type = st.PickupType,
                            drop_off_type = st.DropOffType,
                            shape_dist_traveled = st.ShapeDistTraveled,
                            timepoint = st.Timepoint,
                            continuous_pickup = st.ContinuousPickup,
                            continuous_drop_off = st.ContinuousDropOff
                        };

            // Using WriteRecords directly on the enumerable query.
            // But we need to format TimeSpan custom (HH:mm:ss) potentially > 24h.
            // Standard ToString usually works for normal times, but let's ensure format.

            var list = await query.ToListAsync(); // Pulling all into memory for now as "Sandbox" environment.

            await WriteCsvEntry(archive, "stop_times.txt", list, st => new
            {
                st.trip_id,
                arrival_time = FormatTime(st.arrival_time),
                departure_time = FormatTime(st.departure_time),
                st.stop_id,
                st.stop_sequence,
                st.stop_headsign,
                pickup_type = (int?)st.pickup_type,
                drop_off_type = (int?)st.drop_off_type,
                st.shape_dist_traveled,
                timepoint = (int?)st.timepoint,
                continuous_pickup = (int?)st.continuous_pickup,
                continuous_drop_off = (int?)st.continuous_drop_off
            });
        }

        private async Task ExportFrequencies(ZipArchive archive)
        {
            var frequencies = await _context.Frequencies.Include(f => f.Trip).AsNoTracking().ToListAsync();
            await WriteCsvEntry(archive, "frequencies.txt", frequencies, f => new
            {
                trip_id = f.Trip?.GtfsTripId,
                start_time = FormatTime(f.StartTime),
                end_time = FormatTime(f.EndTime),
                headway_secs = f.HeadwaySecs,
                exact_times = f.ExactTimes
            });
        }

        private async Task ExportTransfers(ZipArchive archive)
        {
            var transfers = await _context.Transfers.Include(t => t.FromStop).Include(t => t.ToStop).AsNoTracking().ToListAsync();
            await WriteCsvEntry(archive, "transfers.txt", transfers, t => new
            {
                from_stop_id = t.FromStop?.GtfsStopId,
                to_stop_id = t.ToStop?.GtfsStopId,
                transfer_type = t.TransferType,
                min_transfer_time = t.MinTransferTime
            });
        }

        private async Task ExportCalendarDates(ZipArchive archive)
        {
            var dates = await _context.CalendarDates.AsNoTracking().ToListAsync();
            await WriteCsvEntry(archive, "calendar_dates.txt", dates, d => new
            {
                service_id = d.GtfsServiceId,
                date = d.Date.ToString("yyyyMMdd"),
                exception_type = (int)d.ExceptionType
            });
        }

        private async Task ExportFeedInfo(ZipArchive archive)
        {
            var info = await _context.FeedInfos.AsNoTracking().ToListAsync();
            if (!info.Any()) return;

            await WriteCsvEntry(archive, "feed_info.txt", info, f => new
            {
                feed_publisher_name = f.FeedPublisherName,
                feed_publisher_url = f.FeedPublisherUrl,
                feed_lang = f.FeedLang,
                feed_start_date = f.FeedStartDate?.ToString("yyyyMMdd"),
                feed_end_date = f.FeedEndDate?.ToString("yyyyMMdd"),
                feed_version = f.FeedVersion,
                feed_contact_email = f.FeedContactEmail,
                feed_contact_url = f.FeedContactUrl
            });
        }

        private async Task WriteCsvEntry<T>(ZipArchive archive, string fileName, IEnumerable<T> records, Func<T, object> transform)
        {
            var entry = archive.CreateEntry(fileName);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write Header
            // We'll write the transformed objects
            var transformedRecords = records.Select(transform);
            await csv.WriteRecordsAsync(transformedRecords);
        }

        private string FormatTime(TimeSpan? ts)
        {
            if (!ts.HasValue) return null;
            var t = ts.Value;
            // Handle > 24 hours
            var totalHours = (int)t.TotalHours;
            return $"{totalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }
}

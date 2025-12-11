using System;
using System.Linq;
using TransitFeeds.Models;

namespace TransitFeeds.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if our specific South African agencies exist
            if (context.Agencies.Any(a => a.GtfsAgencyId == "BBS_01"))
            {
                // We assume if BBS_01 exists, the rest of the SA data exists.
                // However, we can check for RRT Express shapes specifically if we want to be granular, 
                // but for now, let's assume one run is enough.
                return;   
            }

            // ==========================================
            // 1. Agencies (North West / Rustenburg Area)
            // ==========================================
            var agencies = new Agency[]
            {
                new Agency { GtfsAgencyId = "BBS_01", AgencyName = "Bojanala Bus Services", AgencyUrl = "http://www.bojanala.co.za", AgencyTimezone = "Africa/Johannesburg", AgencyLang = "en", AgencyPhone = "+27 14 590 0000" },
                new Agency { GtfsAgencyId = "RRT_01", AgencyName = "Rustenburg Rapid Transport", AgencyUrl = "http://www.rustenburg.gov.za", AgencyTimezone = "Africa/Johannesburg", AgencyLang = "en", AgencyPhone = "+27 14 590 3000" }
            };
            // Note: If other agencies exist, we just append. IDs generate automatically.
            context.Agencies.AddRange(agencies);
            context.SaveChanges();

            // ==========================================
            // 2. Calendars
            // ==========================================
            var calendars = new TransitCalendar[]
            {
                // Weekday Service: Mon-Fri
                new TransitCalendar { GtfsServiceId = "WEEKDAY", Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31) },
                // Weekend Service: Sat-Sun
                new TransitCalendar { GtfsServiceId = "WEEKEND", Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = true, Sunday = true, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31) }
            };
            context.TransitCalendars.AddRange(calendars);
            context.SaveChanges();

            // ==========================================
            // 3. Stops (Phokeng & Surrounds)
            // ==========================================
            var stops = new Stop[]
            {
                // Rustenburg CBD
                new Stop { GtfsStopId = "RTB_CBD", StopName = "Rustenburg Taxi Rank", StopDesc = "Main CBD Interchange", StopLat = -25.6685m, StopLon = 27.2424m, LocationType = 0 },
                
                // Tlhabane
                new Stop { GtfsStopId = "TLK_01", StopName = "Tlhabane Forum", StopDesc = "Shopping Centre Stop", StopLat = -25.6450m, StopLon = 27.2200m, LocationType = 0 },

                // Phokeng Entrance / Route R104
                new Stop { GtfsStopId = "PHK_ENT", StopName = "Phokeng Welcome Centre", StopDesc = "R104 Junction", StopLat = -25.6000m, StopLon = 27.2000m, LocationType = 0 },

                // Phokeng Central
                new Stop { GtfsStopId = "PHK_MALL", StopName = "Phokeng Mall", StopDesc = "Central Hub", StopLat = -25.5780m, StopLon = 27.1630m, LocationType = 0 },

                // Stadium
                new Stop { GtfsStopId = "RBS_STAD", StopName = "Royal Bafokeng Stadium", StopDesc = "Stadium Entrance", StopLat = -25.5760m, StopLon = 27.1600m, LocationType = 0 },

                // Luka
                new Stop { GtfsStopId = "LUK_VIL", StopName = "Luka Village", StopDesc = "Community Hall", StopLat = -25.5390m, StopLon = 27.1450m, LocationType = 0 },

                // Chaneng
                new Stop { GtfsStopId = "CHN_TERM", StopName = "Chaneng Terminus", StopDesc = "End of Line", StopLat = -25.4850m, StopLon = 27.1250m, LocationType = 0 }
            };
            context.Stops.AddRange(stops);
            context.SaveChanges();

            // ==========================================
            // 4. Routes
            // ==========================================
            var routes = new TransitRoute[]
            {
                // Route 1: Rustenburg to Chaneng (Long distance)
                new TransitRoute { GtfsRouteId = "RT_101", AgencyId = agencies[0].Id, RouteShortName = "101", RouteLongName = "Rustenburg - Chaneng", RouteDesc = "Via Phokeng & Luka", RouteType = 3, RouteColor = "E11D48", RouteTextColor = "FFFFFF" }, // Red
                
                // Route 2: Phokeng Local Loop (Local)
                new TransitRoute { GtfsRouteId = "RT_102", AgencyId = agencies[0].Id, RouteShortName = "102", RouteLongName = "Phokeng Local Loop", RouteDesc = "Stadium & Mall Circle", RouteType = 3, RouteColor = "2563EB", RouteTextColor = "FFFFFF" }, // Blue

                // Route 3: RRT Express
                new TransitRoute { GtfsRouteId = "RRT_EXP", AgencyId = agencies[1].Id, RouteShortName = "X1", RouteLongName = "City Express", RouteDesc = "Direct CBD to Tlhabane", RouteType = 3, RouteColor = "059669", RouteTextColor = "FFFFFF" } // Green
            };
            context.TransitRoutes.AddRange(routes);
            context.SaveChanges();

            // ==========================================
            // 5. Shapes
            // ==========================================
            
            // Shape for Route 101 (Rustenburg -> Chaneng)
            var shape101 = new ShapesMaster { GtfsShapeId = "SHP_101_OUT" };
            context.ShapesMasters.Add(shape101); // Just add to context, don't save yet if we want batching, but we need Id.
            context.SaveChanges();

            var shapePoints101 = new Shape[]
            {
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 1, ShapePtLat = -25.6685m, ShapePtLon = 27.2424m, ShapeDistTraveled = 0 },    // CBD
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 2, ShapePtLat = -25.6550m, ShapePtLon = 27.2350m, ShapeDistTraveled = 2.5m },  // Leaving CBD
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 3, ShapePtLat = -25.6450m, ShapePtLon = 27.2200m, ShapeDistTraveled = 4.0m },  // Tlhabane
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 4, ShapePtLat = -25.6200m, ShapePtLon = 27.2100m, ShapeDistTraveled = 7.5m },  // Highway R104
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 5, ShapePtLat = -25.6000m, ShapePtLon = 27.2000m, ShapeDistTraveled = 10.0m }, // Phokeng Ent
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 6, ShapePtLat = -25.5850m, ShapePtLon = 27.1800m, ShapeDistTraveled = 13.0m }, // Approaching Central
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 7, ShapePtLat = -25.5780m, ShapePtLon = 27.1630m, ShapeDistTraveled = 15.0m }, // Phokeng Mall
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 8, ShapePtLat = -25.5760m, ShapePtLon = 27.1600m, ShapeDistTraveled = 15.5m }, // Stadium
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 9, ShapePtLat = -25.5500m, ShapePtLon = 27.1500m, ShapeDistTraveled = 19.0m }, // Road to Luka
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 10, ShapePtLat = -25.5390m, ShapePtLon = 27.1450m, ShapeDistTraveled = 21.0m }, // Luka
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 11, ShapePtLat = -25.5100m, ShapePtLon = 27.1350m, ShapeDistTraveled = 25.0m }, // Rural road
                new Shape { ShapeId = shape101.Id, ShapePtSequence = 12, ShapePtLat = -25.4850m, ShapePtLon = 27.1250m, ShapeDistTraveled = 30.0m }  // Chaneng
            };
            context.Shapes.AddRange(shapePoints101);

            // Shape for Route 102 (Phokeng Loop)
            var shape102 = new ShapesMaster { GtfsShapeId = "SHP_102_LOOP" };
            context.ShapesMasters.Add(shape102);
            context.SaveChanges(); 

            var shapePoints102 = new Shape[]
            {
                new Shape { ShapeId = shape102.Id, ShapePtSequence = 1, ShapePtLat = -25.5780m, ShapePtLon = 27.1630m, ShapeDistTraveled = 0 },     // Mall start
                new Shape { ShapeId = shape102.Id, ShapePtSequence = 2, ShapePtLat = -25.5760m, ShapePtLon = 27.1600m, ShapeDistTraveled = 0.5m },  // Stadium
                new Shape { ShapeId = shape102.Id, ShapePtSequence = 3, ShapePtLat = -25.5700m, ShapePtLon = 27.1550m, ShapeDistTraveled = 1.2m },  // North side
                new Shape { ShapeId = shape102.Id, ShapePtSequence = 4, ShapePtLat = -25.5650m, ShapePtLon = 27.1700m, ShapeDistTraveled = 2.5m },  // East side
                new Shape { ShapeId = shape102.Id, ShapePtSequence = 5, ShapePtLat = -25.5780m, ShapePtLon = 27.1630m, ShapeDistTraveled = 3.5m }   // Mall end
            };
            context.Shapes.AddRange(shapePoints102);

            // Shape for RRT Express (X1)
            var shapeX1 = new ShapesMaster { GtfsShapeId = "SHP_X1_EXP" };
            context.ShapesMasters.Add(shapeX1);
            context.SaveChanges();

            var shapePointsX1 = new Shape[]
            {
                new Shape { ShapeId = shapeX1.Id, ShapePtSequence = 1, ShapePtLat = -25.6685m, ShapePtLon = 27.2424m, ShapeDistTraveled = 0 },   // CBD
                new Shape { ShapeId = shapeX1.Id, ShapePtSequence = 2, ShapePtLat = -25.6600m, ShapePtLon = 27.2400m, ShapeDistTraveled = 1.0m }, // North from CBD
                new Shape { ShapeId = shapeX1.Id, ShapePtSequence = 3, ShapePtLat = -25.6500m, ShapePtLon = 27.2300m, ShapeDistTraveled = 3.0m }, // RRT Lane
                new Shape { ShapeId = shapeX1.Id, ShapePtSequence = 4, ShapePtLat = -25.6450m, ShapePtLon = 27.2200m, ShapeDistTraveled = 4.5m }, // Tlhabane Stop
                new Shape { ShapeId = shapeX1.Id, ShapePtSequence = 5, ShapePtLat = -25.6400m, ShapePtLon = 27.2150m, ShapeDistTraveled = 5.5m }  // Tlhabane West
            };
            context.Shapes.AddRange(shapePointsX1);

            context.SaveChanges();


            // ==========================================
            // 6. Trips
            // ==========================================
            var trips = new Trip[]
            {
                // 101 AM Trip
                new Trip { GtfsTripId = "T_101_AM", TransitRouteId = routes[0].Id, ServiceId = calendars[0].Id, ShapeId = shape101.Id, TripHeadsign = "Chaneng", DirectionId = 0, BlockId = "BLK_101" },
                // 102 Loop Trip
                new Trip { GtfsTripId = "T_102_LOOP", TransitRouteId = routes[1].Id, ServiceId = calendars[0].Id, ShapeId = shape102.Id, TripHeadsign = "Phokeng Loop", DirectionId = 0, BlockId = "BLK_102" },
                // X1 Express
                new Trip { GtfsTripId = "T_X1_EXP_AM", TransitRouteId = routes[2].Id, ServiceId = calendars[0].Id, ShapeId = shapeX1.Id, TripHeadsign = "Tlhabane West", DirectionId = 0, BlockId = "BLK_X1" }
            };
            context.Trips.AddRange(trips);
            context.SaveChanges();

            // ==========================================
            // 7. Stop Times
            // ==========================================
            var stopTimes = new StopTime[]
            {
                // Trip 101 (The long one)
                new StopTime { TripId = trips[0].Id, StopId = stops[0].Id, StopSequence = 1, ArrivalTime = new TimeSpan(6, 0, 0), DepartureTime = new TimeSpan(6, 15, 0), StopHeadsign = "Chaneng" }, // Rustenburg
                new StopTime { TripId = trips[0].Id, StopId = stops[1].Id, StopSequence = 2, ArrivalTime = new TimeSpan(6, 30, 0), DepartureTime = new TimeSpan(6, 30, 0), StopHeadsign = "Chaneng" }, // Tlhabane
                new StopTime { TripId = trips[0].Id, StopId = stops[3].Id, StopSequence = 3, ArrivalTime = new TimeSpan(6, 50, 0), DepartureTime = new TimeSpan(6, 55, 0), StopHeadsign = "Chaneng" }, // Phokeng Mall
                new StopTime { TripId = trips[0].Id, StopId = stops[5].Id, StopSequence = 4, ArrivalTime = new TimeSpan(7, 10, 0), DepartureTime = new TimeSpan(7, 10, 0), StopHeadsign = "Chaneng" }, // Luka
                new StopTime { TripId = trips[0].Id, StopId = stops[6].Id, StopSequence = 5, ArrivalTime = new TimeSpan(7, 30, 0), DepartureTime = new TimeSpan(7, 30, 0), StopHeadsign = "Chaneng" }, // Chaneng

                // Trip 102 (The Loop)
                new StopTime { TripId = trips[1].Id, StopId = stops[3].Id, StopSequence = 1, ArrivalTime = new TimeSpan(8, 0, 0), DepartureTime = new TimeSpan(8, 0, 0), StopHeadsign = "Loop" }, // Mall
                new StopTime { TripId = trips[1].Id, StopId = stops[4].Id, StopSequence = 2, ArrivalTime = new TimeSpan(8, 05, 0), DepartureTime = new TimeSpan(8, 05, 0), StopHeadsign = "Loop" }, // Stadium
                new StopTime { TripId = trips[1].Id, StopId = stops[3].Id, StopSequence = 3, ArrivalTime = new TimeSpan(8, 20, 0), DepartureTime = new TimeSpan(8, 20, 0), StopHeadsign = "Loop" },  // Mall

                // Trip X1 (RRT Express)
                new StopTime { TripId = trips[2].Id, StopId = stops[0].Id, StopSequence = 1, ArrivalTime = new TimeSpan(7, 0, 0), DepartureTime = new TimeSpan(7, 05, 0), StopHeadsign = "Tlhabane" }, // CBD
                new StopTime { TripId = trips[2].Id, StopId = stops[1].Id, StopSequence = 2, ArrivalTime = new TimeSpan(7, 20, 0), DepartureTime = new TimeSpan(7, 20, 0), StopHeadsign = "Tlhabane" }  // Tlhabane
            };
            context.StopTimes.AddRange(stopTimes);
            context.SaveChanges();
        }
    }
}

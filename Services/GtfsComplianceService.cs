using System;
using System.Collections.Generic;
using System.Linq;
using TransitFeeds.Models;

namespace TransitFeeds.Services
{
    public class GtfsComplianceService
    {
        /// <summary>
        /// Gets the effective continuous pickup value for a specific stop time,
        /// handling the override logic between routes and stop times.
        /// </summary>
        public ContinuousStopping GetEffectiveContinuousPickup(StopTime stopTime)
        {
            if (stopTime == null) return ContinuousStopping.NoContinuous;

            // stop_times.txt override
            if (stopTime.ContinuousPickup.HasValue)
            {
                return stopTime.ContinuousPickup.Value;
            }

            // Fallback to routes.txt
            if (stopTime.Trip?.TransitRoute?.ContinuousPickup.HasValue == true)
            {
                return stopTime.Trip.TransitRoute.ContinuousPickup.Value;
            }

            // Default
            return ContinuousStopping.NoContinuous;
        }

        /// <summary>
        /// Gets the effective continuous drop-off value.
        /// </summary>
        public ContinuousStopping GetEffectiveContinuousDropOff(StopTime stopTime)
        {
            if (stopTime == null) return ContinuousStopping.NoContinuous;

            if (stopTime.ContinuousDropOff.HasValue)
            {
                return stopTime.ContinuousDropOff.Value;
            }

            if (stopTime.Trip?.TransitRoute?.ContinuousDropOff.HasValue == true)
            {
                return stopTime.Trip.TransitRoute.ContinuousDropOff.Value;
            }

            return ContinuousStopping.NoContinuous;
        }

        /// <summary>
        /// Validates a route based on GTFS rules.
        /// </summary>
        public List<string> ValidateRoute(TransitRoute route)
        {
            var errors = new List<string>();
            if (string.IsNullOrEmpty(route.RouteShortName) && string.IsNullOrEmpty(route.RouteLongName))
            {
                errors.Add("Route must have either a short name or a long name.");
            }
            return errors;
        }

        /// <summary>
        /// Validates stop coordinates.
        /// </summary>
        public List<string> ValidateStop(Stop stop)
        {
            var errors = new List<string>();
            if (stop.StopLat < -90 || stop.StopLat > 90) errors.Add("Latitude must be between -90 and 90.");
            if (stop.StopLon < -180 || stop.StopLon > 180) errors.Add("Longitude must be between -180 and 180.");
            return errors;
        }
    }
}

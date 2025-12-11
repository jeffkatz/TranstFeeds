using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace TransitFeeds.Migrations
{
    public partial class SeedSouthAfricanTransitData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Agencies (Transit Authorities)
            migrationBuilder.InsertData(
                table: "Agencies",
                columns: new[] { "gtfs_agency_id", "agency_name", "agency_url", "agency_timezone", "agency_phone", "agency_lang" },
                values: new object[,]
                {
                    { "BPLD", "Bojanala Platinum District Transport", "https://bojanala.gov.za", "Africa/Johannesburg", "+27 14 590 4500", "en" },
                    { "RLM", "Rustenburg Local Municipality Transport", "https://rustenburg.gov.za", "Africa/Johannesburg", "+27 14 590 3111", "en" },
                    { "MLM", "Madibeng Local Municipality Transport", "https://madibeng.gov.za", "Africa/Johannesburg", "+27 12 318 9300", "en" },
                    { "MORLM", "Moretele Local Municipality Transport", "https://moretele.gov.za", "Africa/Johannesburg", "+27 12 716 0300", "en" },
                    { "DKKD", "Dr Kenneth Kaunda District Transport", "https://drkennethkaunda.gov.za", "Africa/Johannesburg", "+27 18 487 8000", "en" },
                    { "JBMLM", "JB Marks Local Municipality Transport", "https://jbmarks.gov.za", "Africa/Johannesburg", "+27 18 294 2000", "en" },
                    { "CMLM", "City of Matlosana Transport", "https://matlosana.gov.za", "Africa/Johannesburg", "+27 18 487 8000", "en" },
                    { "NMMD", "Ngaka Modiri Molema District Transport", "https://nmm.gov.za", "Africa/Johannesburg", "+27 18 381 8300", "en" },
                    { "MHLM", "Mahikeng Local Municipality Transport", "https://mahikeng.gov.za", "Africa/Johannesburg", "+27 18 381 8300", "en" },
                    { "DTSLM", "Ditsobotla Local Municipality Transport", "https://ditsobotla.gov.za", "Africa/Johannesburg", "+27 18 381 8300", "en" },
                    { "DRSMD", "Dr Ruth Segomotsi Mompati District Transport", "https://drrsm.gov.za", "Africa/Johannesburg", "+27 53 927 0500", "en" },
                    { "GTLM", "Greater Taung Local Municipality Transport", "https://taung.gov.za", "Africa/Johannesburg", "+27 53 994 8300", "en" },
                    { "KMLM", "Kagisano-Molopo Local Municipality Transport", "https://kagisano.gov.za", "Africa/Johannesburg", "+27 53 927 0500", "en" }
                });

            // Seed Transit Calendar (Service patterns)
            migrationBuilder.InsertData(
                table: "TransitCalendar",
                columns: new[] { "gtfs_service_id", "start_date", "end_date", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" },
                values: new object[,]
                {
                    { "WEEKDAY", new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), true, true, true, true, true, false, false },
                    { "WEEKEND", new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), false, false, false, false, false, true, true },
                    { "DAILY", new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), true, true, true, true, true, true, true }
                });

            // Note: We'll need to get the auto-generated IDs for agencies and calendars to create routes
            // This is a simplified version - in a real scenario, you'd use raw SQL or a different approach
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM StopTimes");
            migrationBuilder.Sql("DELETE FROM Trips");
            migrationBuilder.Sql("DELETE FROM Stops");
            migrationBuilder.Sql("DELETE FROM TransitRoutes");
            migrationBuilder.Sql("DELETE FROM TransitCalendar");
            migrationBuilder.Sql("DELETE FROM Agencies");
        }
    }
}

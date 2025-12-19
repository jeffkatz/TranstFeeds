using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranstFeeds.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarDatesAndTimeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarDates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_service_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    exception_type = table.Column<byte>(type: "tinyint", nullable: false),
                    transit_calendar_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarDates", x => x.id);
                    table.ForeignKey(
                        name: "FK_CalendarDates_TransitCalendar_transit_calendar_id",
                        column: x => x.transit_calendar_id,
                        principalTable: "TransitCalendar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDates_transit_calendar_id",
                table: "CalendarDates",
                column: "transit_calendar_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarDates");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranstFeeds.Migrations
{
    /// <inheritdoc />
    public partial class Phase4SpecCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "direction_id",
                table: "Trips",
                type: "int",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "bikes_allowed",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "continuous_drop_off",
                table: "TransitRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "continuous_pickup",
                table: "TransitRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "route_sort_order",
                table: "TransitRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "continuous_drop_off",
                table: "StopTimes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "continuous_pickup",
                table: "StopTimes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "timepoint",
                table: "StopTimes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "platform_code",
                table: "Stops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tts_stop_name",
                table: "Stops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "exception_type",
                table: "CalendarDates",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.CreateTable(
                name: "FeedInfo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feed_publisher_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    feed_publisher_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    feed_lang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    feed_start_date = table.Column<DateTime>(type: "date", nullable: true),
                    feed_end_date = table.Column<DateTime>(type: "date", nullable: true),
                    feed_version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    feed_contact_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    feed_contact_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedInfo", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedInfo");

            migrationBuilder.DropColumn(
                name: "bikes_allowed",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "continuous_drop_off",
                table: "TransitRoutes");

            migrationBuilder.DropColumn(
                name: "continuous_pickup",
                table: "TransitRoutes");

            migrationBuilder.DropColumn(
                name: "route_sort_order",
                table: "TransitRoutes");

            migrationBuilder.DropColumn(
                name: "continuous_drop_off",
                table: "StopTimes");

            migrationBuilder.DropColumn(
                name: "continuous_pickup",
                table: "StopTimes");

            migrationBuilder.DropColumn(
                name: "timepoint",
                table: "StopTimes");

            migrationBuilder.DropColumn(
                name: "platform_code",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "tts_stop_name",
                table: "Stops");

            migrationBuilder.AlterColumn<byte>(
                name: "direction_id",
                table: "Trips",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "exception_type",
                table: "CalendarDates",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

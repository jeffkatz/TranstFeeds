using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitFeeds.Migrations
{
    public partial class RefactorIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing tables to rebuild schema cleanly
            migrationBuilder.DropTable(name: "StopTimes");
            migrationBuilder.DropTable(name: "Trips");
            migrationBuilder.DropTable(name: "Shapes");
            migrationBuilder.DropTable(name: "ShapesMaster");
            migrationBuilder.DropTable(name: "TransitRoutes");
            migrationBuilder.DropTable(name: "Stops");
            migrationBuilder.DropTable(name: "TransitCalendar");
            migrationBuilder.DropTable(name: "Agencies");

            // Recreate Agencies
            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_agency_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    agency_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    agency_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    agency_timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    agency_phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    agency_lang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.id);
                });

            // Recreate TransitCalendar
            migrationBuilder.CreateTable(
                name: "TransitCalendar",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_service_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    monday = table.Column<bool>(type: "bit", nullable: false),
                    tuesday = table.Column<bool>(type: "bit", nullable: false),
                    wednesday = table.Column<bool>(type: "bit", nullable: false),
                    thursday = table.Column<bool>(type: "bit", nullable: false),
                    friday = table.Column<bool>(type: "bit", nullable: false),
                    saturday = table.Column<bool>(type: "bit", nullable: false),
                    sunday = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitCalendar", x => x.id);
                });

            // Recreate ShapesMaster
            migrationBuilder.CreateTable(
                name: "ShapesMaster",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_shape_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShapesMaster", x => x.id);
                });

            // Recreate Shapes
            migrationBuilder.CreateTable(
                name: "Shapes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shape_id = table.Column<int>(type: "int", nullable: false),
                    shape_pt_sequence = table.Column<int>(type: "int", nullable: false),
                    shape_pt_lat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    shape_pt_lon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    shape_dist_traveled = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shapes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Shapes_ShapesMaster_shape_id",
                        column: x => x.shape_id,
                        principalTable: "ShapesMaster",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Recreate Stops
            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_stop_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    stop_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stop_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    stop_desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stop_lat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    stop_lon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    zone_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stop_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location_type = table.Column<byte>(type: "tinyint", nullable: true),
                    wheelchair_boarding = table.Column<byte>(type: "tinyint", nullable: true),
                    parent_station_id = table.Column<int>(type: "int", nullable: true),
                    stop_timezone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.id);
                    table.ForeignKey(
                        name: "FK_Stops_Stops_parent_station_id",
                        column: x => x.parent_station_id,
                        principalTable: "Stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Recreate TransitRoutes
            migrationBuilder.CreateTable(
                name: "TransitRoutes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_route_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    agency_id = table.Column<int>(type: "int", nullable: true),
                    route_short_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    route_long_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    route_type = table.Column<int>(type: "int", nullable: true),
                    route_text_color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    route_color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    route_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    route_desc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitRoutes", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransitRoutes_Agencies_agency_id",
                        column: x => x.agency_id,
                        principalTable: "Agencies",
                        principalColumn: "id");
                });

            // Recreate Trips
            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gtfs_trip_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    transit_route_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<int>(type: "int", nullable: false),
                    shape_id = table.Column<int>(type: "int", nullable: true),
                    trip_headsign = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    trip_short_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    direction_id = table.Column<byte>(type: "tinyint", nullable: true),
                    wheelchair_accessible = table.Column<byte>(type: "tinyint", nullable: true),
                    block_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.id);
                    table.ForeignKey(
                        name: "FK_Trips_ShapesMaster_shape_id",
                        column: x => x.shape_id,
                        principalTable: "ShapesMaster",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Trips_TransitCalendar_service_id",
                        column: x => x.service_id,
                        principalTable: "TransitCalendar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trips_TransitRoutes_transit_route_id",
                        column: x => x.transit_route_id,
                        principalTable: "TransitRoutes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Recreate StopTimes
            migrationBuilder.CreateTable(
                name: "StopTimes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    trip_id = table.Column<int>(type: "int", nullable: false),
                    stop_id = table.Column<int>(type: "int", nullable: false),
                    stop_sequence = table.Column<int>(type: "int", nullable: false),
                    arrival_time = table.Column<string>(type: "varchar(8)", nullable: true),
                    departure_time = table.Column<string>(type: "varchar(8)", nullable: true),
                    stop_headsign = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    pickup_type = table.Column<byte>(type: "tinyint", nullable: true),
                    drop_off_type = table.Column<byte>(type: "tinyint", nullable: true),
                    shape_dist_traveled = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopTimes", x => x.id);
                    table.ForeignKey(
                        name: "FK_StopTimes_Stops_stop_id",
                        column: x => x.stop_id,
                        principalTable: "Stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StopTimes_Trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "Trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Shapes_shape_id",
                table: "Shapes",
                column: "shape_id");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_parent_station_id",
                table: "Stops",
                column: "parent_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_stop_id",
                table: "StopTimes",
                column: "stop_id");

            migrationBuilder.CreateIndex(
                name: "IX_StopTimes_trip_id",
                table: "StopTimes",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransitRoutes_agency_id",
                table: "TransitRoutes",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_service_id",
                table: "Trips",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_shape_id",
                table: "Trips",
                column: "shape_id");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_transit_route_id",
                table: "Trips",
                column: "transit_route_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "StopTimes");
            migrationBuilder.DropTable(name: "Trips");
            migrationBuilder.DropTable(name: "Shapes");
            migrationBuilder.DropTable(name: "ShapesMaster");
            migrationBuilder.DropTable(name: "TransitRoutes");
            migrationBuilder.DropTable(name: "Stops");
            migrationBuilder.DropTable(name: "TransitCalendar");
            migrationBuilder.DropTable(name: "Agencies");
        }
    }
}

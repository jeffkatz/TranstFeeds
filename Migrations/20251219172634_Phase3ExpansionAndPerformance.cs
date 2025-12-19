using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranstFeeds.Migrations
{
    /// <inheritdoc />
    public partial class Phase3ExpansionAndPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Frequencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    trip_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<string>(type: "varchar(10)", nullable: false),
                    end_time = table.Column<string>(type: "varchar(10)", nullable: false),
                    headway_secs = table.Column<int>(type: "int", nullable: false),
                    exact_times = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frequencies", x => x.id);
                    table.ForeignKey(
                        name: "FK_Frequencies_Trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "Trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    from_stop_id = table.Column<int>(type: "int", nullable: false),
                    to_stop_id = table.Column<int>(type: "int", nullable: false),
                    transfer_type = table.Column<byte>(type: "tinyint", nullable: false),
                    min_transfer_time = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.id);
                    table.ForeignKey(
                        name: "FK_Transfers_Stops_from_stop_id",
                        column: x => x.from_stop_id,
                        principalTable: "Stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Stops_to_stop_id",
                        column: x => x.to_stop_id,
                        principalTable: "Stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Frequencies_trip_id",
                table: "Frequencies",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_from_stop_id",
                table: "Transfers",
                column: "from_stop_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_to_stop_id",
                table: "Transfers",
                column: "to_stop_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Frequencies");

            migrationBuilder.DropTable(
                name: "Transfers");
        }
    }
}

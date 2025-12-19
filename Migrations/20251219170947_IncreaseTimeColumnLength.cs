using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranstFeeds.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseTimeColumnLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "departure_time",
                table: "StopTimes",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(8)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "arrival_time",
                table: "StopTimes",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(8)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "departure_time",
                table: "StopTimes",
                type: "varchar(8)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "arrival_time",
                table: "StopTimes",
                type: "varchar(8)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);
        }
    }
}

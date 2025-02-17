using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class DenormalisedPathway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PathwayName",
                table: "PathwayTypeAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScreeningName",
                table: "PathwayTypeAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathwayName",
                table: "PathwayTypeAssignments");

            migrationBuilder.DropColumn(
                name: "ScreeningName",
                table: "PathwayTypeAssignments");
        }
    }
}

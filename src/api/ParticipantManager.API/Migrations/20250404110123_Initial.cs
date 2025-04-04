using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DOB = table.Column<DateOnly>(type: "date", nullable: false),
                    NhsNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantId);
                });

            migrationBuilder.CreateTable(
                name: "PathwayTypeEnrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathwayTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrolmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LapsedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextActionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ScreeningName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathwayTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathwayTypeEnrolments", x => x.EnrolmentId);
                    table.ForeignKey(
                        name: "FK_PathwayTypeEnrolments_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "ParticipantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrolmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathwayVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathwayTypeEnrolmentEnrolmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.EpisodeId);
                    table.ForeignKey(
                        name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                        column: x => x.PathwayTypeEnrolmentEnrolmentId,
                        principalTable: "PathwayTypeEnrolments",
                        principalColumn: "EnrolmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                column: "PathwayTypeEnrolmentEnrolmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PathwayTypeEnrolments_ParticipantId",
                table: "PathwayTypeEnrolments",
                column: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "PathwayTypeEnrolments");

            migrationBuilder.DropTable(
                name: "Participants");
        }
    }
}

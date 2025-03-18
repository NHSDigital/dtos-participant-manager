using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class Renamed_Assignments_To_Enrolments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeEnrollments_PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes");

            migrationBuilder.DropTable(
                name: "PathwayTypeEnrollments");

            migrationBuilder.RenameColumn(
                name: "PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes",
                newName: "PathwayTypeEnrolmentEnrolmentId");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                table: "Episodes",
                newName: "EnrolmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes",
                newName: "IX_Episodes_PathwayTypeEnrolmentEnrolmentId");

            migrationBuilder.CreateTable(
                name: "PathwayTypeEnrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathwayTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrolmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LapsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_PathwayTypeEnrolments_ParticipantId",
                table: "PathwayTypeEnrolments",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                column: "PathwayTypeEnrolmentEnrolmentId",
                principalTable: "PathwayTypeEnrolments",
                principalColumn: "EnrolmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes");

            migrationBuilder.DropTable(
                name: "PathwayTypeEnrolments");

            migrationBuilder.RenameColumn(
                name: "PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                newName: "PathwayTypeEnrollmentEnrollmentId");

            migrationBuilder.RenameColumn(
                name: "EnrolmentId",
                table: "Episodes",
                newName: "EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                newName: "IX_Episodes_PathwayTypeEnrollmentEnrollmentId");

            migrationBuilder.CreateTable(
                name: "PathwayTypeEnrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LapsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PathwayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathwayTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScreeningName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathwayTypeEnrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_PathwayTypeEnrollments_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "ParticipantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PathwayTypeEnrollments_ParticipantId",
                table: "PathwayTypeEnrollments",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeEnrollments_PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes",
                column: "PathwayTypeEnrollmentEnrollmentId",
                principalTable: "PathwayTypeEnrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

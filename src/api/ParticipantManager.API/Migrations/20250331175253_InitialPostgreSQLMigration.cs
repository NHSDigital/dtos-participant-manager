using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DOB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NhsNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantId);
                });

            migrationBuilder.CreateTable(
                name: "PathwayTypeEnrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathwayTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrolmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LapsedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    NextActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScreeningName = table.Column<string>(type: "text", nullable: false),
                    PathwayTypeName = table.Column<string>(type: "text", nullable: false)
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
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrolmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathwayVersion = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PathwayTypeEnrolmentEnrolmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.EpisodeId);
                    table.ForeignKey(
                        name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmen~",
                        column: x => x.PathwayTypeEnrolmentEnrolmentId,
                        principalTable: "PathwayTypeEnrolments",
                        principalColumn: "EnrolmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Outcome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.EncounterId);
                    table.ForeignKey(
                        name: "FK_Encounters_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "EpisodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_EpisodeId",
                table: "Encounters",
                column: "EpisodeId");

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
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "PathwayTypeEnrolments");

            migrationBuilder.DropTable(
                name: "Participants");
        }
    }
}

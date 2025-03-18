using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class renamedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeAssignments_PathwayTypeAssignmentEnrollmentId",
                table: "Episodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PathwayTypeAssignments_Participants_ParticipantId",
                table: "PathwayTypeAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PathwayTypeAssignments",
                table: "PathwayTypeAssignments");

            migrationBuilder.RenameTable(
                name: "PathwayTypeAssignments",
                newName: "PathwayTypeEnrollments");

            migrationBuilder.RenameColumn(
                name: "PathwayTypeAssignmentEnrollmentId",
                table: "Episodes",
                newName: "PathwayTypeEnrollmentEnrollmentId");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "Episodes",
                newName: "EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_PathwayTypeAssignmentEnrollmentId",
                table: "Episodes",
                newName: "IX_Episodes_PathwayTypeEnrollmentEnrollmentId");

            migrationBuilder.RenameColumn(
                name: "AssignmentDate",
                table: "PathwayTypeEnrollments",
                newName: "EnrollmentDate");

            migrationBuilder.RenameIndex(
                name: "IX_PathwayTypeAssignments_ParticipantId",
                table: "PathwayTypeEnrollments",
                newName: "IX_PathwayTypeEnrollments_ParticipantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PathwayTypeEnrollments",
                table: "PathwayTypeEnrollments",
                column: "EnrollmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeEnrollments_PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes",
                column: "PathwayTypeEnrollmentEnrollmentId",
                principalTable: "PathwayTypeEnrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PathwayTypeEnrollments_Participants_ParticipantId",
                table: "PathwayTypeEnrollments",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "ParticipantId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeEnrollments_PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PathwayTypeEnrollments_Participants_ParticipantId",
                table: "PathwayTypeEnrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PathwayTypeEnrollments",
                table: "PathwayTypeEnrollments");

            migrationBuilder.RenameTable(
                name: "PathwayTypeEnrollments",
                newName: "PathwayTypeAssignments");

            migrationBuilder.RenameColumn(
                name: "PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes",
                newName: "PathwayTypeAssignmentEnrollmentId");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                table: "Episodes",
                newName: "AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_PathwayTypeEnrollmentEnrollmentId",
                table: "Episodes",
                newName: "IX_Episodes_PathwayTypeAssignmentEnrollmentId");

            migrationBuilder.RenameColumn(
                name: "EnrollmentDate",
                table: "PathwayTypeAssignments",
                newName: "AssignmentDate");

            migrationBuilder.RenameIndex(
                name: "IX_PathwayTypeEnrollments_ParticipantId",
                table: "PathwayTypeAssignments",
                newName: "IX_PathwayTypeAssignments_ParticipantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PathwayTypeAssignments",
                table: "PathwayTypeAssignments",
                column: "EnrollmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeAssignments_PathwayTypeAssignmentEnrollmentId",
                table: "Episodes",
                column: "PathwayTypeAssignmentEnrollmentId",
                principalTable: "PathwayTypeAssignments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PathwayTypeAssignments_Participants_ParticipantId",
                table: "PathwayTypeAssignments",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "ParticipantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

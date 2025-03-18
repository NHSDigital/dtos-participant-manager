using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class refactorPathwayTypeAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeAssignments_PathwayTypeAssignmentAssignmentId",
                table: "Episodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PathwayTypeAssignments",
                table: "PathwayTypeAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "PathwayTypeAssignments");

            migrationBuilder.RenameColumn(
                name: "PathwayId",
                table: "PathwayTypeAssignments",
                newName: "EnrollmentId");

            migrationBuilder.RenameColumn(
                name: "PathwayTypeAssignmentAssignmentId",
                table: "Episodes",
                newName: "PathwayTypeAssignmentEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_PathwayTypeAssignmentAssignmentId",
                table: "Episodes",
                newName: "IX_Episodes_PathwayTypeAssignmentEnrollmentId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeAssignments_PathwayTypeAssignmentEnrollmentId",
                table: "Episodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PathwayTypeAssignments",
                table: "PathwayTypeAssignments");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                table: "PathwayTypeAssignments",
                newName: "PathwayId");

            migrationBuilder.RenameColumn(
                name: "PathwayTypeAssignmentEnrollmentId",
                table: "Episodes",
                newName: "PathwayTypeAssignmentAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_PathwayTypeAssignmentEnrollmentId",
                table: "Episodes",
                newName: "IX_Episodes_PathwayTypeAssignmentAssignmentId");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "PathwayTypeAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PathwayTypeAssignments",
                table: "PathwayTypeAssignments",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeAssignments_PathwayTypeAssignmentAssignmentId",
                table: "Episodes",
                column: "PathwayTypeAssignmentAssignmentId",
                principalTable: "PathwayTypeAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParticipantManager.API.Migrations
{
    /// <inheritdoc />
    public partial class updatingsomepropertiestoberequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PathwayVersion",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Outcome",
                table: "Encounters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                column: "PathwayTypeEnrolmentEnrolmentId",
                principalTable: "PathwayTypeEnrolments",
                principalColumn: "EnrolmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PathwayVersion",
                table: "Episodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Outcome",
                table: "Encounters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_PathwayTypeEnrolments_PathwayTypeEnrolmentEnrolmentId",
                table: "Episodes",
                column: "PathwayTypeEnrolmentEnrolmentId",
                principalTable: "PathwayTypeEnrolments",
                principalColumn: "EnrolmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLifelog.Migrations
{
    public partial class AddLogFileInfoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LogFileInfoId",
                table: "LocationHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LogFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationHistories_LogFileInfoId",
                table: "LocationHistories",
                column: "LogFileInfoId");

            migrationBuilder.Sql($"INSERT INTO LogFiles (Created) VALUES ('{DateTime.MinValue.ToString("O").Replace('T', ' ')}')");
            migrationBuilder.Sql($"UPDATE LocationHistories SET LogFileInfoId = 1 WHERE LogFileInfoId = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationHistories_LogFiles_LogFileInfoId",
                table: "LocationHistories",
                column: "LogFileInfoId",
                principalTable: "LogFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationHistories_LogFiles_LogFileInfoId",
                table: "LocationHistories");

            migrationBuilder.DropTable(
                name: "LogFiles");

            migrationBuilder.DropIndex(
                name: "IX_LocationHistories_LogFileInfoId",
                table: "LocationHistories");

            migrationBuilder.DropColumn(
                name: "LogFileInfoId",
                table: "LocationHistories");
        }
    }
}

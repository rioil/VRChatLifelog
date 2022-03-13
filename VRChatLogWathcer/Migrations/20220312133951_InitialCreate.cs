using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JoinLeaveHistories",
                columns: table => new
                {
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    Joined = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Left = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsLocal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinLeaveHistories", x => new { x.PlayerName, x.Joined });
                });

            migrationBuilder.CreateTable(
                name: "LocationHistories",
                columns: table => new
                {
                    WorldId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Joined = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Left = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHistories", x => new { x.WorldId, x.Joined });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JoinLeaveHistories");

            migrationBuilder.DropTable(
                name: "LocationHistories");
        }
    }
}

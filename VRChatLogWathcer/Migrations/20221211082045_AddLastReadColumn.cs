using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    public partial class AddLastReadColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastRead",
                table: "LogFiles",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "LogFiles");
        }
    }
}

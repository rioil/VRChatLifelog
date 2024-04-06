using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLifelog.Migrations
{
    public partial class AddInstanceDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                table: "LocationHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MasterId",
                table: "LocationHistories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "LocationHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "LocationHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "LocationHistories");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "LocationHistories");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "LocationHistories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "LocationHistories");
        }
    }
}

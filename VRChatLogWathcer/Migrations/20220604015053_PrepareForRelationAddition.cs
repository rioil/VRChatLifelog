using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    public partial class PrepareForRelationAddition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "LocationHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LocaionId",
                table: "JoinLeaveHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "LocationHistories");

            migrationBuilder.DropColumn(
                name: "LocaionId",
                table: "JoinLeaveHistories");
        }
    }
}

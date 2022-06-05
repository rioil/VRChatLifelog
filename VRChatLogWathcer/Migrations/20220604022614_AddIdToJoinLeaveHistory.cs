using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    public partial class AddIdToJoinLeaveHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "JoinLeaveHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "JoinLeaveHistories");
        }
    }
}

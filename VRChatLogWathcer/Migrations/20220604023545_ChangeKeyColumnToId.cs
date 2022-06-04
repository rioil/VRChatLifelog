using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    public partial class ChangeKeyColumnToId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JoinLeaveHistories_PlayerName_Joined",
                table: "JoinLeaveHistories",
                columns: new[] { "PlayerName", "Joined" },
                unique: true);
            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinLeaveHistories",
                table: "JoinLeaveHistories");
            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinLeaveHistories",
                table: "JoinLeaveHistories",
                column: "Id");
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationHistories",
                table: "LocationHistories");
            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationHistories",
                table: "LocationHistories",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JoinLeaveHistories_PlayerName_Joined",
                table: "JoinLeaveHistories");
            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinLeaveHistories",
                table: "JoinLeaveHistories");
            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinLeaveHistories",
                table: "JoinLeaveHistories",
                columns: new string[] { "PlayerName", "Joined" });
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationHistories",
                table: "LocationHistories");
            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationHistories",
                table: "LocationHistories",
                columns: new string[] { "WorldId", "Joined" });
        }
    }
}

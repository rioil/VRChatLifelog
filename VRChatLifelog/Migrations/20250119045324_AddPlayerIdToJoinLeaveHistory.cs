using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLifelog.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerIdToJoinLeaveHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerId",
                table: "JoinLeaveHistories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "JoinLeaveHistories");
        }
    }
}

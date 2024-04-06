using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLifelog.Migrations
{
    public partial class AddWorldNameColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorldName",
                table: "LocationHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorldName",
                table: "LocationHistories");
        }
    }
}

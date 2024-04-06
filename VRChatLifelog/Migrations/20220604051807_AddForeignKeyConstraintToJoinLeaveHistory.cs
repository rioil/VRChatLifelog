using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRChatLifelog.Migrations
{
    public partial class AddForeignKeyConstraintToJoinLeaveHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JoinLeaveHistories_PlayerName_Joined",
                table: "JoinLeaveHistories");

            migrationBuilder.RenameColumn(
                name: "LocaionId",
                table: "JoinLeaveHistories",
                newName: "LocationHistoryId");

            migrationBuilder.AlterColumn<string>(
                name: "WorldId",
                table: "LocationHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "LocationHistories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "PlayerName",
                table: "JoinLeaveHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "JoinLeaveHistories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinLeaveHistories_LocationHistoryId",
                table: "JoinLeaveHistories",
                column: "LocationHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_JoinLeaveHistories_LocationHistories_LocationHistoryId",
                table: "JoinLeaveHistories",
                column: "LocationHistoryId",
                principalTable: "LocationHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JoinLeaveHistories_LocationHistories_LocationHistoryId",
                table: "JoinLeaveHistories");

            migrationBuilder.DropIndex(
                name: "IX_JoinLeaveHistories_LocationHistoryId",
                table: "JoinLeaveHistories");

            migrationBuilder.RenameColumn(
                name: "LocationHistoryId",
                table: "JoinLeaveHistories",
                newName: "LocaionId");

            migrationBuilder.AlterColumn<string>(
                name: "WorldId",
                table: "LocationHistories",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "LocationHistories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "PlayerName",
                table: "JoinLeaveHistories",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "JoinLeaveHistories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinLeaveHistories_PlayerName_Joined",
                table: "JoinLeaveHistories",
                columns: new[] { "PlayerName", "Joined" },
                unique: true);
        }
    }
}

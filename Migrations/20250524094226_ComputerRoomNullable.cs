using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ComputerRoomNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_computer_room_room_id",
                table: "computer");

            migrationBuilder.AlterColumn<long>(
                name: "room_id",
                table: "computer",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_computer_room_room_id",
                table: "computer",
                column: "room_id",
                principalTable: "room",
                principalColumn: "room_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_computer_room_room_id",
                table: "computer");

            migrationBuilder.AlterColumn<long>(
                name: "room_id",
                table: "computer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_computer_room_room_id",
                table: "computer",
                column: "room_id",
                principalTable: "room",
                principalColumn: "room_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

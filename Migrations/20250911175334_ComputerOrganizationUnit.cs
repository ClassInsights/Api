using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ComputerOrganizationUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "regex",
                table: "room");

            migrationBuilder.AddColumn<string>(
                name: "organization_unit",
                table: "room",
                type: "character varying(2500)",
                maxLength: 2500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "organization_unit",
                table: "room");

            migrationBuilder.AddColumn<string>(
                name: "regex",
                table: "room",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}

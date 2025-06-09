using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ComputerLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "computer_log",
                columns: table => new
                {
                    computer_log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    computer_id = table.Column<long>(type: "bigint", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    level = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    message = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    details = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_computer_log", x => x.computer_log_id);
                    table.ForeignKey(
                        name: "FK_computer_log_computer_computer_id",
                        column: x => x.computer_id,
                        principalTable: "computer",
                        principalColumn: "computer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_computer_log_computer_id_timestamp",
                table: "computer_log",
                columns: new[] { "computer_id", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "computer_log");
        }
    }
}

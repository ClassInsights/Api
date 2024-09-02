using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tabClass",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Head = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AzureGroupID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabClasses_1", x => x.ClassID);
                });

            migrationBuilder.CreateTable(
                name: "tabLog",
                columns: table => new
                {
                    LogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabLog", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "tabRoom",
                columns: table => new
                {
                    RoomID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LongName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabRoom", x => x.RoomID);
                });

            migrationBuilder.CreateTable(
                name: "tabSubject",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LongName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabSubjects", x => x.SubjectID);
                });

            migrationBuilder.CreateTable(
                name: "tabUser",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AzureUserID = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabUsers", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "tabComputer",
                columns: table => new
                {
                    ComputerID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MacAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUser = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabComputers", x => x.ComputerID);
                    table.ForeignKey(
                        name: "FK_tabComputers_tabRooms",
                        column: x => x.RoomID,
                        principalTable: "tabRoom",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tabLesson",
                columns: table => new
                {
                    LessonID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomID = table.Column<int>(type: "integer", nullable: false),
                    SubjectID = table.Column<int>(type: "integer", nullable: false),
                    ClassID = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabLessons_1", x => x.LessonID);
                    table.ForeignKey(
                        name: "FK_tabLessons_tabClasses",
                        column: x => x.ClassID,
                        principalTable: "tabClass",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tabLessons_tabRooms",
                        column: x => x.RoomID,
                        principalTable: "tabRoom",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tabLessons_tabSubjects",
                        column: x => x.SubjectID,
                        principalTable: "tabSubject",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tabComputer_RoomID",
                table: "tabComputer",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_tabLesson_ClassID",
                table: "tabLesson",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_tabLesson_RoomID",
                table: "tabLesson",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_tabLesson_SubjectID",
                table: "tabLesson",
                column: "SubjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tabComputer");

            migrationBuilder.DropTable(
                name: "tabLesson");

            migrationBuilder.DropTable(
                name: "tabLog");

            migrationBuilder.DropTable(
                name: "tabUser");

            migrationBuilder.DropTable(
                name: "tabClass");

            migrationBuilder.DropTable(
                name: "tabRoom");

            migrationBuilder.DropTable(
                name: "tabSubject");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AzureUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "class",
                columns: table => new
                {
                    class_id = table.Column<long>(type: "bigint", nullable: false),
                    display_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    azure_group_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabClasses_1", x => x.class_id);
                });

            migrationBuilder.CreateTable(
                name: "log",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabLog", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "room",
                columns: table => new
                {
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabRoom", x => x.room_id);
                });

            migrationBuilder.CreateTable(
                name: "subject",
                columns: table => new
                {
                    subject_id = table.Column<long>(type: "bigint", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabSubjects", x => x.subject_id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    azure_user_id = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    refresh_token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_seen = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabUsers", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "computer",
                columns: table => new
                {
                    computer_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mac_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_seen = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    last_user = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabComputers", x => x.computer_id);
                    table.ForeignKey(
                        name: "FK_tabComputers_tabRooms",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson",
                columns: table => new
                {
                    lesson_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_id = table.Column<long>(type: "bigint", nullable: false),
                    class_id = table.Column<long>(type: "bigint", nullable: false),
                    start = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    end = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabLessons_1", x => x.lesson_id);
                    table.ForeignKey(
                        name: "FK_tabLessons_tabClasses",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "class_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tabLessons_tabRooms",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tabLessons_tabSubjects",
                        column: x => x.subject_id,
                        principalTable: "subject",
                        principalColumn: "subject_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tabComputer_RoomID",
                table: "computer",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_tabLesson_ClassID",
                table: "lesson",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_tabLesson_RoomID",
                table: "lesson",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_tabLesson_SubjectID",
                table: "lesson",
                column: "subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "computer");

            migrationBuilder.DropTable(
                name: "lesson");

            migrationBuilder.DropTable(
                name: "log");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "class");

            migrationBuilder.DropTable(
                name: "room");

            migrationBuilder.DropTable(
                name: "subject");

            migrationBuilder.CreateTable(
                name: "tabClass",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "integer", nullable: false),
                    AzureGroupID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Head = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
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
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                    LongName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
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
                    LongName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
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
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUser = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    MacAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    ClassID = table.Column<int>(type: "integer", nullable: false),
                    RoomID = table.Column<int>(type: "integer", nullable: false),
                    SubjectID = table.Column<int>(type: "integer", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
    }
}

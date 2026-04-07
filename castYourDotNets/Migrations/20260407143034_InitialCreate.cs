using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace castYourDotNets.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scriptures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ScriptureSource = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Book = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Chapter = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsMemorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    PracticeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStreakDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPracticedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MemorizedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scriptures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    NormalizedUsername = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerseVaults",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    scripture = table.Column<int>(type: "INTEGER", nullable: false),
                    book = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Chapter = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseInt = table.Column<int>(type: "INTEGER", nullable: false),
                    Verse_Refrence = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    VerseText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseVaults", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MemorizationEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsMemorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMemorizedThroughGame = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemorizationEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemorizationEntries_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Book = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Chapter = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseStart = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsMemorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    MemorizedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ReviewStreakDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LastReviewedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageClasses_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemorizationEntries_UserId",
                table: "MemorizationEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PageClasses_UserId",
                table: "PageClasses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_NormalizedUsername",
                table: "UserAccounts",
                column: "NormalizedUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemorizationEntries");

            migrationBuilder.DropTable(
                name: "PageClasses");

            migrationBuilder.DropTable(
                name: "Scriptures");

            migrationBuilder.DropTable(
                name: "VerseVaults");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}

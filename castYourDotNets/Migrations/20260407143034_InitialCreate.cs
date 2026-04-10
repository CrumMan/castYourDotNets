#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace castYourDotNets.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Scriptures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Topic = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "MemorizationEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScriptureId = table.Column<Guid>(type: "TEXT", nullable: true),
                    GameText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsMemorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMemorizedThroughGame = table.Column<bool>(type: "INTEGER", nullable: false),
                    TableVerses = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    TableThemes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TableKeywords = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TableInsights = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TablePersonalApplication = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HasMemorizationTable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemorizationEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemorizationEntries_Scriptures_ScriptureId",
                        column: x => x.ScriptureId,
                        principalTable: "Scriptures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MemorizationEntries_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserScriptures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScriptureId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PracticeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStreakDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LongestStreakDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPracticedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MemorizedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsMemorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    DifficultyRating = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonalNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SuccessRate = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessfulAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserScriptures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserScriptures_Scriptures_ScriptureId",
                        column: x => x.ScriptureId,
                        principalTable: "Scriptures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserScriptures_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemorizationEntries_ScriptureId",
                table: "MemorizationEntries",
                column: "ScriptureId");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserScriptures_ScriptureId",
                table: "UserScriptures",
                column: "ScriptureId");

            migrationBuilder.CreateIndex(
                name: "IX_UserScriptures_Status",
                table: "UserScriptures",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserScriptures_UserId",
                table: "UserScriptures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserScriptures_IsMemorized",
                table: "UserScriptures",
                column: "IsMemorized");

            migrationBuilder.CreateIndex(
                name: "IX_UserScriptures_LastPracticedAtUtc",
                table: "UserScriptures",
                column: "LastPracticedAtUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemorizationEntries");

            migrationBuilder.DropTable(
                name: "PageClasses");

            migrationBuilder.DropTable(
                name: "UserScriptures");

            migrationBuilder.DropTable(
                name: "VerseVaults");

            migrationBuilder.DropTable(
                name: "Scriptures");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}

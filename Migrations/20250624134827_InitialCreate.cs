using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoogleTranslateHistoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TranslationEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserEmail = table.Column<string>(type: "TEXT", nullable: true),
                    SourceText = table.Column<string>(type: "TEXT", nullable: true),
                    TranslatedText = table.Column<string>(type: "TEXT", nullable: true),
                    SourceLang = table.Column<string>(type: "TEXT", nullable: true),
                    TargetLang = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CorrectAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    WrongAttempts = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranslationEntries");
        }
    }
}

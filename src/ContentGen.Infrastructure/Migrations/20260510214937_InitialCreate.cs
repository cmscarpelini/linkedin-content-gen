using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentGen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    RawHtml = table.Column<string>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TechnicalSummaryPtBR = table.Column<string>(type: "TEXT", nullable: false),
                    InsightsPtBR = table.Column<string>(type: "TEXT", nullable: false),
                    CasualExplanationPtBR = table.Column<string>(type: "TEXT", nullable: false),
                    PostSuggestionsPtBR = table.Column<string>(type: "TEXT", nullable: false),
                    TechnicalSummaryEnUS = table.Column<string>(type: "TEXT", nullable: false),
                    InsightsEnUS = table.Column<string>(type: "TEXT", nullable: false),
                    CasualExplanationEnUS = table.Column<string>(type: "TEXT", nullable: false),
                    PostSuggestionsEnUS = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessedContents_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedContents_ArticleId",
                table: "ProcessedContents",
                column: "ArticleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedContents");

            migrationBuilder.DropTable(
                name: "Articles");
        }
    }
}

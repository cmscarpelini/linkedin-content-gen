using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentGen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostPublications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    PostIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPublications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostPublications_ArticleId_Language_PostIndex",
                table: "PostPublications",
                columns: new[] { "ArticleId", "Language", "PostIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostPublications");
        }
    }
}

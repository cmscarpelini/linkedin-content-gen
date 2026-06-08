using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentGen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRawHtmlColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawHtml",
                table: "Articles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawHtml",
                table: "Articles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

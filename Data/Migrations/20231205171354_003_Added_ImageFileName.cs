using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboTicketsMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class _003_Added_ImageFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "TicketAttachments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "TicketAttachments");
        }
    }
}

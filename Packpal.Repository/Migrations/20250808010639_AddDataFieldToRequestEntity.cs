using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Packpal.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDataFieldToRequestEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Requests");
        }
    }
}

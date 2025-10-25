using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Genre_Icon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "genres",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "genres");
        }
    }
}

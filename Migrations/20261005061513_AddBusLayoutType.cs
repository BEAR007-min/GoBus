using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoBus.Migrations
{
    /// <inheritdoc />
    public partial class AddBusLayoutType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LayoutType",
                table: "Buses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LayoutType",
                table: "Buses");
        }
    }
}

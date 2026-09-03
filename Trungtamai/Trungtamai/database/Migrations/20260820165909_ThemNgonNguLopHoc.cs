using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trungtamai.Migrations
{
    /// <inheritdoc />
    public partial class ThemNgonNguLopHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NgonNgu",
                table: "LopHocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgonNgu",
                table: "LopHocs");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trungtamai.Migrations
{
    /// <inheritdoc />
    public partial class ThemLichHocVaPhong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GioBatDau",
                table: "LopHocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GioKetThuc",
                table: "LopHocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NgayHoc",
                table: "LopHocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhongHoc",
                table: "LopHocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GioBatDau",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "GioKetThuc",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "NgayHoc",
                table: "LopHocs");

            migrationBuilder.DropColumn(
                name: "PhongHoc",
                table: "LopHocs");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trungtamai.Migrations
{
    /// <inheritdoc />
    public partial class ThemHocPhiHocVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDongCuoi",
                table: "HocViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienDaDong",
                table: "HocViens",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienPhaiDong",
                table: "HocViens",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayDongCuoi",
                table: "HocViens");

            migrationBuilder.DropColumn(
                name: "SoTienDaDong",
                table: "HocViens");

            migrationBuilder.DropColumn(
                name: "SoTienPhaiDong",
                table: "HocViens");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trungtamai.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangDiemSo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiemSos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    MaHocVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTenHocVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiemKT1 = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    DiemKT2 = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    DiemKT3 = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    DiemKiemTraLon = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaoVienNhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemSos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemSos");
        }
    }
}

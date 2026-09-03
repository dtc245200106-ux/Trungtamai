using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trungtamai.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangLopHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LopHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KhoaHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaoVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiSoToiDa = table.Column<int>(type: "int", nullable: false),
                    SiSoHienTai = table.Column<int>(type: "int", nullable: false),
                    CaHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LopHocs");
        }
    }
}

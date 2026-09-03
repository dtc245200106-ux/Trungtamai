using Microsoft.EntityFrameworkCore;
using Trungtamai.Models;

namespace Trungtamai.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<KhoaHoc> KhoaHocs { get; set; }
        public DbSet<GiaoVien> GiaoViens { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }   // <-- thêm dòng này
        public DbSet<HocVien> HocViens { get; set; }
        public DbSet<DiemDanh> DiemDanhs { get; set; }
        public DbSet<DiemSo> DiemSos { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
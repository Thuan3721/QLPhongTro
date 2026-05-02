using HeThongQuanLyPhongTro.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<CoSo> CoSo { get; set; }
        public DbSet<Phong> Phong { get; set; }
        public DbSet<HopDong> HopDong { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDon { get; set; }
        public DbSet<ThanhToan> ThanhToan { get; set; }
        public DbSet<BaiDang> BaiDang { get; set; }
        public DbSet<CoSoVatChat> CoSoVatChat { get; set; }
        public DbSet<NguoiOHopDong> NguoiOHopDong { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đặt tên bảng đúng với SQL
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<KhachHang>().ToTable("KhachHang");
            modelBuilder.Entity<CoSo>().ToTable("CoSo");
            modelBuilder.Entity<Phong>().ToTable("Phong");
            modelBuilder.Entity<HopDong>().ToTable("HopDong");
            modelBuilder.Entity<HoaDon>().ToTable("HoaDon");
            modelBuilder.Entity<ChiTietHoaDon>().ToTable("ChiTietHoaDon");
            modelBuilder.Entity<ThanhToan>().ToTable("ThanhToan");
            modelBuilder.Entity<BaiDang>().ToTable("BaiDang");
            modelBuilder.Entity<CoSoVatChat>().ToTable("CoSoVatChat");
            modelBuilder.Entity<NguoiOHopDong>().ToTable("NguoiOHopDong");
        }
    }
}
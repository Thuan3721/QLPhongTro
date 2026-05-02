using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chủ
        public async Task<IActionResult> Index(string searchString)
        {
            var baiDangs = _context.BaiDang
                .Include(b => b.PhongNavigation)
                .ThenInclude(p => p.CoSo)
                .Where(b => b.TrangThai == "Hiển thị")
                .OrderByDescending(b => b.NgayDang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                baiDangs = baiDangs.Where(b =>
                    (b.TieuDe != null && b.TieuDe.Contains(searchString)) ||
                    (b.PhongNavigation != null && b.PhongNavigation.TenPhong.Contains(searchString)));
            }

            ViewBag.SearchString = searchString;

            // Lấy phòng nổi bật
            ViewBag.PhongNoiBat = await _context.Phong
                .Include(p => p.CoSo)
                .Where(p => p.TrangThai == "Trống")
                .Take(6)
                .ToListAsync();

            return View(await baiDangs.ToListAsync());
        }

        // Chi tiết bài đăng - QUAN TRỌNG
        public async Task<IActionResult> ChiTietBaiDang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiDang = await _context.BaiDang
                .Include(b => b.PhongNavigation)
                .ThenInclude(p => p.CoSo)
                .FirstOrDefaultAsync(b => b.MaBaiDang == id && b.TrangThai == "Hiển thị");

            if (baiDang == null)
            {
                return NotFound();
            }

            // Bài đăng liên quan
            var baiDangLienQuan = await _context.BaiDang
                .Include(b => b.PhongNavigation)
                .Where(b => b.TrangThai == "Hiển thị" && b.MaBaiDang != id)
                .OrderByDescending(b => b.NgayDang)
                .Take(3)
                .ToListAsync();

            ViewBag.BaiDangLienQuan = baiDangLienQuan;
            return View(baiDang);
        }

        // Chi tiết phòng - QUAN TRỌNG
        public async Task<IActionResult> ChiTietPhong(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phong
                .Include(p => p.CoSo)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        // Danh sách phòng trống
        public async Task<IActionResult> DanhSachPhongTrong()
        {
            var phongs = await _context.Phong
                .Include(p => p.CoSo)
                .Where(p => p.TrangThai == "Trống")
                .ToListAsync();
            return View(phongs);
        }
    }
}
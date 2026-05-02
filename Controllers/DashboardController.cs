using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SỬA PHƯƠNG THỨC INDEX NÀY
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var role = HttpContext.Session.GetString("Role");

            // Nếu là Admin -> giao diện Admin
            if (role == "Admin")
            {
                var model = new DashboardViewModel();

                // Thống kê cho Admin
                model.TongSoPhong = _context.Phong.Count();
                model.SoPhongDaThue = _context.Phong.Count(p => p.TrangThai == "Đã thuê");
                model.SoPhongTrong = _context.Phong.Count(p => p.TrangThai == "Trống");
                model.TongSoKhachThue = _context.HopDong
                    .Where(h => h.TrangThai == "Hiệu lực")
                    .Select(h => h.MaKhachHang)
                    .Distinct()
                    .Count();

                var now = DateTime.Now;
                model.DoanhThuThangNay = _context.HoaDon
                    .Where(h => h.Thang == now.Month && h.Nam == now.Year && h.TrangThai == "Đã thanh toán")
                    .Sum(h => h.TongTien) ?? 0;

                var ngayHetHan = DateTime.Now.AddDays(7);

                var hopDongSapHetHan = _context.HopDong
                    .Include(h => h.PhongNavigation)
                    .Include(h => h.KhachHangNavigation)
                    .Where(h => h.TrangThai == "Hiệu lực" && h.NgayKetThuc <= ngayHetHan && h.NgayKetThuc >= DateTime.Now)
                    .ToList();

                model.SoHopDongSapHetHan = hopDongSapHetHan.Count;
                model.HopDongSapHetHanList = hopDongSapHetHan.Select(h => new HopDongSapHetHan
                {
                    TenPhong = h.PhongNavigation?.TenPhong ?? "N/A",
                    TenKhachHang = h.KhachHangNavigation?.HoTen ?? "N/A",
                    NgayKetThuc = h.NgayKetThuc ?? DateTime.Now,
                    SoNgayConLai = (h.NgayKetThuc - DateTime.Now)?.Days ?? 0
                }).ToList();

                // ========== THÊM THỐNG KÊ BÀI ĐĂNG ==========
                model.TongSoBaiDang = await _context.BaiDang.CountAsync();
                model.SoBaiDangHienThi = await _context.BaiDang.CountAsync(b => b.TrangThai == "Hiển thị");
                model.SoBaiDangAn = await _context.BaiDang.CountAsync(b => b.TrangThai == "Ẩn");
                model.SoBaiDangThangNay = await _context.BaiDang.CountAsync(b =>
                    b.NgayDang.HasValue &&
                    b.NgayDang.Value.Month == now.Month &&
                    b.NgayDang.Value.Year == now.Year);

                // Lấy 5 bài đăng gần đây nhất
                var baiDangGanDay = await _context.BaiDang
                    .Include(b => b.PhongNavigation)
                    .OrderByDescending(b => b.NgayDang)
                    .Take(5)
                    .Select(b => new BaiDangGanDay
                    {
                        MaBaiDang = b.MaBaiDang,
                        TieuDe = b.TieuDe ?? "Không có tiêu đề",
                        TenPhong = b.PhongNavigation != null ? b.PhongNavigation.TenPhong : "N/A",
                        NgayDang = b.NgayDang ?? DateTime.Now,
                        TrangThai = b.TrangThai ?? "Ẩn",
                        LuotXem = 0
                    })
                    .ToListAsync();

                model.BaiDangGanDayList = baiDangGanDay;

                ViewBag.Role = role;
                ViewBag.Username = HttpContext.Session.GetString("Username");

                return View("AdminDashboard", model);
            }

            // Nếu là Khách -> chuyển sang trang Khách
            if (role == "Khach")
            {
                return RedirectToAction("KhachDashboard");
            }

            return RedirectToAction("Index", "Login");
        }

        // Action cho Khách hàng
        public async Task<IActionResult> KhachDashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            var userTaiKhoan = HttpContext.Session.GetInt32("UserId");

            // Lấy thông tin khách hàng từ tài khoản
            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == userTaiKhoan);

            if (khachHang == null)
            {
                ViewBag.Error = "Không tìm thấy thông tin khách hàng";
                return View();
            }

            // Lấy hợp đồng hiện tại của khách
            var hopDongHienTai = await _context.HopDong
                .Include(h => h.PhongNavigation)
                .FirstOrDefaultAsync(h => h.MaKhachHang == khachHang.MaKhachHang && h.TrangThai == "Hiệu lực");

            ViewBag.KhachHang = khachHang;
            ViewBag.HopDong = hopDongHienTai;
            ViewBag.Role = "Khach";
            ViewBag.Username = HttpContext.Session.GetString("Username");

            return View();
        }
    }
}
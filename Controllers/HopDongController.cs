using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class HopDongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HopDongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách hợp đồng
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var hopDongs = _context.HopDong
                .Include(h => h.PhongNavigation)
                .Include(h => h.KhachHangNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                hopDongs = hopDongs.Where(h =>
                    (h.PhongNavigation != null && h.PhongNavigation.TenPhong.Contains(searchString)) ||
                    (h.KhachHangNavigation != null && h.KhachHangNavigation.HoTen.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                hopDongs = hopDongs.Where(h => h.TrangThai == trangThai);
            }

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.TrangThaiList = new List<string> { "Tất cả", "Hiệu lực", "Hết hạn", "Đã hủy" };

            return View(await hopDongs.ToListAsync());
        }

        // GET: Tạo hợp đồng mới
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.PhongList = _context.Phong
                .Include(p => p.CoSo)
                .Where(p => p.TrangThai == "Trống")
                .ToList();

            ViewBag.KhachHangList = _context.KhachHang.ToList();

            return View();
        }

        // POST: Tạo hợp đồng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int maPhong, int maKhachHang, DateTime ngayBatDau, DateTime ngayKetThuc, decimal tienCoc)
        {
            // Kiểm tra phòng còn trống không
            var phong = await _context.Phong.FindAsync(maPhong);
            if (phong == null || phong.TrangThai != "Trống")
            {
                TempData["Error"] = "Phòng này đã được thuê hoặc không tồn tại!";
                ViewBag.PhongList = _context.Phong.Where(p => p.TrangThai == "Trống").Include(p => p.CoSo).ToList();
                ViewBag.KhachHangList = _context.KhachHang.ToList();
                return View();
            }

            // Kiểm tra ngày hợp lệ
            if (ngayKetThuc <= ngayBatDau)
            {
                TempData["Error"] = "Ngày kết thúc phải sau ngày bắt đầu!";
                ViewBag.PhongList = _context.Phong.Where(p => p.TrangThai == "Trống").Include(p => p.CoSo).ToList();
                ViewBag.KhachHangList = _context.KhachHang.ToList();
                return View();
            }

            // Tạo hợp đồng mới
            var hopDong = new HopDong
            {
                MaPhong = maPhong,
                MaKhachHang = maKhachHang,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                TienCoc = tienCoc,
                TrangThai = "Hiệu lực"
            };

            _context.HopDong.Add(hopDong);

            // Cập nhật trạng thái phòng thành Đã thuê
            phong.TrangThai = "Đã thuê";
            _context.Update(phong);

            // Ghi vào lịch sử thuê phòng (nếu có bảng LichSuThuePhong)
            // _context.LichSuThuePhongs.Add(...);

            await _context.SaveChangesAsync();

            // Tạo hóa đơn cho các tháng
            await TaoHoaDonChoHopDong(hopDong.MaHopDong, phong.GiaPhong, ngayBatDau, ngayKetThuc);

            TempData["Success"] = "Tạo hợp đồng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Chi tiết hợp đồng
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var hopDong = await _context.HopDong
                .Include(h => h.PhongNavigation)
                .ThenInclude(p => p.CoSo)
                .Include(h => h.KhachHangNavigation)
                .FirstOrDefaultAsync(m => m.MaHopDong == id);

            if (hopDong == null)
            {
                return NotFound();
            }

            var hoaDons = await _context.HoaDon
                .Where(h => h.MaHopDong == id)
                .OrderByDescending(h => h.Nam)
                .ThenByDescending(h => h.Thang)
                .ToListAsync();

            ViewBag.HoaDons = hoaDons;
            return View(hopDong);
        }

        // POST: Chấm dứt hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDut(int id)
        {
            var hopDong = await _context.HopDong
                .Include(h => h.PhongNavigation)
                .FirstOrDefaultAsync(h => h.MaHopDong == id);

            if (hopDong == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái hợp đồng
            hopDong.TrangThai = "Đã hủy";
            _context.Update(hopDong);

            // Trả phòng về trạng thái trống
            var phong = hopDong.PhongNavigation;
            if (phong != null)
            {
                phong.TrangThai = "Trống";
                _context.Update(phong);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã chấm dứt hợp đồng!";
            return RedirectToAction(nameof(Index));
        }

        // Tạo hóa đơn tự động cho hợp đồng
        private async Task TaoHoaDonChoHopDong(int maHopDong, decimal giaPhong, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            int currentMonth = ngayBatDau.Month;
            int currentYear = ngayBatDau.Year;
            int endMonth = ngayKetThuc.Month;
            int endYear = ngayKetThuc.Year;

            while (currentYear < endYear || (currentYear == endYear && currentMonth <= endMonth))
            {
                var existingHoaDon = await _context.HoaDon
                    .AnyAsync(h => h.MaHopDong == maHopDong && h.Thang == currentMonth && h.Nam == currentYear);

                if (!existingHoaDon)
                {
                    var hoaDon = new HoaDon
                    {
                        MaHopDong = maHopDong,
                        Thang = currentMonth,
                        Nam = currentYear,
                        TongTien = giaPhong,
                        TrangThai = "Chưa thanh toán",
                        NgayTao = DateTime.Now
                    };
                    _context.HoaDon.Add(hoaDon);
                }

                currentMonth++;
                if (currentMonth > 12)
                {
                    currentMonth = 1;
                    currentYear++;
                }
            }

            await _context.SaveChangesAsync();
        }

        // API: Lấy thông tin phòng
        [HttpGet]
        public async Task<IActionResult> GetPhongInfo(int maPhong)
        {
            var phong = await _context.Phong
                .Include(p => p.CoSo)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                giaPhong = phong.GiaPhong,
                dienTich = phong.DienTich,
                tenCoSo = phong.CoSo?.TenCoSo
            });
        }
    }
}
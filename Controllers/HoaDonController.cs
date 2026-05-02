using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách hóa đơn
        public async Task<IActionResult> Index(string searchString, int? thang, int? nam, string trangThai)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var hoaDons = _context.HoaDon
                .Include(h => h.HopDongNavigation)
                    .ThenInclude(h => h.PhongNavigation)
                .Include(h => h.HopDongNavigation)
                    .ThenInclude(h => h.KhachHangNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                hoaDons = hoaDons.Where(h =>
                    (h.HopDongNavigation != null && h.HopDongNavigation.KhachHangNavigation != null &&
                     h.HopDongNavigation.KhachHangNavigation.HoTen.Contains(searchString)) ||
                    (h.HopDongNavigation != null && h.HopDongNavigation.PhongNavigation != null &&
                     h.HopDongNavigation.PhongNavigation.TenPhong.Contains(searchString)));
            }

            if (thang.HasValue)
            {
                hoaDons = hoaDons.Where(h => h.Thang == thang.Value);
            }

            if (nam.HasValue)
            {
                hoaDons = hoaDons.Where(h => h.Nam == nam.Value);
            }

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                hoaDons = hoaDons.Where(h => h.TrangThai == trangThai);
            }

            ViewBag.SearchString = searchString;
            ViewBag.Thang = thang;
            ViewBag.Nam = nam;
            ViewBag.TrangThai = trangThai;
            ViewBag.TrangThaiList = new List<string> { "Tất cả", "Chưa thanh toán", "Đã thanh toán" };
            ViewBag.NamList = await _context.HoaDon
                .Select(h => h.Nam)
                .Distinct()
                .OrderByDescending(n => n)
                .ToListAsync();

            return View(await hoaDons.ToListAsync());
        }

        // GET: Chi tiết hóa đơn
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

            var hoaDon = await _context.HoaDon
                .Include(h => h.HopDongNavigation)
                    .ThenInclude(h => h.PhongNavigation)
                        .ThenInclude(p => p.CoSo)
                .Include(h => h.HopDongNavigation)
                    .ThenInclude(h => h.KhachHangNavigation)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            var chiTietHoaDons = await _context.ChiTietHoaDon
                .Where(c => c.MaHoaDon == id)
                .ToListAsync();

            ViewBag.ChiTietHoaDons = chiTietHoaDons;
            return View(hoaDon);
        }

        // GET: Chỉnh sửa hóa đơn (CHỈ CHO PHÉP KHI CHƯA THANH TOÁN)
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDon
                .Include(h => h.HopDongNavigation)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // KIỂM TRA: Nếu đã thanh toán thì không cho sửa
            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                TempData["Error"] = "Hóa đơn đã thanh toán không thể sửa!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.HopDongList = _context.HopDong
                .Include(h => h.PhongNavigation)
                .Include(h => h.KhachHangNavigation)
                .Where(h => h.TrangThai == "Hiệu lực")
                .ToList();

            return View(hoaDon);
        }

        // POST: Chỉnh sửa hóa đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int maHopDong, int thang, int nam, decimal tongTien)
        {
            var hoaDon = await _context.HoaDon.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            // KIỂM TRA: Nếu đã thanh toán thì không cho sửa
            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                TempData["Error"] = "Hóa đơn đã thanh toán không thể sửa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng lặp
            var exists = await _context.HoaDon
                .AnyAsync(h => h.MaHopDong == maHopDong && h.Thang == thang && h.Nam == nam && h.MaHoaDon != id);

            if (exists)
            {
                TempData["Error"] = "Hóa đơn của tháng này đã tồn tại!";
                ViewBag.HopDongList = _context.HopDong
                    .Include(h => h.PhongNavigation)
                    .Include(h => h.KhachHangNavigation)
                    .Where(h => h.TrangThai == "Hiệu lực")
                    .ToList();
                return View(hoaDon);
            }

            hoaDon.MaHopDong = maHopDong;
            hoaDon.Thang = thang;
            hoaDon.Nam = nam;
            hoaDon.TongTien = tongTien;

            _context.Update(hoaDon);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật hóa đơn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Xóa hóa đơn (CHỈ CHO PHÉP KHI CHƯA THANH TOÁN)
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDon
                .Include(h => h.HopDongNavigation)
                    .ThenInclude(h => h.PhongNavigation)
                .Include(h => h.HopDongNavigation)
                    .ThenInclude(h => h.KhachHangNavigation)
                .FirstOrDefaultAsync(m => m.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // KIỂM TRA: Nếu đã thanh toán thì không cho xóa
            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                TempData["Error"] = "Hóa đơn đã thanh toán không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            return View(hoaDon);
        }

        // POST: Xóa hóa đơn
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaDon = await _context.HoaDon.FindAsync(id);
            if (hoaDon != null)
            {
                // KIỂM TRA: Nếu đã thanh toán thì không cho xóa
                if (hoaDon.TrangThai == "Đã thanh toán")
                {
                    TempData["Error"] = "Hóa đơn đã thanh toán không thể xóa!";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra đã có thanh toán chưa (thêm lớp bảo vệ)
                var hasPayment = await _context.ThanhToan.AnyAsync(t => t.MaHoaDon == id);
                if (hasPayment)
                {
                    TempData["Error"] = "Không thể xóa vì hóa đơn đã có thanh toán!";
                    return RedirectToAction(nameof(Index));
                }

                _context.HoaDon.Remove(hoaDon);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa hóa đơn thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Thanh toán hóa đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int id, decimal soTien, string noiDung)
        {
            var hoaDon = await _context.HoaDon.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                TempData["Error"] = "Hóa đơn này đã được thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            // Tạo bản ghi thanh toán
            var thanhToan = new ThanhToan
            {
                MaHoaDon = id,
                SoTien = soTien,
                NgayThanhToan = DateTime.Now,
                NoiDungChuyenKhoan = noiDung,
                TrangThai = "Thành công"
            };

            _context.ThanhToan.Add(thanhToan);

            // Cập nhật trạng thái hóa đơn
            hoaDon.TrangThai = "Đã thanh toán";
            _context.Update(hoaDon);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Thanh toán thành công {soTien:N0} đ!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Tạo hóa đơn hàng loạt
        public async Task<IActionResult> TaoHangLoat()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var thangHienTai = DateTime.Now.Month;
            var namHienTai = DateTime.Now.Year;

            var hopDongs = await _context.HopDong
                .Include(h => h.PhongNavigation)
                .Where(h => h.TrangThai == "Hiệu lực")
                .ToListAsync();

            int dem = 0;
            foreach (var hopDong in hopDongs)
            {
                var exists = await _context.HoaDon
                    .AnyAsync(h => h.MaHopDong == hopDong.MaHopDong && h.Thang == thangHienTai && h.Nam == namHienTai);

                if (!exists && hopDong.PhongNavigation != null)
                {
                    var hoaDon = new HoaDon
                    {
                        MaHopDong = hopDong.MaHopDong,
                        Thang = thangHienTai,
                        Nam = namHienTai,
                        TongTien = hopDong.PhongNavigation.GiaPhong,
                        TrangThai = "Chưa thanh toán",
                        NgayTao = DateTime.Now
                    };
                    _context.HoaDon.Add(hoaDon);
                    dem++;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã tạo {dem} hóa đơn cho tháng {thangHienTai}/{namHienTai}!";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoaDon(int maHoaDon, int soNguoi, decimal tienDien, decimal tienNuoc, decimal chiPhiPhatSinh, string ghiChu, decimal tongCong)
        {
            try
            {
                var hoaDon = await _context.HoaDon.FindAsync(maHoaDon);
                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                if (hoaDon.TrangThai == "Đã thanh toán")
                {
                    return Json(new { success = false, message = "Hóa đơn đã thanh toán, không thể cập nhật!" });
                }

                // Xóa chi tiết cũ
                var oldDetails = await _context.ChiTietHoaDon.Where(c => c.MaHoaDon == maHoaDon).ToListAsync();
                _context.ChiTietHoaDon.RemoveRange(oldDetails);

                // Thêm chi tiết mới
                var giaPhong = hoaDon.TongTien ?? 0;
                _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                {
                    MaHoaDon = maHoaDon,
                    LoaiKhoanThu = "Tiền phòng",
                    SoLuong = 1,
                    DonGia = giaPhong,
                    ThanhTien = giaPhong
                });

                var phiDichVu = soNguoi * 200000;
                if (phiDichVu > 0)
                {
                    _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                    {
                        MaHoaDon = maHoaDon,
                        LoaiKhoanThu = $"Phí dịch vụ ({soNguoi} người)",
                        SoLuong = soNguoi,
                        DonGia = 200000,
                        ThanhTien = phiDichVu
                    });
                }

                if (tienDien > 0)
                {
                    _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                    {
                        MaHoaDon = maHoaDon,
                        LoaiKhoanThu = "Tiền điện",
                        SoLuong = 1,
                        DonGia = tienDien,
                        ThanhTien = tienDien
                    });
                }

                if (tienNuoc > 0)
                {
                    _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                    {
                        MaHoaDon = maHoaDon,
                        LoaiKhoanThu = "Tiền nước",
                        SoLuong = 1,
                        DonGia = tienNuoc,
                        ThanhTien = tienNuoc
                    });
                }

                if (chiPhiPhatSinh > 0)
                {
                    _context.ChiTietHoaDon.Add(new ChiTietHoaDon
                    {
                        MaHoaDon = maHoaDon,
                        LoaiKhoanThu = $"Phát sinh: {ghiChu}",
                        SoLuong = 1,
                        DonGia = chiPhiPhatSinh,
                        ThanhTien = chiPhiPhatSinh
                    });
                }

                hoaDon.TongTien = tongCong;
                _context.Update(hoaDon);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Thống kê doanh thu
        public async Task<IActionResult> ThongKe()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var namHienTai = DateTime.Now.Year;
            var thongKeTheoThang = new List<object>();

            for (int i = 1; i <= 12; i++)
            {
                var tongThu = await _context.HoaDon
                    .Where(h => h.Thang == i && h.Nam == namHienTai && h.TrangThai == "Đã thanh toán")
                    .SumAsync(h => h.TongTien);

                var tongNo = await _context.HoaDon
                    .Where(h => h.Thang == i && h.Nam == namHienTai && h.TrangThai == "Chưa thanh toán")
                    .SumAsync(h => h.TongTien);

                thongKeTheoThang.Add(new { thang = i, tongThu = tongThu, tongNo = tongNo });
            }

            ViewBag.ThongKeTheoThang = thongKeTheoThang;
            ViewBag.NamHienTai = namHienTai;
            ViewBag.TongDoanhThu = await _context.HoaDon
                .Where(h => h.Nam == namHienTai && h.TrangThai == "Đã thanh toán")
                .SumAsync(h => h.TongTien);

            ViewBag.TongNo = await _context.HoaDon
                .Where(h => h.Nam == namHienTai && h.TrangThai == "Chưa thanh toán")
                .SumAsync(h => h.TongTien);

            return View();
        }
    }
}
using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using HeThongQuanLyPhongTro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class HopDongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PdfHopDongService _pdfService;

        public HopDongController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _pdfService = new PdfHopDongService(webHostEnvironment);
        }

        // GET: Danh sách hợp đồng
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            var hopDongs = await _context.HopDong
                .Include(h => h.PhongNavigation)
                .ThenInclude(p => p.CoSo)
                .Include(h => h.KhachHangNavigation)
                .OrderByDescending(h => h.MaHopDong)
                .ToListAsync();

            return View(hopDongs);
        }

        // GET: Tạo hợp đồng mới
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            ViewBag.PhongList = await _context.Phong
                .Include(p => p.CoSo)
                .Where(p => p.TrangThai == "Trống")
                .ToListAsync();

            ViewBag.KhachHangList = await _context.KhachHang.ToListAsync();

            return View();
        }

        // POST: Tạo hợp đồng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int maPhong, int maKhachHang,
            DateTime ngayBatDau, DateTime ngayKetThuc, decimal tienCoc, string ngayKy)
        {
            // 1. Kiểm tra phòng tồn tại
            var phong = await _context.Phong.FindAsync(maPhong);
            if (phong == null)
            {
                TempData["Error"] = "Phòng không tồn tại!";
                return RedirectToAction("Create");
            }

            // 2. Kiểm tra phòng còn trống không
            if (phong.TrangThai != "Trống")
            {
                TempData["Error"] = "Phòng này đã được thuê!";
                ViewBag.PhongList = await _context.Phong.Where(p => p.TrangThai == "Trống").Include(p => p.CoSo).ToListAsync();
                ViewBag.KhachHangList = await _context.KhachHang.ToListAsync();
                return View();
            }

            // 3. KIỂM TRA TRÙNG LẶP HỢP ĐỒNG
            var hopDongTrung = await _context.HopDong
                .Where(h => h.MaPhong == maPhong && h.TrangThai == "Hiệu lực")
                .AnyAsync(h =>
                    (ngayBatDau >= h.NgayBatDau && ngayBatDau <= h.NgayKetThuc) ||
                    (ngayKetThuc >= h.NgayBatDau && ngayKetThuc <= h.NgayKetThuc) ||
                    (ngayBatDau <= h.NgayBatDau && ngayKetThuc >= h.NgayKetThuc)
                );

            if (hopDongTrung)
            {
                TempData["Error"] = "Phòng này đã có hợp đồng trong thời gian này! Không thể tạo hợp đồng chồng chéo.";
                ViewBag.PhongList = await _context.Phong.Where(p => p.TrangThai == "Trống").Include(p => p.CoSo).ToListAsync();
                ViewBag.KhachHangList = await _context.KhachHang.ToListAsync();
                return View();
            }

            // 4. Kiểm tra ngày hợp lệ
            if (ngayKetThuc <= ngayBatDau)
            {
                TempData["Error"] = "Ngày kết thúc phải sau ngày bắt đầu!";
                ViewBag.PhongList = await _context.Phong.Where(p => p.TrangThai == "Trống").Include(p => p.CoSo).ToListAsync();
                ViewBag.KhachHangList = await _context.KhachHang.ToListAsync();
                return View();
            }

            // 5. Lấy danh sách người ở đã thêm trước đó (liên kết với khách hàng)
            var danhSachNguoiO = await _context.NguoiOHopDong
                .Where(n => n.MaKhachHang == maKhachHang)
                .ToListAsync();

            // 6. Tạo hợp đồng mới
            var khachHang = await _context.KhachHang.FindAsync(maKhachHang);
            var coSo = await _context.CoSo.FindAsync(phong.MaCoSo);

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
            await _context.SaveChangesAsync();

            // 7. Cập nhật MaHopDong cho các người ở
            foreach (var nguoi in danhSachNguoiO)
            {
                nguoi.MaHopDong = hopDong.MaHopDong;
                _context.Update(nguoi);
            }
            await _context.SaveChangesAsync();

            // 8. Xuất PDF (có danh sách người ở)
            string pdfPath = await _pdfService.XuatHopDongPdf(hopDong, khachHang, phong, coSo, ngayKy, danhSachNguoiO);
            hopDong.FileHopDong = pdfPath;
            _context.Update(hopDong);

            // 9. Cập nhật trạng thái phòng
            phong.TrangThai = "Đã thuê";
            _context.Update(phong);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tạo hợp đồng thành công! Số hợp đồng: {hopDong.MaHopDong}";
            return RedirectToAction(nameof(Index));
        }

        // Xem PDF trên hệ thống
        public async Task<IActionResult> XemPdf(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            var hopDong = await _context.HopDong.FindAsync(id);
            if (hopDong == null || string.IsNullOrEmpty(hopDong.FileHopDong))
            {
                TempData["Error"] = "Hợp đồng chưa có file PDF!";
                return RedirectToAction("Details", new { id });
            }

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, hopDong.FileHopDong.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File PDF không tồn tại!";
                return RedirectToAction("Details", new { id });
            }

            return PhysicalFile(filePath, "application/pdf");
        }

        // Tải PDF về máy
        public async Task<IActionResult> TaiPdf(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            var hopDong = await _context.HopDong.FindAsync(id);
            if (hopDong == null || string.IsNullOrEmpty(hopDong.FileHopDong))
                return NotFound();

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, hopDong.FileHopDong.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", $"HopDong_{hopDong.MaHopDong}.pdf");
        }

        // Chi tiết hợp đồng
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            if (id == null) return NotFound();

            var hopDong = await _context.HopDong
                .Include(h => h.PhongNavigation)
                .ThenInclude(p => p.CoSo)
                .Include(h => h.KhachHangNavigation)
                .Include(h => h.NguoiOHopDongNavigation) // Lấy danh sách người ở
                .FirstOrDefaultAsync(m => m.MaHopDong == id);

            if (hopDong == null) return NotFound();

            return View(hopDong);
        }

        // Chấm dứt hợp đồng
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

            // Chuyển trạng thái thành "Đã hủy"
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

            TempData["Success"] = "Đã chấm dứt hợp đồng! Phòng đã trống, có thể tạo hợp đồng mới.";
            return RedirectToAction(nameof(Index));
        }

        // Gia hạn hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHan(int id, DateTime ngayKetThucMoi)
        {
            var hopDong = await _context.HopDong.FindAsync(id);
            if (hopDong == null)
            {
                return NotFound();
            }

            if (ngayKetThucMoi <= hopDong.NgayKetThuc)
            {
                TempData["Error"] = "Ngày kết thúc mới phải sau ngày kết thúc hiện tại!";
                return RedirectToAction("Details", new { id });
            }

            hopDong.NgayKetThuc = ngayKetThucMoi;
            _context.Update(hopDong);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã gia hạn hợp đồng đến ngày {ngayKetThucMoi:dd/MM/yyyy}";
            return RedirectToAction(nameof(Index));
        }

        // API kiểm tra trùng hợp đồng
        [HttpGet]
        public async Task<IActionResult> KiemTraTrung(int maPhong, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            var isConflict = await _context.HopDong
                .Where(h => h.MaPhong == maPhong && h.TrangThai == "Hiệu lực")
                .AnyAsync(h =>
                    (ngayBatDau >= h.NgayBatDau && ngayBatDau <= h.NgayKetThuc) ||
                    (ngayKetThuc >= h.NgayBatDau && ngayKetThuc <= h.NgayKetThuc) ||
                    (ngayBatDau <= h.NgayBatDau && ngayKetThuc >= h.NgayKetThuc)
                );

            if (isConflict)
            {
                return Json(new { isConflict = true, message = "Phòng đã có hợp đồng trong thời gian này!" });
            }
            return Json(new { isConflict = false });
        }
    }
}
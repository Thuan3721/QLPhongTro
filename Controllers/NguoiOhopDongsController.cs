using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class NguoiOHopDongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NguoiOHopDongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách người ở
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            var nguoiO = await _context.NguoiOHopDong
                .Include(n => n.KhachHangNavigation)
                .ToListAsync();

            return View(nguoiO);
        }

        // GET: Thêm người ở
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            ViewBag.KhachHangList = _context.KhachHang.ToList();
            return View();
        }

        // POST: Thêm người ở
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguoiOHopDong nguoiO)
        {
            if (ModelState.IsValid)
            {
                _context.NguoiOHopDong.Add(nguoiO);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm người ở thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhachHangList = _context.KhachHang.ToList();
            return View(nguoiO);
        }

        // GET: Sửa người ở
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            if (id == null) return NotFound();

            var nguoiO = await _context.NguoiOHopDong.FindAsync(id);
            if (nguoiO == null) return NotFound();

            ViewBag.KhachHangList = _context.KhachHang.ToList();
            return View(nguoiO);
        }

        // POST: Sửa người ở
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NguoiOHopDong nguoiO)
        {
            if (id != nguoiO.MaNguoiO) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(nguoiO);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật người ở thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.KhachHangList = _context.KhachHang.ToList();
            return View(nguoiO);
        }

        // GET: Xóa người ở
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            if (id == null) return NotFound();

            var nguoiO = await _context.NguoiOHopDong
                .Include(n => n.KhachHangNavigation)
                .FirstOrDefaultAsync(n => n.MaNguoiO == id);

            if (nguoiO == null) return NotFound();

            return View(nguoiO);
        }

        // POST: Xóa người ở
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nguoiO = await _context.NguoiOHopDong.FindAsync(id);
            if (nguoiO != null)
            {
                _context.NguoiOHopDong.Remove(nguoiO);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa người ở thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: Lấy danh sách người ở theo khách hàng (dùng khi tạo hợp đồng)
        [HttpGet]
        public async Task<IActionResult> GetByKhachHang(int maKhachHang)
        {
            var danhSach = await _context.NguoiOHopDong
                .Where(n => n.MaKhachHang == maKhachHang)
                .ToListAsync();

            return Json(danhSach);
        }
    }
}
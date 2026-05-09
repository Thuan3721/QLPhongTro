using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class BaiDangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BaiDangController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            var baiDangs = await _context.BaiDang
                .Include(b => b.PhongNavigation)
                .ThenInclude(p => p.CoSo)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            return View(baiDangs);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            ViewBag.PhongList = _context.Phong.Where(p => p.TrangThai == "Trống").ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaiDang baiDang)
        {
            if (ModelState.IsValid)
            {
                baiDang.NgayDang = DateTime.Now;
                baiDang.TrangThai = "Hiển thị";
                _context.Add(baiDang);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đăng bài thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PhongList = _context.Phong.ToList();
            return View(baiDang);
        }

        // Các action khác tương tự...
    }
}
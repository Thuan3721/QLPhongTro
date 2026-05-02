using HeThongQuanLyPhongTro.Data;
using HeThongQuanLyPhongTro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;
using System.Linq;
// ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
namespace HeThongQuanLyPhongTro.Controllers
{
    public class PhongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhongController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: Danh sách phòng
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var phongs = _context.Phong
                .Include(p => p.CoSo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                phongs = phongs.Where(p => p.TenPhong.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                phongs = phongs.Where(p => p.TrangThai == trangThai);
            }

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.TrangThaiList = new List<string> { "Tất cả", "Trống", "Đã thuê" };

            return View(await phongs.ToListAsync());
        }

        // GET: Chi tiết phòng
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

            var phong = await _context.Phong
                .Include(p => p.CoSo)
                .FirstOrDefaultAsync(m => m.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        // GET: Thêm mới phòng
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.CoSoList = _context.CoSo.ToList();
            return View();
        }

        // POST: Thêm mới phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phong phong)
        {
            if (ModelState.IsValid)
            {
                phong.TrangThai = "Trống";
                _context.Add(phong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CoSoList = _context.CoSo.ToList();
            return View(phong);
        }

        // GET: Chỉnh sửa phòng
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

            var phong = await _context.Phong.FindAsync(id);
            if (phong == null)
            {
                return NotFound();
            }

            ViewBag.CoSoList = _context.CoSo.ToList();
            return View(phong);
        }

        // POST: Chỉnh sửa phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Phong phong)
        {
            if (id != phong.MaPhong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phong);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật phòng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhongExists(phong.MaPhong))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CoSoList = _context.CoSo.ToList();
            return View(phong);
        }

        // GET: Xóa phòng
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

            var phong = await _context.Phong
                .Include(p => p.CoSo)
                .FirstOrDefaultAsync(m => m.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        // POST: Xóa phòng
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phong = await _context.Phong.FindAsync(id);
            if (phong != null)
            {
                var coHopDong = await _context.HopDong.AnyAsync(h => h.MaPhong == id && h.TrangThai == "Hiệu lực");
                if (coHopDong)
                {
                    TempData["Error"] = "Không thể xóa vì phòng này đang có hợp đồng hiệu lực!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Phong.Remove(phong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa phòng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Export danh sách phòng ra Excel
        public async Task<IActionResult> ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var phongs = await _context.Phong
                .Include(p => p.CoSo)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách phòng");

                worksheet.Cells[1, 1].Value = "Mã phòng";
                worksheet.Cells[1, 2].Value = "Tên phòng";
                worksheet.Cells[1, 3].Value = "Cơ sở";
                worksheet.Cells[1, 4].Value = "Giá phòng";
                worksheet.Cells[1, 5].Value = "Diện tích";
                worksheet.Cells[1, 6].Value = "Trạng thái";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var phong in phongs)
                {
                    worksheet.Cells[row, 1].Value = phong.MaPhong;
                    worksheet.Cells[row, 2].Value = phong.TenPhong;
                    worksheet.Cells[row, 3].Value = phong.CoSo?.TenCoSo;
                    worksheet.Cells[row, 4].Value = phong.GiaPhong;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 5].Value = phong.DienTich;
                    worksheet.Cells[row, 6].Value = phong.TrangThai;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"DanhSachPhong_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private bool PhongExists(int id)
        {
            return _context.Phong.Any(e => e.MaPhong == id);
        }
    }
}
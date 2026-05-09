using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HeThongQuanLyPhongTro.Models;
using HeThongQuanLyPhongTro.Data;

namespace HeThongQuanLyPhongTro.Controllers
{
    public class NguoiOHopDongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NguoiOHopDongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var nguoiOHopDong = _context.NguoiOHopDong.Include(n => n.HopDongNavigation);
            return View(await nguoiOHopDong.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nguoiOHopDong = await _context.NguoiOHopDong
                .Include(n => n.HopDongNavigation)
                .FirstOrDefaultAsync(m => m.MaNguoiO == id);
            if (nguoiOHopDong == null) return NotFound();

            return View(nguoiOHopDong);
        }

        public IActionResult Create()
        {
            ViewData["MaHopDong"] = new SelectList(_context.HopDong, "MaHopDong", "MaHopDong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNguoiO,MaHopDong,HoTen,CCCD,SoDienThoai,LaNguoiDaiDien")] NguoiOHopDong nguoiOHopDong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nguoiOHopDong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHopDong"] = new SelectList(_context.HopDong, "MaHopDong", "MaHopDong", nguoiOHopDong.MaHopDong);
            return View(nguoiOHopDong);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nguoiOHopDong = await _context.NguoiOHopDong.FindAsync(id);
            if (nguoiOHopDong == null) return NotFound();
            ViewData["MaHopDong"] = new SelectList(_context.HopDong, "MaHopDong", "MaHopDong", nguoiOHopDong.MaHopDong);
            return View(nguoiOHopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNguoiO,MaHopDong,HoTen,CCCD,SoDienThoai,LaNguoiDaiDien")] NguoiOHopDong nguoiOHopDong)
        {
            if (id != nguoiOHopDong.MaNguoiO) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nguoiOHopDong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NguoiOHopDongExists(nguoiOHopDong.MaNguoiO)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHopDong"] = new SelectList(_context.HopDong, "MaHopDong", "MaHopDong", nguoiOHopDong.MaHopDong);
            return View(nguoiOHopDong);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nguoiOHopDong = await _context.NguoiOHopDong
                .Include(n => n.HopDongNavigation)
                .FirstOrDefaultAsync(m => m.MaNguoiO == id);
            if (nguoiOHopDong == null) return NotFound();

            return View(nguoiOHopDong);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nguoiOHopDong = await _context.NguoiOHopDong.FindAsync(id);
            if (nguoiOHopDong != null)
            {
                _context.NguoiOHopDong.Remove(nguoiOHopDong);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NguoiOHopDongExists(int id)
        {
            return _context.NguoiOHopDong.Any(e => e.MaNguoiO == id);
        }
    }
}
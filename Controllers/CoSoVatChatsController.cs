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
    public class CoSoVatChatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoSoVatChatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CoSoVatChats
        public async Task<IActionResult> Index()
        {
            var coSoVatChats = _context.CoSoVatChat.Include(c => c.PhongNavigation);
            return View(await coSoVatChats.ToListAsync());
        }

        // GET: CoSoVatChats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coSoVatChat = await _context.CoSoVatChat
                .Include(c => c.PhongNavigation)
                .FirstOrDefaultAsync(m => m.MaCSVC == id);
            if (coSoVatChat == null)
            {
                return NotFound();
            }

            return View(coSoVatChat);
        }

        // GET: CoSoVatChats/Create
        public IActionResult Create()
        {
            ViewData["MaPhong"] = new SelectList(_context.Phong, "MaPhong", "TenPhong");
            return View();
        }

        // POST: CoSoVatChats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCSVC,MaPhong,TenThietBi,SoLuong,TinhTrang")] CoSoVatChat coSoVatChat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(coSoVatChat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaPhong"] = new SelectList(_context.Phong, "MaPhong", "TenPhong", coSoVatChat.MaPhong);
            return View(coSoVatChat);
        }

        // GET: CoSoVatChats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coSoVatChat = await _context.CoSoVatChat.FindAsync(id);
            if (coSoVatChat == null)
            {
                return NotFound();
            }
            ViewData["MaPhong"] = new SelectList(_context.Phong, "MaPhong", "TenPhong", coSoVatChat.MaPhong);
            return View(coSoVatChat);
        }

        // POST: CoSoVatChats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaCSVC,MaPhong,TenThietBi,SoLuong,TinhTrang")] CoSoVatChat coSoVatChat)
        {
            if (id != coSoVatChat.MaCSVC)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coSoVatChat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoSoVatChatExists(coSoVatChat.MaCSVC))
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
            ViewData["MaPhong"] = new SelectList(_context.Phong, "MaPhong", "TenPhong", coSoVatChat.MaPhong);
            return View(coSoVatChat);
        }

        // GET: CoSoVatChats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coSoVatChat = await _context.CoSoVatChat
                .Include(c => c.PhongNavigation)
                .FirstOrDefaultAsync(m => m.MaCSVC == id);
            if (coSoVatChat == null)
            {
                return NotFound();
            }

            return View(coSoVatChat);
        }

        // POST: CoSoVatChats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coSoVatChat = await _context.CoSoVatChat.FindAsync(id);
            if (coSoVatChat != null)
            {
                _context.CoSoVatChat.Remove(coSoVatChat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoSoVatChatExists(int id)
        {
            return _context.CoSoVatChat.Any(e => e.MaCSVC == id);
        }
    }
}
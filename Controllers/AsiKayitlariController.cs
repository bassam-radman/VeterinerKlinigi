
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AsiKayitlariController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AsiKayitlariController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kayitlar = await _db.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .AsNoTracking()
                .OrderByDescending(a => a.YapilmaTarihi)
                .ToListAsync();

            var hayvanlar = await _db.EvcilHayvanlar
                .Where(h => h.AktifMi)
                .OrderBy(h => h.Ad)
                .ToListAsync();

            ViewBag.Hayvanlar =
                new SelectList(hayvanlar, "Id", "Ad");

            return View(kayitlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int evcilHayvanId,
            string asiAdi,
            DateTime yapilmaTarihi,
            DateTime? sonrakiDozTarihi,
            string? aciklama)
        {
            bool hayvanVar = await _db.EvcilHayvanlar
                .AnyAsync(h =>
                    h.Id == evcilHayvanId && h.AktifMi);

            if (!hayvanVar ||
                string.IsNullOrWhiteSpace(asiAdi) ||
                asiAdi.Length > 100 ||
                aciklama?.Length > 500 ||
                yapilmaTarihi == default ||
                yapilmaTarihi.Date > DateTime.Today ||
                (sonrakiDozTarihi.HasValue &&
                 sonrakiDozTarihi.Value.Date <= yapilmaTarihi.Date))
            {
                TempData["HataMesaji"] =
                    "Aşı bilgilerini ve tarihleri kontrol ediniz.";

                return RedirectToAction(nameof(Index));
            }

            string ad = asiAdi.Trim();

            bool tekrarVar = await _db.AsiKayitlari
                .AnyAsync(a =>
                    a.EvcilHayvanId == evcilHayvanId &&
                    a.AsiAdi == ad &&
                    a.YapilmaTarihi == yapilmaTarihi.Date);

            if (tekrarVar)
            {
                TempData["HataMesaji"] =
                    "Aynı güne ait aynı aşı kaydı zaten var.";

                return RedirectToAction(nameof(Index));
            }

            _db.AsiKayitlari.Add(new AsiKaydi
            {
                EvcilHayvanId = evcilHayvanId,
                AsiAdi = ad,
                YapilmaTarihi = yapilmaTarihi.Date,
                SonrakiDozTarihi = sonrakiDozTarihi?.Date,
                Aciklama = aciklama?.Trim()
            });

            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Aşı kaydı başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

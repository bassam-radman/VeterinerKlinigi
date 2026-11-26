
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VeterinerHizmetlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VeterinerHizmetlerController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Veteriner ve hizmet seceneklerini hazirla
        private async Task SecenekleriYukle()
        {
            var veterinerler = await _context.Veterinerler
                .Where(v => v.AktifMi)
                .OrderBy(v => v.AdSoyad)
                .ToListAsync();

            var hizmetler = await _context.Hizmetler
                .Where(h => h.AktifMi)
                .OrderBy(h => h.Ad)
                .ToListAsync();

            ViewBag.Veterinerler = new SelectList(
                veterinerler, "Id", "AdSoyad");

            ViewBag.Hizmetler = new SelectList(
                hizmetler, "Id", "Ad");
        }

        // Atanmis hizmetleri listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SecenekleriYukle();

            var kayitlar = await _context.VeterinerHizmetler
                .Include(vh => vh.Veteriner)
                .Include(vh => vh.Hizmet)
                .AsNoTracking()
                .OrderBy(vh => vh.Veteriner.AdSoyad)
                .ThenBy(vh => vh.Hizmet.Ad)
                .ToListAsync();

            return View(kayitlar);
        }

        // Veterinere yeni hizmet ata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int veterinerId, int hizmetId)
        {
            // Veteriner aktif mi?
            bool veterinerVar = await _context.Veterinerler
                .AnyAsync(v =>
                    v.Id == veterinerId && v.AktifMi);

            // Hizmet aktif mi?
            bool hizmetVar = await _context.Hizmetler
                .AnyAsync(h =>
                    h.Id == hizmetId && h.AktifMi);

            if (!veterinerVar || !hizmetVar)
            {
                TempData["HataMesaji"] =
                    "Aktif bir veteriner ve hizmet seciniz.";

                return RedirectToAction(nameof(Index));
            }

            // Ayni atama daha once yapilmis mi?
            bool zatenVar = await _context.VeterinerHizmetler
                .AnyAsync(vh =>
                    vh.VeterinerId == veterinerId &&
                    vh.HizmetId == hizmetId);

            if (zatenVar)
            {
                TempData["HataMesaji"] =
                    "Bu hizmet veterinere zaten atanmis.";

                return RedirectToAction(nameof(Index));
            }

            var yeniKayit = new VeterinerHizmet
            {
                VeterinerId = veterinerId,
                HizmetId = hizmetId
            };

            _context.VeterinerHizmetler.Add(yeniKayit);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["HataMesaji"] =
                    "Atama kaydedilemedi. " +
                    "Ayni kayit mevcut olabilir.";

                return RedirectToAction(nameof(Index));
            }

            TempData["BasariMesaji"] =
                "Hizmet veterinere basariyla atandi.";

            return RedirectToAction(nameof(Index));
        }

        // Veterinerden hizmet atamasini kaldir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.VeterinerHizmetler
                .FindAsync(id);

            if (kayit == null)
            {
                return NotFound();
            }

            // Bu veteriner-hizmet ikilisine ait randevu var mi?
            bool randevuVar = await _context.Randevular
                .AnyAsync(r =>
                    r.VeterinerId == kayit.VeterinerId &&
                    r.HizmetId == kayit.HizmetId);

            if (randevuVar)
            {
                TempData["HataMesaji"] =
                    "Bu atamaya bagli randevular bulunuyor. " +
                    "Atama kaldirilamadi.";

                return RedirectToAction(nameof(Index));
            }

            _context.VeterinerHizmetler.Remove(kayit);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Hizmet atamasi basariyla kaldirildi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

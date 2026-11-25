
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VeterinerlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VeterinerlerController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Tum veterinerleri listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var veterinerler = await _context.Veterinerler
                .AsNoTracking()
                .OrderBy(v => v.AdSoyad)
                .ToListAsync();

            return View(veterinerler);
        }

        // Yeni veteriner ekleme formunu goster
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Veteriner
            {
                AktifMi = true
            });
        }

        // Yeni veterineri veritabanina kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AdSoyad,UzmanlikAlani,Aciklama,AktifMi")]
            Veteriner veteriner)
        {
            if (!ModelState.IsValid)
            {
                return View(veteriner);
            }

            _context.Veterinerler.Add(veteriner);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Veteriner başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        // Veteriner duzenleme formunu goster
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var veteriner = await _context.Veterinerler
                .FindAsync(id);

            if (veteriner == null)
            {
                return NotFound();
            }

            return View(veteriner);
        }

        // Veteriner bilgilerini guncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,AdSoyad,UzmanlikAlani,Aciklama,AktifMi")]
            Veteriner model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var veteriner = await _context.Veterinerler
                .FindAsync(id);

            if (veteriner == null)
            {
                return NotFound();
            }

            veteriner.AdSoyad = model.AdSoyad;
            veteriner.UzmanlikAlani = model.UzmanlikAlani;
            veteriner.Aciklama = model.Aciklama;
            veteriner.AktifMi = model.AktifMi;

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Veteriner başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        // Veteriner silme onay sayfasini goster
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var veteriner = await _context.Veterinerler
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veteriner == null)
            {
                return NotFound();
            }

            return View(veteriner);
        }

        // Veterineri veritabanindan sil
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var veteriner = await _context.Veterinerler
                .FindAsync(id);

            if (veteriner == null)
            {
                return NotFound();
            }

            // Veterinere ait randevu var mi?
            bool randevuVar = await _context.Randevular
                .AnyAsync(r => r.VeterinerId == id);

            // Veterinerin calisma saatleri var mi?
            bool musaitlikVar = await _context.VeterinerMusaitlikler
                .AnyAsync(m => m.VeterinerId == id);

            // Veterinere atanmis hizmet var mi?
            bool hizmetVar = await _context.VeterinerHizmetler
                .AnyAsync(vh => vh.VeterinerId == id);

            // Veterinerin kullanici hesabi bagli mi?
            bool kullaniciBagli =
                !string.IsNullOrEmpty(veteriner.KullaniciId);

            if (randevuVar || musaitlikVar ||
                hizmetVar || kullaniciBagli)
            {
                TempData["HataMesaji"] =
                    "Bu veterinere bagli kayitlar bulunuyor. " +
                    "Silmek yerine pasif duruma getirebilirsiniz.";

                return RedirectToAction(nameof(Index));
            }

            _context.Veterinerler.Remove(veteriner);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Veteriner basariyla silindi.";

            return RedirectToAction(nameof(Index));
        }

    }

}

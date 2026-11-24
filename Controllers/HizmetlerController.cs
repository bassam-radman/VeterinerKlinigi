
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HizmetlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetlerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hizmetleri listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var hizmetler = await _context.Hizmetler
                .AsNoTracking()
                .OrderBy(h => h.Ad)
                .ToListAsync();

            return View(hizmetler);
        }

        // Yeni hizmet ekleme formunu göster
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Hizmet
            {
                AktifMi = true
            });
        }

        // Yeni hizmeti veritabanına kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Ad,Aciklama,SureDakika,Ucret,AktifMi")]
            Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                return View(hizmet);
            }

            _context.Hizmetler.Add(hizmet);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Hizmet başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        // Hizmet duzenleme formunu goster
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);

            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // Hizmet degisikliklerini kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Ad,Aciklama,SureDakika,Ucret,AktifMi")]
            Hizmet model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hizmet = await _context.Hizmetler.FindAsync(id);

            if (hizmet == null)
            {
                return NotFound();
            }

            hizmet.Ad = model.Ad;
            hizmet.Aciklama = model.Aciklama;
            hizmet.SureDakika = model.SureDakika;
            hizmet.Ucret = model.Ucret;
            hizmet.AktifMi = model.AktifMi;

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Hizmet başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }


        // Hizmet silme onay sayfasi
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var hizmet = await _context.Hizmetler
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // Hizmeti veritabanindan sil
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler
                .FindAsync(id);

            if (hizmet == null)
            {
                return NotFound();
            }

            // Hizmetin randevulari var mi?
            bool randevuVar = await _context.Randevular
                .AnyAsync(r => r.HizmetId == id);

            // Hizmet bir veterinere atanmis mi?
            bool veterinerIliskisiVar =
                await _context.VeterinerHizmetler
                    .AnyAsync(vh => vh.HizmetId == id);

            if (randevuVar || veterinerIliskisiVar)
            {
                TempData["HataMesaji"] =
                    "Bu hizmete bagli kayitlar bulundugu icin " +
                    "silme islemi yapilamadi.";

                return RedirectToAction(nameof(Index));
            }

            _context.Hizmetler.Remove(hizmet);
            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Hizmet basariyla silindi.";

            return RedirectToAction(nameof(Index));
        }


    }


}

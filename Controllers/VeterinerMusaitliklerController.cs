
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VeterinerMusaitliklerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VeterinerMusaitliklerController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Veterinerlerin calisma saatlerini listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var musaitlikler = await _context
                .VeterinerMusaitlikler
                .Include(m => m.Veteriner)
                .AsNoTracking()
                .OrderBy(m => m.Veteriner.AdSoyad)
                .ThenBy(m => m.Gun)
                .ToListAsync();

            return View(musaitlikler);
        }

        // Yeni calisma saati ekleme formunu goster
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var veterinerler = await _context.Veterinerler
                .AsNoTracking()
                .Where(v => v.AktifMi)
                .OrderBy(v => v.AdSoyad)
                .ToListAsync();

            ViewBag.Veterinerler = new SelectList(
                veterinerler,
                "Id",
                "AdSoyad"
            );

            var model = new VeterinerMusaitlik
            {
                Gun = DayOfWeek.Monday,
                BaslangicSaati = new TimeSpan(9, 0, 0),
                BitisSaati = new TimeSpan(17, 0, 0),
                AktifMi = true
            };

            return View(model);
        }

        // Calisma saatini veritabanina kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VeterinerId,Gun,BaslangicSaati,BitisSaati,AktifMi")]
            VeterinerMusaitlik model)
        {
            // Formdan gelmeyen navigation property
            ModelState.Remove(nameof(VeterinerMusaitlik.Veteriner));

            // Gecerli gun kontrolu
            if (!Enum.IsDefined(typeof(DayOfWeek), model.Gun))
            {
                ModelState.AddModelError(
                    nameof(model.Gun),
                    "Gecerli bir gun seciniz.");
            }

            // Saat kontrolu
            if (model.BaslangicSaati < TimeSpan.Zero ||
                model.BitisSaati > TimeSpan.FromHours(24) ||
                model.BitisSaati <= model.BaslangicSaati)
            {
                ModelState.AddModelError(
                    nameof(model.BitisSaati),
                    "Bitis saati baslangictan sonra olmalidir.");
            }

            // Veteriner kontrolu
            bool veterinerVar = await _context.Veterinerler
                .AnyAsync(v => v.Id == model.VeterinerId && v.AktifMi);

            if (!veterinerVar)
            {
                ModelState.AddModelError(
                    nameof(model.VeterinerId),
                    "Aktif bir veteriner seciniz.");
            }

            // Calisma saatlerinin cakisma kontrolu
            if (ModelState.IsValid && model.AktifMi)
            {
                var mevcutSaatler = await _context
                    .VeterinerMusaitlikler
                    .Where(m =>
                        m.VeterinerId == model.VeterinerId &&
                        m.Gun == model.Gun &&
                        m.AktifMi)
                    .ToListAsync();

                bool cakismaVar = mevcutSaatler.Any(m =>
                    model.BaslangicSaati < m.BitisSaati &&
                    model.BitisSaati > m.BaslangicSaati);

                if (cakismaVar)
                {
                    ModelState.AddModelError(
                        nameof(model.BaslangicSaati),
                        "Bu saat araliginda veterinerin " +
                        "baska bir calisma kaydi var.");
                }
            }

            // Hata varsa formu tekrar goster
            if (!ModelState.IsValid)
            {
                var veterinerler = await _context.Veterinerler
                    .AsNoTracking()
                    .Where(v => v.AktifMi)
                    .OrderBy(v => v.AdSoyad)
                    .ToListAsync();

                ViewBag.Veterinerler = new SelectList(
                    veterinerler,
                    "Id",
                    "AdSoyad",
                    model.VeterinerId);

                return View(model);
            }

            // Veritabanina kaydet
            _context.VeterinerMusaitlikler.Add(model);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Calisma saati basariyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        // Calisma saati duzenleme formunu goster
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kayit = await _context.VeterinerMusaitlikler
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (kayit == null)
            {
                return NotFound();
            }

            var veterinerler = await _context.Veterinerler
                .AsNoTracking()
                .Where(v => v.AktifMi ||
                            v.Id == kayit.VeterinerId)
                .OrderBy(v => v.AdSoyad)
                .ToListAsync();

            ViewBag.Veterinerler = new SelectList(
                veterinerler,
                "Id",
                "AdSoyad",
                kayit.VeterinerId);

            return View(kayit);
        }

        // Calisma saati degisikliklerini kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,VeterinerId,Gun,BaslangicSaati,BitisSaati,AktifMi")]
            VeterinerMusaitlik model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ModelState.Remove(
                nameof(VeterinerMusaitlik.Veteriner));

            if (!Enum.IsDefined(typeof(DayOfWeek), model.Gun))
            {
                ModelState.AddModelError(
                    nameof(model.Gun),
                    "Gecerli bir gun seciniz.");
            }

            // Saatlerin gecerli oldugunu kontrol et
            if (model.BaslangicSaati < TimeSpan.Zero ||
                model.BitisSaati > TimeSpan.FromHours(24) ||
                model.BitisSaati <= model.BaslangicSaati)
            {
                ModelState.AddModelError(
                    nameof(model.BitisSaati),
                    "Bitis saati baslangictan sonra olmalidir.");
            }

            // Veteriner kontrolu
            bool veterinerVar = await _context.Veterinerler
                .AnyAsync(v => v.Id == model.VeterinerId &&
                               v.AktifMi);

            if (!veterinerVar)
            {
                ModelState.AddModelError(
                    nameof(model.VeterinerId),
                    "Aktif bir veteriner seciniz.");
            }

            // Diger kayitlarla saat cakismasi
            if (ModelState.IsValid && model.AktifMi)
            {
                var digerKayitlar = await _context
                    .VeterinerMusaitlikler
                    .Where(m =>
                        m.Id != id &&
                        m.VeterinerId == model.VeterinerId &&
                        m.Gun == model.Gun &&
                        m.AktifMi)
                    .ToListAsync();

                bool cakismaVar = digerKayitlar.Any(m =>
                    model.BaslangicSaati < m.BitisSaati &&
                    model.BitisSaati > m.BaslangicSaati);

                if (cakismaVar)
                {
                    ModelState.AddModelError(
                        nameof(model.BaslangicSaati),
                        "Bu saat araliginda baska bir " +
                        "calisma kaydi bulunuyor.");
                }
            }

            if (!ModelState.IsValid)
            {
                var veterinerler = await _context.Veterinerler
                    .AsNoTracking()
                    .Where(v => v.AktifMi ||
                                v.Id == model.VeterinerId)
                    .OrderBy(v => v.AdSoyad)
                    .ToListAsync();

                ViewBag.Veterinerler = new SelectList(
                    veterinerler,
                    "Id",
                    "AdSoyad",
                    model.VeterinerId);

                return View(model);
            }

            var kayit = await _context.VeterinerMusaitlikler
                .FindAsync(id);

            if (kayit == null)
            {
                return NotFound();
            }

            kayit.VeterinerId = model.VeterinerId;
            kayit.Gun = model.Gun;
            kayit.BaslangicSaati = model.BaslangicSaati;
            kayit.BitisSaati = model.BitisSaati;
            kayit.AktifMi = model.AktifMi;

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Calisma saati basariyla guncellendi.";

            return RedirectToAction(nameof(Index));
        }
        // Calisma saati silme onay sayfasini goster
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var kayit = await _context.VeterinerMusaitlikler
                .Include(m => m.Veteriner)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (kayit == null)
            {
                return NotFound();
            }

            return View(kayit);
        }

        // Calisma saatini veritabanindan sil
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kayit = await _context.VeterinerMusaitlikler
                .FindAsync(id);

            if (kayit == null)
            {
                return NotFound();
            }

            _context.VeterinerMusaitlikler.Remove(kayit);

            await _context.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Calisma saati basariyla silindi.";

            return RedirectToAction(nameof(Index));
        }


    }
}

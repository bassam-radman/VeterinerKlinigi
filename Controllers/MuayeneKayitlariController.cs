using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MuayeneKayitlariController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MuayeneKayitlariController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kayitlar = await _db.MuayeneKayitlari
                .Include(m => m.Randevu)
                    .ThenInclude(r => r.EvcilHayvan)
                .Include(m => m.Randevu)
                    .ThenInclude(r => r.Veteriner)
                .AsNoTracking()
                .OrderByDescending(m => m.KayitTarihi)
                .ToListAsync();

            var randevular = await _db.Randevular
                .Include(r => r.EvcilHayvan)
                .Include(r => r.Veteriner)
                .AsNoTracking()
                .Where(r =>
                    r.Durum == RandevuDurumu.Tamamlandi &&
                    r.RandevuTarihi <= DateTime.Now)
                .ToListAsync();

            var secenekler = randevular
                .Where(r => !kayitlar.Any(m => m.RandevuId == r.Id))
                .Select(r => new
                {
                    r.Id,
                    Ad = r.EvcilHayvan.Ad + " - " +
                         r.Veteriner.AdSoyad + " - " +
                         r.RandevuTarihi.ToString("dd.MM.yyyy HH:mm")
                })
                .ToList();

            ViewBag.Randevular =
                new SelectList(secenekler, "Id", "Ad");

            return View(kayitlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int randevuId,
            string bulgular,
            string? teshis,
            string? tedavi,
            string? hekimNotu)
        {
            if (string.IsNullOrWhiteSpace(bulgular) ||
                bulgular.Length > 2000 ||
                teshis?.Length > 1000 ||
                tedavi?.Length > 2000 ||
                hekimNotu?.Length > 2000)
            {
                TempData["HataMesaji"] =
                    "Muayene bilgilerini kontrol ediniz.";

                return RedirectToAction(nameof(Index));
            }

            bool gecerliRandevu = await _db.Randevular
                .AnyAsync(r =>
                    r.Id == randevuId &&
                    r.Durum == RandevuDurumu.Tamamlandi &&
                    r.RandevuTarihi <= DateTime.Now);

            if (!gecerliRandevu)
            {
                TempData["HataMesaji"] =
                    "Tamamlanmış geçmiş bir randevu seçiniz.";

                return RedirectToAction(nameof(Index));
            }

            bool kayitVar = await _db.MuayeneKayitlari
                .AnyAsync(m => m.RandevuId == randevuId);

            if (kayitVar)
            {
                TempData["HataMesaji"] =
                    "Bu randevu için muayene kaydı zaten var.";

                return RedirectToAction(nameof(Index));
            }

            _db.MuayeneKayitlari.Add(new MuayeneKaydi
            {
                RandevuId = randevuId,
                Bulgular = bulgular.Trim(),
                Teshis = teshis?.Trim(),
                Tedavi = tedavi?.Trim(),
                HekimNotu = hekimNotu?.Trim(),
                KayitTarihi = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["HataMesaji"] =
                    "Muayene kaydı oluşturulamadı.";

                return RedirectToAction(nameof(Index));
            }

            TempData["BasariMesaji"] =
                "Muayene kaydı başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kayit = await _db.MuayeneKayitlari
                .Include(m => m.Randevu)
                    .ThenInclude(r => r.EvcilHayvan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (kayit == null)
                return NotFound();

            return View(kayit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string bulgular,
            string? teshis,
            string? tedavi,
            string? hekimNotu)
        {
            var kayit = await _db.MuayeneKayitlari
                .FindAsync(id);

            if (kayit == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(bulgular) ||
                bulgular.Length > 2000 ||
                teshis?.Length > 1000 ||
                tedavi?.Length > 2000 ||
                hekimNotu?.Length > 2000)
            {
                TempData["HataMesaji"] =
                    "Muayene bilgilerini kontrol ediniz.";

                return RedirectToAction(nameof(Edit), new { id });
            }

            kayit.Bulgular = bulgular.Trim();
            kayit.Teshis = teshis?.Trim();
            kayit.Tedavi = tedavi?.Trim();
            kayit.HekimNotu = hekimNotu?.Trim();

            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Muayene kaydı başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

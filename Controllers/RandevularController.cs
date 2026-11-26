
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RandevularController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RandevularController(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task SecenekleriYukle()
        {
            ViewBag.Hayvanlar = new SelectList(
                await _db.EvcilHayvanlar
                    .Where(h => h.AktifMi)
                    .OrderBy(h => h.Ad)
                    .ToListAsync(),
                "Id", "Ad");

            ViewBag.Veterinerler = new SelectList(
                await _db.Veterinerler
                    .Where(v => v.AktifMi)
                    .OrderBy(v => v.AdSoyad)
                    .ToListAsync(),
                "Id", "AdSoyad");

            ViewBag.Hizmetler = new SelectList(
                await _db.Hizmetler
                    .Where(h => h.AktifMi)
                    .OrderBy(h => h.Ad)
                    .ToListAsync(),
                "Id", "Ad");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var randevular = await _db.Randevular
                .Include(r => r.EvcilHayvan)
                .Include(r => r.Veteriner)
                .Include(r => r.Hizmet)
                .AsNoTracking()
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();

            return View(randevular);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await SecenekleriYukle();

            return View(new RandevuOlusturModel
            {
                RandevuTarihi =
                    DateTime.Today.AddDays(1).AddHours(10)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            RandevuOlusturModel model)
        {
            if (model.RandevuTarihi <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(model.RandevuTarihi),
                    "Randevu tarihi gelecekte olmalıdır.");
            }

            bool hayvanVar = await _db.EvcilHayvanlar
                .AnyAsync(h =>
                    h.Id == model.EvcilHayvanId && h.AktifMi);

            bool veterinerVar = await _db.Veterinerler
                .AnyAsync(v =>
                    v.Id == model.VeterinerId && v.AktifMi);

            var hizmet = await _db.Hizmetler
                .FirstOrDefaultAsync(h =>
                    h.Id == model.HizmetId && h.AktifMi);

            if (!hayvanVar)
                ModelState.AddModelError(
                    nameof(model.EvcilHayvanId),
                    "Geçerli bir hayvan seçiniz.");

            if (!veterinerVar)
                ModelState.AddModelError(
                    nameof(model.VeterinerId),
                    "Geçerli bir veteriner seçiniz.");

            if (hizmet == null)
                ModelState.AddModelError(
                    nameof(model.HizmetId),
                    "Geçerli bir hizmet seçiniz.");

            if (hizmet != null && veterinerVar && hayvanVar)
            {
                bool yetkili = await _db.VeterinerHizmetler
                    .AnyAsync(vh =>
                        vh.VeterinerId == model.VeterinerId &&
                        vh.HizmetId == model.HizmetId);

                if (!yetkili)
                {
                    ModelState.AddModelError(
                        nameof(model.HizmetId),
                        "Bu hizmet seçilen veterinere atanmamış.");
                }

                var baslangic = model.RandevuTarihi.TimeOfDay;
                var bitis = baslangic.Add(
                    TimeSpan.FromMinutes(hizmet.SureDakika));

                // SQLite TimeSpan karsilastirmalarini
                // bellekte yapalim.
                var calismaSaatleri = await _db.VeterinerMusaitlikler
                    .AsNoTracking()
                    .Where(m =>
                        m.VeterinerId == model.VeterinerId &&
                        m.Gun == model.RandevuTarihi.DayOfWeek &&
                        m.AktifMi)
                    .ToListAsync();

                bool uygunSaat = calismaSaatleri.Any(m =>
                    baslangic >= m.BaslangicSaati &&
                    bitis <= m.BitisSaati);

                if (!uygunSaat)
                {
                    ModelState.AddModelError(
                        nameof(model.RandevuTarihi),
                        "Randevu veterinerin çalışma saatlerine uymuyor.");
                }

                var gunBaslangici = model.RandevuTarihi.Date;
                var gunBitisi = gunBaslangici.AddDays(1);

                var mevcutRandevular = await _db.Randevular
                    .AsNoTracking()
                    .Where(r =>
                        (r.VeterinerId == model.VeterinerId ||
                         r.EvcilHayvanId == model.EvcilHayvanId) &&
                        r.RandevuTarihi >= gunBaslangici &&
                        r.RandevuTarihi < gunBitisi &&
                        r.Durum != RandevuDurumu.Reddedildi &&
                        r.Durum != RandevuDurumu.IptalEdildi)
                    .ToListAsync();

                var yeniBitis = model.RandevuTarihi
                    .AddMinutes(hizmet.SureDakika);

                bool cakismaVar = mevcutRandevular.Any(r =>
                    model.RandevuTarihi <
                        r.RandevuTarihi.AddMinutes(r.SureDakika) &&
                    yeniBitis > r.RandevuTarihi);

                if (cakismaVar)
                {
                    ModelState.AddModelError(
                        nameof(model.RandevuTarihi),
                        "Bu saatte veterinerin veya hayvanın başka randevusu var.");
                }
            }

            if (!ModelState.IsValid)
            {
                await SecenekleriYukle();
                return View(model);
            }

            var yeniRandevu = new Randevu
            {
                EvcilHayvanId = model.EvcilHayvanId,
                VeterinerId = model.VeterinerId,
                HizmetId = model.HizmetId,
                RandevuTarihi = model.RandevuTarihi,
                SureDakika = hizmet!.SureDakika,
                Ucret = hizmet.Ucret,
                Aciklama = model.Aciklama,
                Durum = RandevuDurumu.Bekliyor,
                OlusturulmaTarihi = DateTime.UtcNow
            };

            _db.Randevular.Add(yeniRandevu);
            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Randevu başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumGuncelle(
            int id, RandevuDurumu yeniDurum)
        {
            var randevu = await _db.Randevular.FindAsync(id);

            if (randevu == null)
                return NotFound();

            bool izinli = randevu.Durum switch
            {
                RandevuDurumu.Bekliyor =>
                    yeniDurum == RandevuDurumu.Onaylandi ||
                    yeniDurum == RandevuDurumu.Reddedildi ||
                    yeniDurum == RandevuDurumu.IptalEdildi,

                RandevuDurumu.Onaylandi =>
                    yeniDurum == RandevuDurumu.Tamamlandi ||
                    yeniDurum == RandevuDurumu.IptalEdildi,

                _ => false
            };

            if (!izinli)
            {
                TempData["HataMesaji"] =
                    "Bu durum değişikliğine izin verilmiyor.";

                return RedirectToAction(nameof(Index));
            }

            if (yeniDurum == RandevuDurumu.Tamamlandi &&
                randevu.RandevuTarihi
                    .AddMinutes(randevu.SureDakika) > DateTime.Now)
            {
                TempData["HataMesaji"] =
                    "Randevu saati bitmeden tamamlandı olarak işaretlenemez.";

                return RedirectToAction(nameof(Index));
            }


            randevu.Durum = yeniDurum;
            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Randevu durumu güncellendi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

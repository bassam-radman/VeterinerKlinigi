
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RaporlamaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RaporlamaController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.HayvanSayisi = await _db.EvcilHayvanlar
                .CountAsync();

            ViewBag.VeterinerSayisi = await _db.Veterinerler
                .CountAsync(v => v.AktifMi);

            ViewBag.HizmetSayisi = await _db.Hizmetler
                .CountAsync(h => h.AktifMi);

            var randevular = await _db.Randevular
                .AsNoTracking()
                .Select(r => new
                {
                    r.Durum,
                    r.Ucret
                })
                .ToListAsync();

            ViewBag.ToplamRandevu = randevular.Count;

            ViewBag.Bekleyen = randevular.Count(r =>
                r.Durum == RandevuDurumu.Bekliyor);

            ViewBag.Onaylanan = randevular.Count(r =>
                r.Durum == RandevuDurumu.Onaylandi);

            ViewBag.Tamamlanan = randevular.Count(r =>
                r.Durum == RandevuDurumu.Tamamlandi);

            ViewBag.Iptal = randevular.Count(r =>
                r.Durum == RandevuDurumu.IptalEdildi);

            ViewBag.Reddedilen = randevular.Count(r =>
                r.Durum == RandevuDurumu.Reddedildi);

            ViewBag.TamamlananUcretToplami = randevular
                .Where(r => r.Durum == RandevuDurumu.Tamamlandi)
                .Sum(r => r.Ucret);

            return View();
        }
    }
}

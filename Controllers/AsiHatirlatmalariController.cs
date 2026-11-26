
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AsiHatirlatmalariController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AsiHatirlatmalariController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bugun = DateTime.Today;
            var sonTarih = bugun.AddDays(30);

            var tumAsilar = await _db.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .AsNoTracking()
                .ToListAsync();

            // Ayni hayvan ve asi icin en son kaydi al.
            var sonKayitlar = tumAsilar
                .GroupBy(a => new
                {
                    a.EvcilHayvanId,
                    a.AsiAdi
                })
                .Select(g => g
                    .OrderByDescending(a => a.YapilmaTarihi)
                    .ThenByDescending(a => a.Id)
                    .First())
                .Where(a =>
                    a.SonrakiDozTarihi.HasValue &&
                    a.SonrakiDozTarihi.Value.Date <= sonTarih)
                .OrderBy(a => a.SonrakiDozTarihi)
                .ToList();

            ViewBag.Geciken = sonKayitlar.Count(a =>
                a.SonrakiDozTarihi!.Value.Date < bugun);

            ViewBag.Yaklasan = sonKayitlar.Count(a =>
                a.SonrakiDozTarihi!.Value.Date >= bugun);

            return View(sonKayitlar);
        }
    }
}

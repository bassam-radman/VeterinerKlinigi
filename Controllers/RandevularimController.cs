
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize]
    public class RandevularimController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RandevularimController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Kullanicinin kendi randevularini listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kullaniciId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Challenge();
            }

            var randevular = await _db.Randevular
                .Include(r => r.EvcilHayvan)
                .Include(r => r.Veteriner)
                .Include(r => r.Hizmet)
                .Where(r =>
                    r.EvcilHayvan.SahipId == kullaniciId)
                .AsNoTracking()
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();

            return View(randevular);
        }

        // Kullanicinin kendi randevusunu iptal etmesi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var kullaniciId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Challenge();
            }

            // Sadece kullaniciya ait randevuyu bul
            var randevu = await _db.Randevular
                .Include(r => r.EvcilHayvan)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.EvcilHayvan.SahipId == kullaniciId);

            if (randevu == null)
            {
                return NotFound();
            }

            // Gecmis randevular iptal edilemez
            if (randevu.RandevuTarihi <= DateTime.Now)
            {
                TempData["HataMesaji"] =
                    "Gecmis randevular iptal edilemez.";

                return RedirectToAction(nameof(Index));
            }

            // Sadece bekleyen veya onaylanmis
            // randevular iptal edilebilir
            if (randevu.Durum != RandevuDurumu.Bekliyor &&
                randevu.Durum != RandevuDurumu.Onaylandi)
            {
                TempData["HataMesaji"] =
                    "Bu randevu iptal edilemiyor.";

                return RedirectToAction(nameof(Index));
            }

            // Randevuyu iptal et
            randevu.Durum = RandevuDurumu.IptalEdildi;

            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Randevunuz basariyla iptal edildi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

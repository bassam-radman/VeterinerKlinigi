
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize]
    public class SaglikGecmisiController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SaglikGecmisiController(
            ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kullaniciId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Challenge();
            }

            // Sadece kullanicinin hayvanlarina ait
            // muayene kayitlarini getir
            var muayeneler = await _db.MuayeneKayitlari
                .Include(m => m.Randevu)
                    .ThenInclude(r => r.EvcilHayvan)
                .Where(m =>
                    m.Randevu.EvcilHayvan.SahipId == kullaniciId)
                .AsNoTracking()
                .OrderByDescending(m => m.KayitTarihi)
                .ToListAsync();

            // Sadece kullanicinin hayvanlarina ait
            // asi kayitlarini getir
            var asilar = await _db.AsiKayitlari
                .Include(a => a.EvcilHayvan)
                .Where(a =>
                    a.EvcilHayvan.SahipId == kullaniciId)
                .AsNoTracking()
                .OrderByDescending(a => a.YapilmaTarihi)
                .ToListAsync();

            var model = new SaglikGecmisiViewModel
            {
                Muayeneler = muayeneler,
                Asilar = asilar
            };

            return View(model);
        }
    }
}

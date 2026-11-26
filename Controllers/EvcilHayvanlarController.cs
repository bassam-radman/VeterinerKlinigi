
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EvcilHayvanlarController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EvcilHayvanlarController(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task SahipleriYukle(string? secili = null)
        {
            var kullanicilar = await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewBag.Sahipler = new SelectList(
                kullanicilar, "Id", "Email", secili);
        }

        private async Task BilgileriDogrula(EvcilHayvan model)
        {
            bool sahipVar = await _db.Users
                .AnyAsync(u => u.Id == model.SahipId);

            if (!sahipVar)
                ModelState.AddModelError(
                    nameof(model.SahipId),
                    "Geçerli bir hayvan sahibi seçiniz.");

            if (model.DogumTarihi.Year < 1900 ||
                model.DogumTarihi.Date > DateTime.Today)
                ModelState.AddModelError(
                    nameof(model.DogumTarihi),
                    "Geçerli bir doğum tarihi giriniz.");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var hayvanlar = await _db.EvcilHayvanlar
                .AsNoTracking()
                .OrderBy(h => h.Ad)
                .ToListAsync();

            ViewBag.SahipEpostalar = await _db.Users
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            return View(hayvanlar);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await SahipleriYukle();

            return View("Form", new EvcilHayvan
            {
                DogumTarihi = DateTime.Today,
                AktifMi = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Ad,Tur,Irk,DogumTarihi,Kilo,SahipId,AktifMi")]
            EvcilHayvan model)
        {
            await BilgileriDogrula(model);

            if (!ModelState.IsValid)
            {
                await SahipleriYukle(model.SahipId);
                return View("Form", model);
            }

            _db.EvcilHayvanlar.Add(model);
            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Evcil hayvan başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hayvan = await _db.EvcilHayvanlar.FindAsync(id);

            if (hayvan == null)
                return NotFound();

            await SahipleriYukle(hayvan.SahipId);

            return View("Form", hayvan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Ad,Tur,Irk,DogumTarihi,Kilo,SahipId,AktifMi")]
            EvcilHayvan model)
        {
            if (id != model.Id)
                return BadRequest();

            var hayvan = await _db.EvcilHayvanlar.FindAsync(id);

            if (hayvan == null)
                return NotFound();

            await BilgileriDogrula(model);

            if (!ModelState.IsValid)
            {
                await SahipleriYukle(model.SahipId);
                return View("Form", model);
            }

            hayvan.Ad = model.Ad;
            hayvan.Tur = model.Tur;
            hayvan.Irk = model.Irk;
            hayvan.DogumTarihi = model.DogumTarihi;
            hayvan.Kilo = model.Kilo;
            hayvan.SahipId = model.SahipId;
            hayvan.AktifMi = model.AktifMi;

            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Evcil hayvan başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hayvan = await _db.EvcilHayvanlar.FindAsync(id);

            if (hayvan == null)
                return NotFound();

            bool randevuVar = await _db.Randevular
                .AnyAsync(r => r.EvcilHayvanId == id);

            bool asiVar = await _db.AsiKayitlari
                .AnyAsync(a => a.EvcilHayvanId == id);

            if (randevuVar || asiVar)
            {
                TempData["HataMesaji"] =
                    "Bu hayvana bağlı kayıtlar var. Silinemez.";

                return RedirectToAction(nameof(Index));
            }

            _db.EvcilHayvanlar.Remove(hayvan);
            await _db.SaveChangesAsync();

            TempData["BasariMesaji"] =
                "Evcil hayvan başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

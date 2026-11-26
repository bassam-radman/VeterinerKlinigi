
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KullanicilarController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public KullanicilarController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var kullanicilar = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var model = new List<KullaniciRolViewModel>();

            foreach (var kullanici in kullanicilar)
            {
                var roller = await _userManager
                    .GetRolesAsync(kullanici);

                model.Add(new KullaniciRolViewModel
                {
                    Id = kullanici.Id,
                    Email = kullanici.Email ?? "",
                    EmailOnayli = kullanici.EmailConfirmed,
                    Roller = roller.ToList()
                });
            }

            ViewBag.KendiId = _userManager.GetUserId(User);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolDegistir(
            string id, string rol)
        {
            string[] izinliRoller =
            {
                "Admin",
                "Veteriner",
                "Uye"
            };

            if (!izinliRoller.Contains(rol) ||
                !await _roleManager.RoleExistsAsync(rol))
            {
                TempData["HataMesaji"] =
                    "Geçersiz veya tanımsız rol.";

                return RedirectToAction(nameof(Index));
            }

            var kullanici = await _userManager
                .FindByIdAsync(id);

            if (kullanici == null)
                return NotFound();

            // Yonetici kendi rolunu degistiremez.
            if (kullanici.Id == _userManager.GetUserId(User))
            {
                TempData["HataMesaji"] =
                    "Kendi yönetici rolünüzü değiştiremezsiniz.";

                return RedirectToAction(nameof(Index));
            }

            var mevcutRoller = await _userManager
                .GetRolesAsync(kullanici);

            // Sistemdeki son Admin korunur.
            if (mevcutRoller.Contains("Admin") &&
                rol != "Admin")
            {
                var yoneticiler = await _userManager
                    .GetUsersInRoleAsync("Admin");

                if (yoneticiler.Count <= 1)
                {
                    TempData["HataMesaji"] =
                        "Son yönetici hesabının rolü değiştirilemez.";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Yeni rolu once ekle.
            if (!mevcutRoller.Contains(rol))
            {
                var ekleme = await _userManager
                    .AddToRoleAsync(kullanici, rol);

                if (!ekleme.Succeeded)
                {
                    TempData["HataMesaji"] =
                        "Rol eklenemedi.";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Eski rolleri kaldir.
            var kaldirilacak = mevcutRoller
                .Where(r => r != rol)
                .ToList();

            if (kaldirilacak.Count > 0)
            {
                var silme = await _userManager
                    .RemoveFromRolesAsync(
                        kullanici, kaldirilacak);

                if (!silme.Succeeded)
                {
                    TempData["HataMesaji"] =
                        "Eski roller kaldırılamadı. " +
                        "Kullanıcı rollerini kontrol edin.";

                    return RedirectToAction(nameof(Index));
                }
            }

            await _userManager.UpdateSecurityStampAsync(kullanici);

            TempData["BasariMesaji"] =
                "Kullanıcı rolü başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

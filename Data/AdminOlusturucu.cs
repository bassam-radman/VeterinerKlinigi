
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace VeterinerKlinigi.Data
{
    public static class AdminOlusturucu
    {
        public static async Task AdminiOlusturAsync(
            IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var provider = scope.ServiceProvider;

            var environment = provider
                .GetRequiredService<IHostEnvironment>();

            // Admin hesabi sadece gelistirme ortaminda olusturulur.
            if (!environment.IsDevelopment())
                return;

            var configuration = provider
                .GetRequiredService<IConfiguration>();

            var email = configuration["AdminSeed:Email"];
            var password = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Admin bilgileri User Secrets icinde bulunamadi.");
            }

            var userManager = provider
                .GetRequiredService<UserManager<IdentityUser>>();

            var roleManager = provider
                .GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                throw new InvalidOperationException(
                    "Admin rolu bulunamadi.");
            }

            var mevcutKullanici =
                await userManager.FindByEmailAsync(email);

            if (mevcutKullanici != null)
            {
                if (!await userManager.IsInRoleAsync(
                    mevcutKullanici, "Admin"))
                {
                    throw new InvalidOperationException(
                        "Bu e-posta zaten kullaniliyor. " +
                        "Mevcut hesaba otomatik Admin yetkisi verilmedi.");
                }

                return;
            }

            var admin = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };


            // Yalnizca yerel gelistirme ortami icin
            // hocanin istedigi demo sifresini olusturuyoruz.
            admin.PasswordHash = userManager.PasswordHasher
                .HashPassword(admin, password);

            var sonuc = await userManager.CreateAsync(admin);


            if (!sonuc.Succeeded)
            {
                var hatalar = string.Join("; ",
                    sonuc.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    "Admin hesabi olusturulamadi: " + hatalar);
            }

            var rolSonucu = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!rolSonucu.Succeeded)
            {
                throw new InvalidOperationException(
                    "Admin rolu kullaniciya atanamadi.");
            }
        }
    }
}

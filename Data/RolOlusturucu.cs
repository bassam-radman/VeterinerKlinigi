
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace VeterinerKlinigi.Data
{
    public static class RolOlusturucu
    {
        public static async Task RolleriOlusturAsync(
            IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roller =
            {
                "Admin",
                "Veteriner",
                "Uye"
            };

            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    var sonuc = await roleManager.CreateAsync(
                        new IdentityRole(rol));

                    if (!sonuc.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"{rol} rolü oluşturulamadı.");
                    }
                }
            }
        }
    }
}


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Hayvan sahibi - Evcil hayvan
            builder.Entity<EvcilHayvan>()
                .HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.SahipId)
                .OnDelete(DeleteBehavior.Restrict);

            // Veteriner - Kullanici hesabi
            builder.Entity<Veteriner>()
                .HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Veteriner>()
                .HasIndex(x => x.KullaniciId)
                .IsUnique();

            // Evcil hayvan - Asi kayitlari
            builder.Entity<AsiKaydi>()
                .HasOne(x => x.EvcilHayvan)
                .WithMany()
                .HasForeignKey(x => x.EvcilHayvanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu - Evcil hayvan
            builder.Entity<Randevu>()
                .HasOne(x => x.EvcilHayvan)
                .WithMany()
                .HasForeignKey(x => x.EvcilHayvanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu - Veteriner
            builder.Entity<Randevu>()
                .HasOne(x => x.Veteriner)
                .WithMany()
                .HasForeignKey(x => x.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu - Hizmet
            builder.Entity<Randevu>()
                .HasOne(x => x.Hizmet)
                .WithMany()
                .HasForeignKey(x => x.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Veteriner - Musaitlik
            builder.Entity<VeterinerMusaitlik>()
                .HasOne(x => x.Veteriner)
                .WithMany()
                .HasForeignKey(x => x.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Muayene - Randevu
            builder.Entity<MuayeneKaydi>()
                .HasOne(x => x.Randevu)
                .WithMany()
                .HasForeignKey(x => x.RandevuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Her randevu icin en fazla bir muayene kaydi
            builder.Entity<MuayeneKaydi>()
                .HasIndex(x => x.RandevuId)
                .IsUnique();

            // Veteriner - Hizmet iliskisi
            builder.Entity<VeterinerHizmet>()
                .HasOne(x => x.Veteriner)
                .WithMany()
                .HasForeignKey(x => x.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VeterinerHizmet>()
                .HasOne(x => x.Hizmet)
                .WithMany()
                .HasForeignKey(x => x.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ayni hizmet veterinere iki kez atanamaz
            builder.Entity<VeterinerHizmet>()
                .HasIndex(x => new { x.VeterinerId, x.HizmetId })
                .IsUnique();
        }


        public DbSet<Hizmet> Hizmetler { get; set; }
            = null!;

        public DbSet<Veteriner> Veterinerler { get; set; }
            = null!;

        public DbSet<EvcilHayvan> EvcilHayvanlar { get; set; }
            = null!;

        public DbSet<AsiKaydi> AsiKayitlari { get; set; }
            = null!;

        public DbSet<Randevu> Randevular { get; set; }
            = null!;

        public DbSet<VeterinerMusaitlik> VeterinerMusaitlikler { get; set; }
            = null!;

        public DbSet<MuayeneKaydi> MuayeneKayitlari { get; set; }
            = null!;

        public DbSet<VeterinerHizmet> VeterinerHizmetler { get; set; }
            = null!;
    }
}


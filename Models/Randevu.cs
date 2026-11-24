
using System;
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public enum RandevuDurumu
    {
        Bekliyor = 0,
        Onaylandi = 1,
        Reddedildi = 2,
        Tamamlandi = 3,
        IptalEdildi = 4
    }

    public class Randevu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        [DataType(DataType.DateTime)]
        public DateTime RandevuTarihi { get; set; }

        [Range(1, 480)]
        public int SureDakika { get; set; }

        [Range(typeof(decimal), "0", "1000000")]
        public decimal Ucret { get; set; }

        public RandevuDurumu Durum { get; set; }
            = RandevuDurumu.Bekliyor;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Range(1, int.MaxValue)]
        public int EvcilHayvanId { get; set; }

        public EvcilHayvan EvcilHayvan { get; set; }
            = null!;

        [Range(1, int.MaxValue)]
        public int VeterinerId { get; set; }

        public Veteriner Veteriner { get; set; }
            = null!;

        [Range(1, int.MaxValue)]
        public int HizmetId { get; set; }

        public Hizmet Hizmet { get; set; }
            = null!;

        public DateTime OlusturulmaTarihi { get; set; }
            = DateTime.UtcNow;
    }
}

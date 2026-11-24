
using System;
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class MuayeneKaydi
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int RandevuId { get; set; }

        public Randevu Randevu { get; set; } = null!;

        [Required(ErrorMessage = "Muayene bulguları zorunludur.")]
        [StringLength(2000)]
        public string Bulgular { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Teshis { get; set; }

        [StringLength(2000)]
        public string? Tedavi { get; set; }

        [StringLength(2000)]
        public string? HekimNotu { get; set; }

        public DateTime KayitTarihi { get; set; }
            = DateTime.UtcNow;
    }
}

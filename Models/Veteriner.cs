
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class Veteriner
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(100)]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        public string? KullaniciId { get; set; }

        public bool AktifMi { get; set; } = true;
    }
}

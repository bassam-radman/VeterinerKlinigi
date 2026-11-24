
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Range(1, 480, ErrorMessage = "Süre 1 ile 480 dakika arasında olmalıdır.")]
        public int SureDakika { get; set; }


        [Range(typeof(decimal), "0.01", "1000000",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Ücret 0,01 ile 1.000.000 arasında olmalıdır.")]
        public decimal Ucret { get; set; }


        public bool AktifMi { get; set; } = true;
    }
}

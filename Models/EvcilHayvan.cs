
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class EvcilHayvan
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hayvan adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hayvan türü zorunludur.")]
        [StringLength(50)]
        public string Tur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hayvan ırkı zorunludur.")]
        [StringLength(100)]
        public string Irk { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime DogumTarihi { get; set; }


        [Range(0.01, 1000.0,
            ErrorMessage = "Kilo 0,01 ile 1000 arasında olmalıdır.")]
        public decimal Kilo { get; set; }


        [Required]
        public string SahipId { get; set; } = string.Empty;

        public string? FotografYolu { get; set; }

        public bool AktifMi { get; set; } = true;
    }
}

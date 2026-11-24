
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class AsiKaydi
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100)]
        public string AsiAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aşı tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime YapilmaTarihi { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SonrakiDozTarihi { get; set; }

        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Required]
        public int EvcilHayvanId { get; set; }

        public EvcilHayvan EvcilHayvan { get; set; } = null!;
    }
}

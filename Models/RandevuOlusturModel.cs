
using System;
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class RandevuOlusturModel
    {
        [Range(1, int.MaxValue)]
        public int EvcilHayvanId { get; set; }

        [Range(1, int.MaxValue)]
        public int VeterinerId { get; set; }

        [Range(1, int.MaxValue)]
        public int HizmetId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RandevuTarihi { get; set; }

        [StringLength(500)]
        public string? Aciklama { get; set; }
    }
}

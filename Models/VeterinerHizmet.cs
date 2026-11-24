
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class VeterinerHizmet
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int VeterinerId { get; set; }

        public Veteriner Veteriner { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int HizmetId { get; set; }

        public Hizmet Hizmet { get; set; } = null!;
    }
}

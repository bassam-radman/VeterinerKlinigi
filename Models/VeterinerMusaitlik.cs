
using System;
using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models
{
    public class VeterinerMusaitlik : IValidatableObject
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int VeterinerId { get; set; }

        public Veteriner Veteriner { get; set; } = null!;

        [Required]
        public DayOfWeek Gun { get; set; }

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }

        public bool AktifMi { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (BitisSaati <= BaslangicSaati)
            {
                yield return new ValidationResult(
                    "Bitiş saati başlangıç saatinden sonra olmalıdır.",
                    new[] { nameof(BitisSaati) }
                );
            }

            if (BaslangicSaati < TimeSpan.Zero ||
                BitisSaati > TimeSpan.FromHours(24))
            {
                yield return new ValidationResult(
                    "Saatler 00:00 ile 24:00 arasında olmalıdır.",
                    new[] { nameof(BaslangicSaati), nameof(BitisSaati) }
                );
            }
        }
    }
}

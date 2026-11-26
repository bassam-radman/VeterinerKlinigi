
namespace VeterinerKlinigi.Models
{
    public class SaglikGecmisiViewModel
    {
        public List<MuayeneKaydi> Muayeneler { get; set; }
            = new();

        public List<AsiKaydi> Asilar { get; set; }
            = new();
    }
}

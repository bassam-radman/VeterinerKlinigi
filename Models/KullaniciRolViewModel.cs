
namespace VeterinerKlinigi.Models
{
    public class KullaniciRolViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public bool EmailOnayli { get; set; }

        public List<string> Roller { get; set; } = new();
    }
}

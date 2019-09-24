using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Profile
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O campo login é requerido")]
        public string login { get; set; }

        [Required(ErrorMessage = "O campo senha é requerido")]
        public string senha { get; set; }
    }
}
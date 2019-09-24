using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Profile
{
    public class recoverLoginViewModel
    {
        [Required(ErrorMessage = "Campo email é requerido")]
        [MaxLength(100, ErrorMessage = "Campo email é limitado em 100 caracteres")]
        [EmailAddress]
        public string email { get; set; }
    }
}
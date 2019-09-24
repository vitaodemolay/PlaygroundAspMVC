using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Home
{
    public class CheckEmailViewModel
    {
        [Required(ErrorMessage = "Campo código é requerido")]
        [MaxLength(15, ErrorMessage = "Campo código é limitado em 15 caracteres")]
        public string Code { get; set; }
    }
}
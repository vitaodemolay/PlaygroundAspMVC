using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models
{
    public class recoverpasswordViewModel
    {
        [Required(ErrorMessage = "Campo login é requerido")]
        [MaxLength(30, ErrorMessage = "Campo login é limitado em 30 caracteres")]
        public string login { get; set; }


    }
}
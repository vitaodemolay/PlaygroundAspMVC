using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Home
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Campo senha atual é requerido")]
        [MaxLength(20, ErrorMessage = "Campo senha atual é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        public string oldpassword { get; set; }

        [Required(ErrorMessage = "Campo nova senha é requerido")]
        [MaxLength(20, ErrorMessage = "Campo nova senha é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required(ErrorMessage = "Campo confirmação é requerido")]
        [MaxLength(20, ErrorMessage = "Campo confirmação é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "Os campos nova senha e confirmação não coincidem")]
        public string confirm { get; set; }
    }
}
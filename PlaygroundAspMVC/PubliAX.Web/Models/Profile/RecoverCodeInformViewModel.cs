using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Profile
{
    public class RecoverCodeInformViewModel
    {
        [Required(ErrorMessage ="Campo código é requerido")]
        [MaxLength(15, ErrorMessage = "Campo código é limitado em 15 caracteres")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Campo senha é requerido")]
        [MaxLength(20, ErrorMessage = "Campo senha é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required(ErrorMessage = "Campo confirmação é requerido")]
        [MaxLength(20, ErrorMessage = "Campo confirmação é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "Os campos senha e confirmação não coincidem")]
        public string confirm { get; set; }

        [Required]
        public Guid userId { get; set; }
    }
}
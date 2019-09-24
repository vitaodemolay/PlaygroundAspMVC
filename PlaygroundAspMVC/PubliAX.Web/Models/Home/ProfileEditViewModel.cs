using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Home
{
    public class ProfileEditViewModel
    {
        [Required]
        public Guid userId { get; set; }

        [Required(ErrorMessage = "Campo login é requerido")]
        [MaxLength(30, ErrorMessage = "Campo login é limitado em 30 caracteres")]
        public string login { get; set; }

        [Required(ErrorMessage = "Campo login é requerido")]
        [MaxLength(50, ErrorMessage = "Campo nome é limitado em 50 caracteres")]
        public string name { get; set; }

        [Required(ErrorMessage = "Campo email é requerido")]
        [MaxLength(100, ErrorMessage = "Campo email é limitado em 100 caracteres")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "Campo celular é requerido")]
        [MaxLength(15, ErrorMessage = "Campo celular é limitado em 15 caracteres")]
        [Phone]
        public string celphone { get; set; }
    }
}
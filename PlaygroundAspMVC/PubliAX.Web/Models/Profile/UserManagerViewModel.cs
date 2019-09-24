using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Profile
{
    public class UserManagerViewModel
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

        [Required(ErrorMessage = "O tipo de pessoa deve ser informado")]
        public char typePerson { get; set; }

        [Required(ErrorMessage = "O codigo geral de pessoa fisica ou juridica deve ser informado (CPF ou CNPJ)")]
        [MaxLength(20, ErrorMessage = "O Campo é limitado em 20 caracteres")]
        public string cgc { get; set; }

        public string provName { get; set; }

        [Required]
        public bool cleanUserForn { get; set; } = false;

        [Required]
        public bool userAdmin { get; set; } = false;
    }
}
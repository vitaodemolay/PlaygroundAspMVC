using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models
{
    public class TermAcceptAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var term = Convert.ToBoolean(value);
            return term;
        }
    }

    public class SignupViewModel
    {
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

        [Required(ErrorMessage = "Campo senha é requerido")]
        [MaxLength(20, ErrorMessage = "Campo senha é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required(ErrorMessage = "Campo confirmação é requerido")]
        [MaxLength(20, ErrorMessage = "Campo confirmação é limitado em 20 caracteres")]
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "Os campos senha e confirmação não coincidem")]
        public string confirm { get; set; }

        [Required(ErrorMessage = "O tipo de pessoa deve ser informado")]
        public char typePerson { get; set; }

        [Required(ErrorMessage = "O codigo geral de pessoa fisica ou juridica deve ser informado (CPF ou CNPJ)")]
        [MaxLength(20, ErrorMessage = "O Campo é limitado em 20 caracteres")]
        public string cgc { get; set; }

        [Display(Name = "Aceita Termo")]
        [TermAccept(ErrorMessage = "Para realizar o cadastro você porecisa aceitar os termos")]
        public bool termAccept { get; set; }

        public string provider { get; set; }
        public string providerKey { get; set; }
    }
}
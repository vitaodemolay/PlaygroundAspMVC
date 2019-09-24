using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Profile
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Campo Provider é requerido")]
        public string provider { get; set; }

        [Required(ErrorMessage = "Campo ProviderKey é requerido")]
        public string providerKey { get; set; }

        [Required(ErrorMessage = "Campo login é requerido")]
        public string name { get; set; }

        [Required(ErrorMessage = "Campo email é requerido")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "O tipo de pessoa deve ser informado")]
        public char typePerson { get; set; }

        [Required(ErrorMessage = "O codigo geral de pessoa fisica ou juridica deve ser informado (CPF ou CNPJ)")]
        [MaxLength(20, ErrorMessage = "O Campo é limitado em 20 caracteres")]
        public string cgc { get; set; }
    }
}
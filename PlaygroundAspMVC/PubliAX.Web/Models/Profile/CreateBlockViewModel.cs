using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Profile
{
    public class CreateBlockViewModel
    {
        [Required(ErrorMessage = "O tipo de bloqueio deve ser informado")]
        public char typePerson { get; set; }

        [Required(ErrorMessage = "O valor para Email, CPF ou CNPJ deve ser informado")]
        [MaxLength(100, ErrorMessage = "O Campo é limitado em 100 caracteres")]
        public string cgc { get; set; }
    }
}
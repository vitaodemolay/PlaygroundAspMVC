using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Prov
{
    public class ProvRegisterViewModel
    {
        [Display(Name ="Nome")]
        [Required(ErrorMessage ="O campo nome não foi informado")]
        [MaxLength(50,ErrorMessage = "Campo nome é limitado em 50 caracteres")]
        public string name { get; set; }

        [Display(Name = "Razao Social")]
        [MaxLength(50, ErrorMessage = "Campo razao social é limitado em 50 caracteres")]
        public string razao { get; set; }

        [Required(ErrorMessage = "O campo cpf/cnpj não foi informado")]
        [MaxLength(18, ErrorMessage = "Campo cpf/cnpj é limitado em 18 caracteres")]
        public string cgc { get; set; }

        [Display(Name = "Tipo de Pessoa")]
        [Required(ErrorMessage = "O campo tipo de pessoa não foi informado")]
        public char typePerson { get; set; }

        [Display(Name = "Insc. Municipal")]
        [MaxLength(20, ErrorMessage = "Campo insc. municipal é limitado em 20 caracteres")]
        public string insMun { get; set; }

        [Display(Name = "Cidade")]
        [Required(ErrorMessage = "O campo cidade não foi informado")]
        [MaxLength(50, ErrorMessage = "Campo cidade é limitado em 50 caracteres")]
        public string city { get; set; }

        [Display(Name = "Endereço")]
        [Required(ErrorMessage = "O campo endereço não foi informado")]
        [MaxLength(100, ErrorMessage = "Campo endereço é limitado em 100 caracteres")]
        public string address { get; set; }

        [Display(Name = "UF")]
        [Required(ErrorMessage = "O campo UF não foi informado")]
        [MaxLength(2, ErrorMessage = "Campo UF é limitado em 2 caracteres")]
        public string state { get; set; }

        [Display(Name = "Bairro")]
        [Required(ErrorMessage = "O campo bairro não foi informado")]
        [MaxLength(20, ErrorMessage = "Campo bairro é limitado em 20 caracteres")]
        public string neighbor { get; set; }

        [Display(Name = "CEP")]
        [Required(ErrorMessage = "O campo CEP não foi informado")]
        [MaxLength(9, ErrorMessage = "Campo CEP é limitado em 9 caracteres")]
        public string zipcode { get; set; }

        [Display(Name = "Telefone")]
        [MaxLength(23, ErrorMessage = "Campo telefone é limitado em 23 caracteres")]
        public string phone { get; set; }

        [Display(Name = "Email")]
        [MaxLength(50, ErrorMessage = "Campo email é limitado em 50 caracteres")]
        public string email { get; set; }

        [Display(Name = "Banco")]
        [Required(ErrorMessage = "O campo Banco não foi informado")]
        public string banc { get; set; }

        [Display(Name = "Agencia")]
        [MaxLength(14, ErrorMessage = "Campo agencia é limitado em 14 caracteres")]
        [Required(ErrorMessage = "O campo Agencia não foi informado")]
        public string agency { get; set; }

        [Display(Name = "Conta")]
        [MaxLength(30, ErrorMessage = "Campo conta é limitado em 30 caracteres")]
        [Required(ErrorMessage = "O campo Conta não foi informado")]
        public string account { get; set; }

        [Display(Name = "Tipo de Conta")]
        [Required(ErrorMessage = "O campo Tipo de Conta não foi informado")]
        public string accountType { get; set; }

        [Display(Name = "Nome do Favorecido")]
        [MaxLength(50, ErrorMessage = "Campo nome do Favorecido é limitado em 50 caracteres")]
        public string favName { get; set; }

        [Display(Name = "CPF")]
        [MaxLength(15, ErrorMessage = "Campo cpf é limitado em 15 caracteres")]
        public string favCpf { get; set; }

    }
}
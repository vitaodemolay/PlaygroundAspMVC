
using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Prov
{
    public class RelateProvViewModel
    {

        [Required(ErrorMessage ="O tipo de pessoa deve ser informado")]
        public char typePerson { get; set; }

        [Required(ErrorMessage ="O codigo geral de pessoa fisica ou juridica deve ser informado (CPF ou CNPJ)")]
        [MaxLength(20,ErrorMessage ="O Campo é limitado em 20 caracteres")] 
        public string cgc { get; set; }

    }
}
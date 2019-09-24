using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Invite
{
    public class InviteRegisterViewModel
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "O campo email não foi informado")]
        [MaxLength(100, ErrorMessage = "Campo email é limitado em 100 caracteres")]
        public string email { get; set; }


        [Display(Name = "Data Expiração")]
        public DateTime? expiration { get; set; }

        [Display(Name = "Token")]
        [MaxLength(12, ErrorMessage ="O campo Token está inválido")]
        public string token { get; set; }
    }
}
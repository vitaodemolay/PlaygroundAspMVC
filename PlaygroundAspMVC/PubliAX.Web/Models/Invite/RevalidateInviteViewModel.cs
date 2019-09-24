using System;
using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Invite
{
    public class RevalidateInviteViewModel
    {
        [Required]
        public Guid inviteId { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "O campo email não foi informado")]
        [MaxLength(100, ErrorMessage = "Campo email é limitado em 100 caracteres")]
        public string email { get; set; }

        [Display(Name = "Data Expiração")]
        public DateTime? expiration { get; set; }
    }
}
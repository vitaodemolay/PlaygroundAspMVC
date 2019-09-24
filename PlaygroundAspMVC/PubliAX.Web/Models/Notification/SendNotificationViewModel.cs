using System.ComponentModel.DataAnnotations;

namespace PubliAX.Web.Models.Notification
{
    public class SendNotificationViewModel
    {
        [Display(Name = "Destinatario")]
        [Required(ErrorMessage = "O campo destinatario não foi informado")]
        public string recipient { get; set; }

        [Display(Name = "Assunto")]
        [Required(ErrorMessage = "O campo assunto não foi informado")]
        [MaxLength(50, ErrorMessage = "Campo assunto é limitado em 50 caracteres")]
        public string subject { get; set; }

        [Display(Name = "Mensagem")]
        [Required(ErrorMessage = "O campo mensagem não foi informado")]
        [MaxLength(2000, ErrorMessage = "Campo mensagem é limitado em 2000 caracteres")]
        public string body { get; set; }
    }
}
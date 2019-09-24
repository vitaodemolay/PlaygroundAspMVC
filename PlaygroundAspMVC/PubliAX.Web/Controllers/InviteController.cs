using MediatR;
using PubliAX.CQRS.Invite.Register;
using PubliAX.CQRS.Invite.Update;
using PubliAX.Domain.DTO;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Models.Invite;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Message;

namespace PubliAX.Web.Controllers
{
    [Authorize]
    public class InviteController : Controller
    {
        private IDapper<UserDto> UserRepository { get; set; }
        private IDapper<NotificationDto> NotificRepository { get; set; }
        private IDapper<InviteDto> InviteRepository { get; set; }
        private IMediator mediator { get; set; }
        private IMsgService msgService { get; set; }
        private string InviteText = @"<html><head></head><body>
	                                    <p><strong>Olá!</strong></p>
                                        <p>Você foi convidado por {0} para se cadastrar no Portal Winn, 
                                           clique no link <a href='{1}' target='_blank' rel='noopener'>{1}</a>
                                           ou vá até o endereço  <a href='{2}' target='_blank' rel='noopener'>{2}</a> 
                                           e utilize o token <strong>{3}</strong> para se cadastrar. </p>
                                       </ body ></ html >";


        private UserDto LoginUser
        {
            get
            {
                UserDto userData = null;
                if (User.Identity.IsAuthenticated)
                {
                    var aux = HttpContext.User as CustomPrincipal;
                    userData = this.UserRepository.Find(f => f.userId == aux.userId);
                    userData.notifications = this.NotificRepository.FindAll(f => f.userId == userData.userId && f.status == Domain.Enum.entityStatus.enable);
                }

                return userData;
            }
        }

        public InviteController(IDapper<UserDto> UserRepository,
             IDapper<NotificationDto> NotificRepository,
            IDapper<InviteDto> InviteRepository,
            IMediator mediator,
            IMsgService msgService)
        {
            this.mediator = mediator;
            this.msgService = msgService;
            this.UserRepository = UserRepository;
            this.NotificRepository = NotificRepository;
            this.InviteRepository = InviteRepository;
        }

        // GET: Invite
        //public ActionResult Index()
        //{
        //    ViewData["LoginUser"] = LoginUser;
        //    var model = this.InviteRepository.FindAll(f => f.status != Domain.Enum.entityStatus.blocked && f.status != Domain.Enum.entityStatus.excluded);
        //    return View(model);
        //}

        //public ActionResult Create()
        //{
        //    ViewData["LoginUser"] = LoginUser;
        //    return View();
        //}

        private bool ValideteInviteRegisterViewModel(InviteRegisterViewModel model)
        {
            bool result = TryValidateModel(model);
            if (!string.IsNullOrEmpty(model.token) && model.token.Length != 12)
            {
                ModelState.AddModelError("token", "O campo Token está inválido");
                result = false;
            }

            return result;
        }

        //    [HttpPost]
        //    public async Task<ActionResult> Create(InviteRegisterViewModel model)
        //    {
        //        if (ValideteInviteRegisterViewModel(model))
        //        {
        //            var result = await this.mediator.Send(new InviteRegisterCommand
        //            {
        //                email = model.email,
        //                expiration = model.expiration,
        //                token = model.token,
        //                userId = LoginUser.userId,
        //            });

        //            if (result.success)
        //            {
        //                this.msgService.sendMail("Convite Portal Winn",
        //                                         string.Format(this.InviteText,
        //                                                       LoginUser.name,
        //                                                       this.Url.Action("signup", "Profile", new { token = result.token }, Request.Url.Scheme),
        //                                                       this.Url.Action("signup", "Profile", null, Request.Url.Scheme),
        //                                                       result.token
        //                                                       ),
        //                                         new string[] { model.email }
        //                                         );
        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ModelState.Clear();
        //                ModelState.AddModelError("", "Falha no registro do convite, por favor revise os dados e tente novamente.");
        //            }

        //        }

        //        ViewData["LoginUser"] = LoginUser;
        //        return View();

        //    }


        //    public ActionResult Edit(Guid id)
        //    {
        //        ViewData["LoginUser"] = LoginUser;
        //        var dados = this.InviteRepository.Find(f => f.inviteId == id);
        //        var model = new RevalidateInviteViewModel
        //        {
        //            inviteId = id,
        //            email = dados.email,
        //            expiration = dados.expiration
        //        };
        //        return View(model);
        //    }

        //    [HttpPost]
        //    public async Task<ActionResult> Edit(RevalidateInviteViewModel model)
        //    {
        //        if (TryValidateModel(model))
        //        {
        //            var result = await this.mediator.Send(new InviteUpdateCommand
        //            {
        //                inviteId = model.inviteId,
        //                status = Domain.Enum.entityStatus.enable,
        //                email = model.email,
        //                expiration = model.expiration,
        //                userId = LoginUser.userId,
        //            });

        //            var inv = this.InviteRepository.Find(f => f.inviteId == model.inviteId);

        //            if (result.success)
        //            {
        //                this.msgService.sendMail("Convite Portal Winn",
        //                                         string.Format(this.InviteText,
        //                                                       LoginUser.name,
        //                                                       this.Url.Action("signup", "Profile", new { token = inv.token }, Request.Url.Scheme),
        //                                                       this.Url.Action("signup", "Profile", null, Request.Url.Scheme),
        //                                                       inv.token
        //                                                       ),
        //                                         new string[] { model.email }
        //                                         );
        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ModelState.Clear();
        //                ModelState.AddModelError("", "Falha no registro do convite, por favor revise os dados e tente novamente.");
        //            }
        //        }

        //        ViewData["LoginUser"] = LoginUser;
        //        return View(model);
        //    }

        //    public ActionResult Delete(Guid id)
        //    {
        //        var inv = this.InviteRepository.Find(f => f.inviteId == id);
        //        if (inv != null)
        //        {
        //            var result = this.mediator.Send(new InviteUpdateCommand
        //            {
        //                inviteId = inv.inviteId,
        //                status = Domain.Enum.entityStatus.excluded,
        //            }).Result;

        //        }

        //        return RedirectToAction("Index");
        //    }

        //    public ActionResult Resend(Guid id)
        //    {
        //        var inv = this.InviteRepository.Find(f => f.inviteId == id);
        //        if (inv != null)
        //        {
        //            this.msgService.sendMail("Convite Portal Winn",
        //                                         string.Format(this.InviteText,
        //                                                       LoginUser.name,
        //                                                       this.Url.Action("signup", "Profile", new { token = inv.token }, Request.Url.Scheme),
        //                                                       this.Url.Action("signup", "Profile", null, Request.Url.Scheme),
        //                                                       inv.token
        //                                                       ),
        //                                         new string[] { inv.email }
        //                                         );
        //        }

        //        return RedirectToAction("Index");
        //    }
    }
}
using MediatR;
using PubliAX.CQRS.User.Password;
using PubliAX.CQRS.User.Update;
using PubliAX.Domain.DTO;
using PubliAX.Domain.Enum;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Models.Home;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Message;

namespace PubliAX.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IDapper<UserDto> UserRepository { get; set; }
        private IDapper<NotificationDto> NotificRepository { get; set; }
        private IDapper<BlockedPersonDto> BlockedRepository { get; set; }
        private IMediator mediator { get; set; }
        private IMsgService msgService { get; set; }
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

        public HomeController(IDapper<UserDto> UserRepository,
            IDapper<NotificationDto> NotificRepository,
            IDapper<BlockedPersonDto> BlockedRepository,
            IMediator mediator,
            IMsgService msgService)
        {
            this.UserRepository = UserRepository;
            this.NotificRepository = NotificRepository;
            this.BlockedRepository = BlockedRepository;
            this.mediator = mediator;
            this.msgService = msgService;
        }

        public ActionResult Index()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }

        public ActionResult profile()
        {
            ViewData["LoginUser"] = LoginUser;
            return View(this.LoginUser);
        }

        public ActionResult profileEdit()
        {
            ViewData["LoginUser"] = LoginUser;
            ProfileEditViewModel model = new ProfileEditViewModel
            {
                userId = this.LoginUser.userId,
                name = this.LoginUser.name,
                login = this.LoginUser.login,
                celphone = this.LoginUser.celphone,
                email = this.LoginUser.email,
            };
            return View(model);
        }


        private bool TryValidateProfileEditModel(ProfileEditViewModel model)
        {
            ModelState.Clear();

            if (!string.IsNullOrEmpty(model.email))
            {
                if (this.UserRepository.Find(f => f.email == model.email) != null)
                    ModelState.AddModelError("email", $"O Email já está cadastrado no sistema. Entre em contato com a Winn");
                else if (this.BlockedRepository.Find(f => f.type == blockedType.email && f.value == model.email) != null)
                    ModelState.AddModelError("email", $"O Email está com problemas para registro no sistema. Entre em contato com a Winn");
            }

            return TryValidateModel(model);
        }

        [HttpPost]
        public async Task<ActionResult> profileEdit(ProfileEditViewModel model)
        {
            if (TryValidateProfileEditModel(model))
            {
                var result = await this.mediator.Send(new UserUpdateCommand
                {
                    userId = model.userId,
                    name = model.name,
                    login = model.login,
                    email = model.email,
                    celphone = model.celphone
                });

                if (result.success)
                    return RedirectToAction("profile", "home");
                else
                {
                    ViewData["LoginUser"] = LoginUser;
                    ModelState.AddModelError("", "Falha no processo de gravação, por favor revise os dados e tente novamente.");
                    return View();
                }

            }
            else
            {
                ViewData["LoginUser"] = LoginUser;
                return View(model);
            }
        }

        public ActionResult changePassword()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> changePassword(ChangePasswordViewModel model)
        {
            if (TryValidateModel(model))
            {
                var result = await this.mediator.Send(new UserPasswordChangeCommand
                {
                    userId = this.LoginUser.userId,
                    oldPassword = model.oldpassword,
                    newPassword = model.confirm
                });

                if (result.success)
                    return RedirectToAction("profile", "home");
                else
                {
                    ViewData["LoginUser"] = LoginUser;
                    ModelState.AddModelError("", "Senha Atual Inválida");
                    return View();
                }
            }
            else
            {
                ViewData["LoginUser"] = LoginUser;
                return View();
            }
        }


        public async Task<ActionResult> CheckEmail()
        {
            ViewData["LoginUser"] = LoginUser;
            var result = await this.mediator.Send(new UserCheckContactGerateCodeCommand
            {
                userId = this.LoginUser.userId,
            });

            try
            {
                if (!result.success) throw new System.Exception();
                if (!this.msgService.sendMail("Portal Winn: Código de validação de e-mail",
                                                 $"O código para a validação do seu email no Portal Winn é {result.description}",
                                                 new string[] { this.LoginUser.email })) throw new System.Exception();

            }
            catch (System.Exception)
            {
                throw new System.Exception("Falha no processo de envio de codigo de validação por email");
            }

            return View();
        }


        [HttpPost]
        public async Task<ActionResult> CheckEmail(CheckEmailViewModel model)
        {
            if (TryValidateModel(model))
            {
                var result = await this.mediator.Send(new UserCheckContactCommand
                {
                    userId = this.LoginUser.userId,
                    emailchecked = true,
                    celphonechecked = false,
                    code = model.Code,
                });

                if (result.success)
                    return RedirectToAction("CheckMailSuccess");
                else
                {
                    ViewData["LoginUser"] = LoginUser;
                    ModelState.AddModelError("", "Código de verficação inválido. Caso tenha dúvida do código, tente clicar em reenviar");
                    return View();
                }
            }
            else
            {
                ViewData["LoginUser"] = LoginUser;
                return View();
            }
        }

        public ActionResult CheckMailSuccess()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }


        public ActionResult changeAvatar()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> changeAvatar(HttpPostedFileBase file)
        {
            if (file != null)
            {
                string basepath = "~/Libs/Img/avatars";
                string pic = LoginUser.userId.ToString();
                pic += System.IO.Path.GetExtension(file.FileName);
                string path = System.IO.Path.Combine(
                                       Server.MapPath(basepath), pic);
                // file is uploaded
                file.SaveAs(path);

                var result = await this.mediator.Send(new UserChangeImageCommand
                {
                    userId = LoginUser.userId,
                    avatar = $"{basepath}/{pic}",
                });
                    
            }
            // after successfully uploading redirect the user
            return RedirectToAction("profile", "Home");
        }

    }
}
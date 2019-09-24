using MediatR;
using Microsoft.Owin.Security;
using PubliAX.CQRS.BlockedPerson.Register;
using PubliAX.CQRS.BlockedPerson.Remove;
using PubliAX.CQRS.ExternalAuthentication.Logon;
using PubliAX.CQRS.ExternalAuthentication.Register;
using PubliAX.CQRS.Notification.Register;
using PubliAX.CQRS.User.Delete;
using PubliAX.CQRS.User.Logon;
using PubliAX.CQRS.User.Password;
using PubliAX.CQRS.User.Register;
using PubliAX.CQRS.User.Update;
using PubliAX.Domain.DTO;
using PubliAX.Domain.Enum;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Configs.HttpResult;
using PubliAX.Web.Configs.Toolkit;
using PubliAX.Web.Models;
using PubliAX.Web.Models.Profile;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Logger;
using VMRCPACK.Infrastructure.Interfaces.Message;

namespace PubliAX.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private IDapper<UserDto> UserRepository { get; set; }
        private IDapper<InviteDto> InviteRepository { get; set; }
        private IDapper<NotificationDto> NotificRepository { get; set; }
        private IDapper<BlockedPersonDto> BlockedRepository { get; set; }
        private IDapper<FornecedorDto> FornRepository { get; set; }
        private IDapper<ExternalAuthenticationDto> externalAuthenticationRepository { get; set; }
        private IMediator mediator { get; set; }
        private IMsgService msgService { get; set; }
        private ILogger logger { get; set; }

        private string AtualTermUrl { get; set; }
        private string TokenPadrao { get; set; }
        private string LoginAdmin { get; set; }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

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

        private UserDto UserSystem
        {
            get
            {
                var userData = this.UserRepository.Find(f => f.login == this.LoginAdmin);
                return userData;
            }
        }

        public ProfileController(IDapper<UserDto> UserRepository,
            IDapper<NotificationDto> NotificRepository,
            IDapper<InviteDto> InviteRepository,
            IDapper<BlockedPersonDto> BlockedRepository,
            IDapper<FornecedorDto> FornRepository,
            IDapper<ExternalAuthenticationDto> externalAuthenticationRepository,
            IMediator mediator,
            IMsgService msgService,
            ILogger logger)
        {
            this.mediator = mediator;
            this.msgService = msgService;
            this.UserRepository = UserRepository;
            this.NotificRepository = NotificRepository;
            this.InviteRepository = InviteRepository;
            this.BlockedRepository = BlockedRepository;
            this.externalAuthenticationRepository = externalAuthenticationRepository;
            this.FornRepository = FornRepository;
            this.logger = logger;

            this.AtualTermUrl = System.Configuration.ConfigurationManager.AppSettings["AtualTermUrl"];
            this.TokenPadrao = System.Configuration.ConfigurationManager.AppSettings["TokenPadrao"];
            this.LoginAdmin = System.Configuration.ConfigurationManager.AppSettings["LoginAdmin"];
        }

        // GET: Profile
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult signin(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> signin(LoginViewModel model, string returnUrl = null)
        {
            if (TryValidateModel(model))
            {
                var result = await this.mediator.Send(new UserLogonValidateCommand
                {
                    login = model.login,
                    password = model.senha,
                });

                if (result.success)
                {
                    SerializeCustomPrincipal serializemodel = new SerializeCustomPrincipal
                    {
                        name = result.name,
                        role = result.role,
                        login = result.login,
                        userId = result.userId,
                        email = result.email,
                        celphone = result.celphone
                    };

                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    string userData = serializer.Serialize(serializemodel);

                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                                                        1,
                                                                        model.login,
                                                                        DateTime.Now,
                                                                        DateTime.Now.AddMinutes(20),
                                                                        false,
                                                                        userData);
                    string encTicket = FormsAuthentication.Encrypt(authTicket);
                    HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                    Response.Cookies.Add(faCookie);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("senha", "Usuário ou senha inválidos");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Profile", new { ReturnUrl = returnUrl }));
        }

        // GET: /Profile/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            var externalProviderDto = externalAuthenticationRepository.Find(f => f.provider == loginInfo.Login.LoginProvider && f.providerKey == loginInfo.Login.ProviderKey);

            if (externalProviderDto == null)
            {
                ViewBag.ReturnUrl = returnUrl;

                var model = new ExternalLoginConfirmationViewModel
                {
                    email = loginInfo.Email,
                    name = loginInfo.ExternalIdentity.Name,
                    provider = loginInfo.Login.LoginProvider,
                    providerKey = loginInfo.Login.ProviderKey,
                    typePerson = 'F',
                };
                return View("ExternalLoginConfirmation", model);
            }

            return LoginWithUserId(externalProviderDto.userId, returnUrl);
        }


        private ActionResult LoginWithUserId(Guid userId, string returnUrl = null)
        {
            var result = this.mediator.Send(new ExternalAuthenticationLogonCommand
            {
                userId = userId,
            }).GetAwaiter().GetResult();

            if (result.success)
            {
                SerializeCustomPrincipal serializemodel = new SerializeCustomPrincipal
                {
                    name = result.name,
                    role = result.role,
                    login = result.login,
                    userId = result.userId,
                    email = result.email,
                    celphone = result.celphone
                };

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                string userData = serializer.Serialize(serializemodel);

                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                                                    1,
                                                                    result.login,
                                                                    DateTime.Now,
                                                                    DateTime.Now.AddMinutes(20),
                                                                    false,
                                                                    userData);
                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                Response.Cookies.Add(faCookie);

                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction("Index", "Home");

                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Error");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction("Index", "Home");

                return Redirect(returnUrl);
            }


            if (TryValidateModel(model))
            {

                var _cgc = model.cgc.Replace(".", "").Replace("-", "").Replace("/", "").Replace("_", "");
                var sql = Resource.GetScriptToGetUserByCPF(_cgc);
                var cnpfj = model.typePerson == 'F' ? "CPF" : "CNPJ";

                if (this.BlockedRepository.Find(f => f.type == blockedType.cnpfj && f.value == _cgc) != null)
                {
                    ModelState.Clear();
                    ModelState.AddModelError("cgc", $"O {cnpfj} está com problemas para registro no sistema. Entre em contato com a Winn");
                    return View();
                }


                var user = this.UserRepository.Find(sql);
                if(user != null)
                {
                    var result = await this.mediator.Send(new ExternalAuthenticationRegisterCommand {
                        userId = user.userId,
                        provider = model.provider,
                        providerKey = model.providerKey,
                    });

                    if (result.success)
                        return LoginWithUserId(user.userId, returnUrl);

                    return RedirectToAction("Index", "Error");
                }


                return RedirectToAction("signupbyexternal", new { provider = model.provider, providerKey = model.providerKey, returnUrl = returnUrl });
            }
            else
            {
                return View();
            }
        }
       

        private void SetViewBagTerm()
        {
            var _term = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(this.AtualTermUrl));
            ViewBag.Term = _term;
        }

        [AllowAnonymous]
        public ActionResult signup(string returnUrl = null)
        {
            SetViewBagTerm();

            ViewBag.ReturnUrl = returnUrl;

            var model = new SignupViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult signupbyexternal(string provider, string providerKey, string returnUrl = null)
        {
            SetViewBagTerm();

            ViewBag.ReturnUrl = returnUrl;

            var model = new SignupViewModel() {
                provider = provider,
                providerKey = providerKey,
            };
            return View("signup", model);
        }

        private bool TryValidateSignupModel(SignupViewModel model)
        {
            ModelState.Clear();
            if (!model.termAccept)
                ModelState.AddModelError("termAccept", "O termo deve ser aceito para continuar");

            if (!string.IsNullOrEmpty(model.login))
            {
                if (this.UserRepository.Find(f => f.login == model.login) != null)
                    ModelState.AddModelError("login", $"O Login já está cadastrado no sistema. Escolha outro");

            }

            if (!string.IsNullOrEmpty(model.cgc))
            {
                var _cgc = model.cgc.Replace(".", "").Replace("-", "").Replace("/", "").Replace("_", "");
                var sql = Resource.GetScriptToGetUserByCPF(_cgc);
                var cnpfj = model.typePerson == 'F' ? "CPF" : "CNPJ";
                if (this.UserRepository.Find(sql) != null)
                    ModelState.AddModelError("cgc", $"O {cnpfj} já está cadastrado no sistema. Entre em contato com a Winn");
                else if (this.BlockedRepository.Find(f => f.type == blockedType.cnpfj && f.value == _cgc) != null)
                    ModelState.AddModelError("cgc", $"O {cnpfj} está com problemas para registro no sistema. Entre em contato com a Winn");
            }

            if (!string.IsNullOrEmpty(model.email))
            {
                if (this.UserRepository.Find(f => f.email == model.email) != null)
                    ModelState.AddModelError("email", $"O Email já está cadastrado no sistema. Entre em contato com a Winn");
                else if (this.BlockedRepository.Find(f => f.type == blockedType.email && f.value == model.email) != null)
                    ModelState.AddModelError("email", $"O Email está com problemas para registro no sistema. Entre em contato com a Winn");
            }

            return TryValidateModel(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> signup(SignupViewModel model)
        {
            SetViewBagTerm();
            try
            {

                if (TryValidateSignupModel(model))
                {

                    var result = await this.mediator.Send(new UserRegisterCommand
                    {
                        name = model.name,
                        login = model.login,
                        celphone = model.celphone,
                        email = model.email,
                        password = model.password,
                        termAccept = this.AtualTermUrl,
                        type = model.typePerson == 'F' ? personType.personal : personType.business,
                        cnpfj = model.cgc.Replace(".", "").Replace("-", "").Replace("/", "").Replace("_", ""),
                    });
                    if (result.success)
                    {
                        if (!string.IsNullOrEmpty(model.provider) && !string.IsNullOrEmpty(model.providerKey))
                        {
                            await this.mediator.Send(new ExternalAuthenticationRegisterCommand
                            {
                                userId = (result.userId).GetValueOrDefault(Guid.Empty),
                                provider = model.provider,
                                providerKey = model.providerKey,
                            });
                        }

                        await this.mediator.Send(new NotificationRegisterCommand
                        {
                            userIdDest = (Guid)result.userId,
                            body = "Seja Bem Vindo ao Portal Winn!",
                            subject = "Bem Vindo",
                            sender = "Sistema",
                        });


                        SerializeCustomPrincipal serializemodel = new SerializeCustomPrincipal
                        {
                            name = model.name,
                            role = "User",
                            login = model.login,
                            userId = result.userId.Value,
                            email = model.email,
                            celphone = model.celphone
                        };

                        JavaScriptSerializer serializer = new JavaScriptSerializer();

                        string userData = serializer.Serialize(serializemodel);

                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                                                            1,
                                                                            model.login,
                                                                            DateTime.Now,
                                                                            DateTime.Now.AddMinutes(20),
                                                                            false,
                                                                            userData);
                        string encTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                        Response.Cookies.Add(faCookie);


                        return RedirectToAction("welcome", "Profile");
                    }
                    else
                    {
                        ModelState.Clear();
                        ModelState.AddModelError("", "Falha no cadastro: " + result.description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        public ActionResult welcome()
        {
            return View();
        }

        public ActionResult signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("signin", "Profile");
        }

        [AllowAnonymous]
        public ActionResult loginrecover()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> loginrecover(recoverLoginViewModel model)
        {
            if (TryValidateModel(model))
            {
                var user = this.UserRepository.Find(f => f.email == model.email);
                if (user == null)
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Email informado não consta no sistema");
                    return View();
                }


                if (!this.msgService.sendMail("Portal Winn: Recuperação de Login",
                                             $"Você solicitou a recuperação de login do Portal Winn. Seu login é {user.login}",
                                             new string[] { user.email }))
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Falha no processo de envio de recuperação de login por email");
                    return View();
                }

                return RedirectToAction("signin", "Profile");
            }
            else
            {
                return View();
            }
        }

        [AllowAnonymous]
        public ActionResult passwordrecover()
        {
            return View();
        }


        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> passwordrecover(recoverpasswordViewModel model)
        {
            if (TryValidateModel(model))
            {
                var user = this.UserRepository.Find(f => f.login == model.login);
                if (user == null)
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Login informado não consta no sistema");
                    return View();
                }

                var result = await this.mediator.Send(new UserPswRecoverGerateCodeCommand
                {
                    userId = user.userId
                });

                if (result.success)
                {
                    if (!this.msgService.sendMail("Portal Winn: Código para Recuperação de conta",
                                                 $"O código para a recuperar sua conta no Portal Winn é {result.description}",
                                                 new string[] { user.email }))
                    {
                        ModelState.Clear();
                        ModelState.AddModelError("", "Falha no processo de envio de codigo de recuperação de conta por email");
                        return View();
                    }

                    return RedirectToAction("passwordrecovercodeinform", "Profile", new { login = user.login });
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Falha no cadastro: " + result.description);
                    return View();
                }
            }
            else
            {
                return View();
            }
        }


        [AllowAnonymous]
        public ActionResult passwordrecovercodeinform(string login)
        {
            var user = this.UserRepository.Find(f => f.login == login);
            if (user == null)
            {
                ModelState.Clear();
                ModelState.AddModelError("", "Login informado não consta no sistema");
                return View();
            }

            return View(new RecoverCodeInformViewModel { userId = user.userId });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> passwordrecovercodeinform(RecoverCodeInformViewModel model)
        {
            if (TryValidateModel(model))
            {
                var result = await this.mediator.Send(new UserPasswordRecoverCommand
                {
                    userId = model.userId,
                    newPassword = model.confirm,
                    recoverCode = model.Code,
                });

                if (result.success)
                {
                    return RedirectToAction("signin", "Profile");
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Falha na recuperação : " + result.description);
                    return View();
                }
            }
            else
            {
                return View();
            }
        }


        public ActionResult listUser()
        {
            ViewData["LoginUser"] = LoginUser;
            var model = this.UserRepository.FindAll(f => f.status != Domain.Enum.entityStatus.excluded);
            return View(model);
        }

        public async Task<ActionResult> changeStatus(Guid userId, entityStatus status)
        {

            var result = await this.mediator.Send(new UserChangeStatusCommand
            {
                userId = userId,
                status = status
            });

            return RedirectToAction("listUser");
        }


        public ActionResult delete(Guid userId)
        {
            ViewData["LoginUser"] = LoginUser;
            var model = this.UserRepository.Find(f => f.userId == userId);
            return View(model);
        }

        [HttpPost]
        public ActionResult deletePost(Guid userId)
        {
            try
            {
                var result = this.mediator.Send(new UserDeleteCommand
                {
                    userId = userId,
                }).Result;
                if (result.success)
                    return RedirectToAction("listUser", "Profile");

                throw new Exception(result.description);

            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message);
                ViewData["LoginUser"] = LoginUser;
                return RedirectToAction("Index", "Error");
            }
        }

        public ActionResult userManager(Guid userId)
        {
            ViewData["LoginUser"] = LoginUser;
            var user = this.UserRepository.Find(f => f.userId == userId);
            var fornName = string.Empty;
            if (user.codforn >= 0)
            {
                var forn = this.FornRepository.Find(f => f.CODIGO == user.codforn);
                fornName = forn.NOME;
            }
            UserManagerViewModel model = new UserManagerViewModel
            {
                userId = user.userId,
                login = user.login,
                name = user.name,
                cgc = user.cnpfj,
                typePerson = user.type == personType.business ? 'J' : 'F',
                celphone = user.celphone,
                email = user.email,
                userAdmin = user.role == "User" ? false : true,
                cleanUserForn = user.codforn >= 0 ? false : true,
                provName = fornName,
            };
            return View(model);
        }

        private bool TryValidateUserManagerModel(UserManagerViewModel model)
        {
            ModelState.Clear();

            if (!string.IsNullOrEmpty(model.login) && this.UserRepository.Find(f => f.userId == model.userId).login != model.login)
            {
                if (this.UserRepository.Find(f => f.login == model.login && f.userId != model.userId) != null)
                    ModelState.AddModelError("login", $"O Login já está cadastrado no sistema. Escolha outro");

            }

            if (!string.IsNullOrEmpty(model.cgc) && model.cleanUserForn)
            {
                var _cgc = model.cgc.Replace(".", "").Replace("-", "").Replace("/", "");
                var sql = $"SELECT * FROM [dbo].[VW$User001] WHERE [dbo].ONLYNUMBER(cnpfj) = '{_cgc}' AND userId <> '{model.userId}'";
                var cnpfj = model.typePerson == 'F' ? "CPF" : "CNPJ";
                if (this.UserRepository.Find(sql) != null)
                    ModelState.AddModelError("cgc", $"O {cnpfj} já está cadastrado no sistema. Entre em contato com a Winn");
                else if (this.BlockedRepository.Find(f => f.type == blockedType.cnpfj && f.value == _cgc) != null)
                    ModelState.AddModelError("cgc", $"O {cnpfj} está com problemas para registro no sistema. Entre em contato com a Winn");
            }

            if (!string.IsNullOrEmpty(model.email) && this.UserRepository.Find(f => f.userId == model.userId).email != model.email)
            {
                if (this.UserRepository.Find(f => f.email == model.email && f.userId != model.userId) != null)
                    ModelState.AddModelError("email", $"O Email já está cadastrado no sistema. Entre em contato com a Winn");
                else if (this.BlockedRepository.Find(f => f.type == blockedType.email && f.value == model.email) != null)
                    ModelState.AddModelError("email", $"O Email está com problemas para registro no sistema. Entre em contato com a Winn");
            }

            return TryValidateModel(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> userManager(UserManagerViewModel model)
        {
            try
            {
                if (TryValidateUserManagerModel(model))
                {

                    var result = await this.mediator.Send(new UserUpdateByManagerCommand
                    {
                        userId = model.userId,
                        name = model.name,
                        login = model.login,
                        email = model.email,
                        celphone = model.celphone,
                        cleanUserForn = model.cleanUserForn,
                        userAdmin = model.userAdmin,
                        cnpfj = model.cgc.Replace(".", "").Replace("-", "").Replace("/", ""),
                        type = model.typePerson == 'F' ? personType.personal : personType.business,
                    });

                    if (result.success)
                    {
                        return RedirectToAction("listUser", "Profile");
                    }
                    else
                    {
                        ModelState.Clear();
                        ModelState.AddModelError("", "Falha na alteracao de usuário: " + result.description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);
            }

            ViewData["LoginUser"] = LoginUser;
            return View(model);
        }

        public ActionResult blacklist()
        {
            ViewData["LoginUser"] = LoginUser;
            var model = this.BlockedRepository.FindAll();
            if (model != null)
                model = model.OrderBy(f => f.registerdate).ToList();

            return View(model);
        }

        public ActionResult createBlock()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> createBlock(CreateBlockViewModel model)
        {
            try
            {
                if (TryValidateModel(model))
                {
                    var _value = string.Empty;
                    if (model.typePerson == 'E')
                        _value = model.cgc;
                    else
                        _value = model.cgc.Replace(".", "").Replace("-", "").Replace("/", "");

                    var result = await this.mediator.Send(new BlockedPersonRegisterCommand
                    {
                        value = _value,
                        type = model.typePerson == 'E' ? blockedType.email : blockedType.cnpfj,
                    });

                    if (result.success)
                    {
                        return RedirectToAction("blacklist", "Profile");
                    }
                    else
                    {
                        ModelState.Clear();
                        ModelState.AddModelError("", "Falha na criação de bloqueio: " + result.description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);
            }

            ViewData["LoginUser"] = LoginUser;
            return View(model);
        }

        public ActionResult deleteBlock(Guid blockId)
        {
            ViewData["LoginUser"] = LoginUser;
            var block = this.BlockedRepository.Find(f => f.blockedId == blockId);
            ViewData["BlockValue"] = block.value;
            ViewData["BlockId"] = blockId;
            return View();
        }

        [HttpPost]
        public ActionResult deleteBlockConfirmed(Guid blockId)
        {
            try
            {
                var result = this.mediator.Send(new BlockedPersonRemoveCommand
                {
                    blockedId = blockId,
                }).Result;
                if (result.success)
                    return RedirectToAction("blacklist", "Profile");

                throw new Exception(result.description);

            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message);
                ViewData["LoginUser"] = LoginUser;
                return RedirectToAction("Index", "Error");
            }
        }
    }
}
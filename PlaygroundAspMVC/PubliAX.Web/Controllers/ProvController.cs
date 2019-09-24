using MediatR;
using PubliAX.CQRS.Fornecedor.Register;
using PubliAX.CQRS.Fornecedor.Update;
using PubliAX.CQRS.User.Update;
using PubliAX.Domain.DTO;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Models.Prov;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Logger;

namespace PubliAX.Web.Controllers
{
    [Authorize]
    public class ProvController : Controller
    {
        private IDapper<UserDto> UserRepository { get; set; }
        private IDapper<NotificationDto> NotificRepository { get; set; }
        private IDapper<FornecedorDto> FornRepository { get; set; }
        private IDapper<BancoDto> BancoRepository { get; set; }
        private IDapper<PagamentoDto> PgtosRepository { get; set; }
        private IMediator mediator { get; set; }
        private ILogger logger { get; set; }
        private string UsuManuPubli { get; set; }
        public string[] MailsFornChangeNotification { get; }

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



        private void GerateBagState()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var arrayState = new string[] { "AC", "AL", "AM", "AP", "BA", "CE", "DF", "ES", "GO", "MA", "MG", "MS", "MT", "PA", "PB", "PE", "PI", "PR", "RJ", "RN", "RO", "RR", "RS", "SC", "SE", "SP", "TO" };
            foreach (var item in arrayState)
            {
                list.Add(new SelectListItem
                {
                    Text = item,
                    Value = item
                });
            }

            ViewBag.State = list;
        }

        private void GerateBagAccountType(string SelectValue)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem
            {
                Text = "Conta Corrente",
                Value = "Co",
                Selected = string.IsNullOrEmpty(SelectValue) || SelectValue == "Co" || SelectValue == "C" ? true : false,
            });

            list.Add(new SelectListItem
            {
                Text = "Conta Poupança",
                Value = "Po",
                Selected = !string.IsNullOrEmpty(SelectValue) && (SelectValue == "Po" || SelectValue == "P") ? true : false,
            });

            list.Add(new SelectListItem
            {
                Text = "Conta Fácil (CEF)",
                Value = "CF",
                Selected = !string.IsNullOrEmpty(SelectValue) && SelectValue == "CF" ? true : false,
            });

            ViewBag.AccountType = list;
        }

        private void GerateBagBanc(string selectBanc)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var bancs = this.BancoRepository.FindAll();
            var filt = new string[] { "001", "033", "104", "237", "341" };
            if (bancs != null)
                bancs.Where(f => filt.Contains(f.BANCO)).ToList().ForEach(f =>
                  {
                      list.Add(new SelectListItem
                      {

                          Text = f.BANCO == "033" ? $"BCO EST.SP S/A-SANTANDER - {f.BANCO}" : $"{f.NOME} - {f.BANCO}",
                          Value = f.BANCO,
                          Selected = !string.IsNullOrEmpty(selectBanc) && f.BANCO == selectBanc ? true : false,
                      });
                  });

            ViewBag.Banc = list;
        }

        private async Task<bool> relateFornWithUser(Int64 id)
        {
            var model = this.FornRepository.Find(f => f.CODIGO == id);
            UserRelateFornCommand command = new UserRelateFornCommand
            {
                userId = LoginUser.userId,
                cnpfj = model.CGC.Replace(".", "").Replace("-", "").Replace("/", ""),
                type = model.FJ == 'F' ? Domain.Enum.personType.personal : Domain.Enum.personType.business,
                codforn = model.CODIGO
            };

            var result = await this.mediator.Send(command);
            if (result.success)
                return true;

            return false;
        }

        private bool TryValidateRegisterModel(ProvRegisterViewModel model)
        {
            bool result = TryValidateModel(model);
            if (model.typePerson == 'J' && string.IsNullOrEmpty(model.razao))
            {
                ModelState.AddModelError("razao", "O campo razão não foi informado.");
                result = false;
            }

            if (model.favCpf == model.cgc)
            {
                ModelState.AddModelError("favCpf", "O CPF do favorecido só deve ser utilizado quando diferente do Cadastro.");
                result = false;
            }

            if (!string.IsNullOrEmpty(model.favCpf) && string.IsNullOrEmpty(model.favName))
            {
                ModelState.AddModelError("favName", "O nome do favorecido deve ser informado quando utilizado o campo CPF do Favorecido");
                result = false;
            }


            return result;
        }

        private bool TryValidateUpdatedel(ProvUpdateViewModel model)
        {
            bool result = TryValidateModel(model);
            if (model.typePerson == 'J' && string.IsNullOrEmpty(model.razao))
            {
                ModelState.AddModelError("razao", "O campo razão não foi informado.");
                result = false;
            }

            if (model.favCpf == model.cgc)
            {
                ModelState.AddModelError("favCpf", "O CPF do favorecido só deve ser utilizado quando diferente do Cadastro.");
                result = false;
            }

            if (!string.IsNullOrEmpty(model.favCpf) && string.IsNullOrEmpty(model.favName))
            {
                ModelState.AddModelError("favName", "O nome do favorecido deve ser informado quando utilizado o campo CPF do Favorecido");
                result = false;
            }

            return result;
        }

        public ProvController(IDapper<UserDto> UserRepository,
        IDapper<NotificationDto> NotificRepository,
        IDapper<FornecedorDto> FornRepository,
        IDapper<BancoDto> BancoRepository,
        IDapper<PagamentoDto> PgtosRepository,
        IMediator mediator,
        ILogger logger)
        {
            this.UserRepository = UserRepository;
            this.NotificRepository = NotificRepository;
            this.FornRepository = FornRepository;
            this.BancoRepository = BancoRepository;
            this.PgtosRepository = PgtosRepository;
            this.mediator = mediator;
            this.logger = logger;
            var mails = ConfigurationManager.AppSettings["MailsFornChangeNotification"];
            if (!string.IsNullOrEmpty(mails))
                this.MailsFornChangeNotification = mails.Split(';');

            this.UsuManuPubli = ConfigurationManager.AppSettings["UsuManuPubli"];
        }

        public ActionResult Relate()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Relate(RelateProvViewModel model)
        {
            if (TryValidateModel(model))
            {
                string _cgc = model.cgc.Replace(".", "").Replace("-", "").Replace("/", "");
                string sql = $"SELECT * FROM [dbo].[for01] AS f WHERE [dbo].ONLYNUMBER(f.CGC) = '{_cgc}'";
                var forn = this.FornRepository.Find(sql);
                if (forn != null)
                {
                    TempData["RegisterFornededor"] = true;
                    return RedirectToAction("detail", new { cod = forn.CODIGO });
                }

                else
                {
                    return RedirectToAction("register", new { typePerson = model.typePerson, cgc = model.cgc });
                }

            }
            else
            {
                ViewData["LoginUser"] = LoginUser;
                return View();
            }

        }

        public ActionResult detail(Int64 cod)
        {
            ViewData["LoginUser"] = LoginUser;
            var model = this.FornRepository.Find(f => f.CODIGO == cod);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> detailPost(Int64 id)
        {
            try
            {
                var result = await relateFornWithUser(id);
                if (result)
                    return RedirectToAction("Index", "Home");

                throw new Exception("Falha ao relacionar usuario com fornecedor");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult register(char typePerson, string cgc)
        {
            ViewData["LoginUser"] = LoginUser;
            GerateBagState();
            GerateBagAccountType(null);
            GerateBagBanc(null);
            var model = new ProvRegisterViewModel
            {
                typePerson = typePerson,
                cgc = cgc,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> register(ProvRegisterViewModel model)
        {
            if (TryValidateRegisterModel(model))
            {
                var command = new FornRegisterCommand
                {
                    NOME = model.name,
                    RAZAO_SOC = model.razao,
                    CGC = model.cgc,
                    FJ = model.typePerson,
                    IM = model.insMun,
                    EMAIL = model.email,
                    TELEFONE = model.phone,
                    ENDERECO = model.address,
                    BAIRRO = model.neighbor,
                    MUNICIPIO = model.city,
                    ESTADO = model.state,
                    CEP = model.zipcode,
                    BANCO_PAG = model.banc,
                    CC_TIPO = model.accountType,
                    CC_PAG = model.account,
                    AGENC_PAG = model.agency,
                    USMANU = this.UsuManuPubli,
                    CNPJ_PAG = model.favCpf,
                    NOME_PAG = model.favName,
                    FJ_PAG = 'F',
                };
                try
                {
                    var result = await this.mediator.Send(command);
                    if (result.success)
                    {
                        var result2 = await relateFornWithUser(result.CodForn);
                        if (result2)
                            return RedirectToAction("Index", "Home");

                        throw new Exception("Falha ao relacionar usuario com fornecedor");
                    }

                    throw new Exception("Falha na gravação dos dados do fornecedor");

                }
                catch (Exception ex)
                {
                    if (logger != null) logger.Error(VMRCPACK.Infrastructure.Helpers.Tools.Util.CompileExceptionObjectOnOneString(ex, string.Empty));
                    ModelState.Clear();
                    ModelState.AddModelError("", "Falha no processo de gravação, por favor revise os dados e tente novamente.");
                }
            }


            ViewData["LoginUser"] = LoginUser;
            GerateBagState();
            GerateBagAccountType(model.accountType);
            GerateBagBanc(model.banc);
            return View(model);
        }

        public ActionResult update(Int64 cod)
        {
            ViewData["LoginUser"] = LoginUser;
            var forn = this.FornRepository.Find(f => f.CODIGO == cod);
            GerateBagAccountType(forn.CC_TIPO);
            GerateBagBanc(forn.XBANCO_PAG);

            GerateBagState();

            var model = new ProvUpdateViewModel
            {
                cgc = forn.CGC,
                codigo = forn.CODIGO,
                name = forn.NOME,
                razao = forn.RAZAO_SOC,
                account = forn.XCC_PAG,
                accountType = forn.CC_TIPO,
                agency = forn.XAGENC_PAG,
                banc = forn.XBANCO_PAG,
                email = forn.EMAIL,
                phone = forn.TELEFONE,
                favCpf = forn.XCNPJ_PAG,
                favName = forn.NOME_PAG,
                typePerson = forn.FJ,

                city = forn.MUNICIPIO,
                address = forn.ENDERECO,
                neighbor = forn.BAIRRO,
                state = forn.ESTADO,
                zipcode = forn.CEP,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> update(ProvUpdateViewModel model)
        {
            if (TryValidateUpdatedel(model))
            {
                var command = new FornUpdateCommand
                {
                    CODIGO = model.codigo,
                    NOME = model.name,
                    RAZAO_SOC = model.razao,
                    EMAIL = model.email,
                    TELEFONE = model.phone,
                    BANCO_PAG = model.banc,
                    CC_TIPO = model.accountType,
                    CC_PAG = model.account,
                    AGENC_PAG = model.agency,
                    USMANU = this.UsuManuPubli,
                    CNPJ_PAG = model.favCpf,
                    NOME_PAG = model.favName,
                    FJ_PAG = 'F',

                    MUNICIPIO = model.city,
                    ENDERECO = model.address,
                    BAIRRO = model.neighbor,
                    ESTADO = model.state,
                    CEP = model.zipcode,

                    NotificationTo = this.MailsFornChangeNotification == null ? null : this.MailsFornChangeNotification.ToList(),
                };
                try
                {
                    var result = await this.mediator.Send(command);
                    if (result.success)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    throw new Exception("Falha na gravação dos dados do fornecedor");

                }
                catch (Exception ex)
                {
                    if (logger != null) logger.Error(VMRCPACK.Infrastructure.Helpers.Tools.Util.CompileExceptionObjectOnOneString(ex, string.Empty));
                    ModelState.Clear();
                    ModelState.AddModelError("", "Falha no processo de gravação, por favor revise os dados e tente novamente.");
                }
            }

            ViewData["LoginUser"] = LoginUser;
            GerateBagAccountType(model.accountType);
            GerateBagBanc(model.banc);

            GerateBagState();

            return View(model);
        }

        public ActionResult Payments()
        {
            ViewData["LoginUser"] = LoginUser;
            var model = this.PgtosRepository.FindAll(f => f.COD_FORNEC == LoginUser.codforn);

            if (model != null && model.Count > 0)
            {
                var datebase = new DateTime(2018, 5, 7);
                var _modelaux = model.Where(f => f.EMISSAO >= datebase);
                
                model = _modelaux.ToList();
            }

            return View(model);
        }
    }
}
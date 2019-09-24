using MediatR;
using Newtonsoft.Json;
using PubliAX.CQRS.PedidoProducao.Register;
using PubliAX.Domain.DTO;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Logger;

namespace PubliAX.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private IDapper<UserDto> UserRepository { get; }
        private IDapper<NotificationDto> NotificRepository { get; }
        private IDapperWrite<PedidoProducaoDto> OrderRepository { get; }
        private IMediator mediator { get; }
        private ILogger logger { get; }

        private string _filePath;

        private string filePath
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                {
                    var uploadPath = Server.MapPath("~/Content/Uploads");
                    _filePath = Path.Combine(@uploadPath, Path.GetFileName("ImportPedidosProducao.txt"));
                }

                return _filePath;
            }
        }

        public OrderController(
            IDapper<NotificationDto> NotificRepository,
            IDapper<UserDto> UserRepository,
            IDapperWrite<PedidoProducaoDto> OrderRepository,
            IMediator mediator,
            ILogger logger
            )
        {
            this.NotificRepository = NotificRepository;
            this.UserRepository = UserRepository;
            this.OrderRepository = OrderRepository;
            this.mediator = mediator;
            this.logger = logger;
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

        private PedidoProducaoViewModel createPP(string line)
        {
            if (string.IsNullOrEmpty(line)) throw new Exception("Line null");

            int[] _auxdate = new int[3];
            _auxdate[0] = Convert.ToInt16(line.Substring(25, 2));
            _auxdate[1] = Convert.ToInt16(line.Substring(27, 2));
            _auxdate[2] = Convert.ToInt16(line.Substring(29, 4));

            var model = new PedidoProducaoViewModel
            {
                ORCAMENTO = Convert.ToInt64(line.Substring(0, 6)),
                LANCAMENTO = Convert.ToInt64(line.Substring(6, 3)),
                COD_FORNEC = Convert.ToInt64(line.Substring(9, 8)),
                VALOR = Convert.ToDouble($"{line.Substring(17, 6)},{line.Substring(23, 2)}"),
                VENCIMENTO = new DateTime(_auxdate[2], _auxdate[1], _auxdate[0]),
            };

            return model;
        }

        // GET: PedidoProducao
        public ActionResult Importation()
        {
            try
            {
                if (System.IO.File.Exists(this.filePath))
                    System.IO.File.Delete(this.filePath);
            }
            catch (Exception)
            {
            }

            ViewData["LoginUser"] = LoginUser;
            return View();
        }

        [HttpPost]
        public string FileUpload()
        {
            try
            {
                if (Request.Files.Count <= 0)
                    throw new Exception("Nenhum arquivo foi selecionado");

                HttpPostedFileBase _file = Request.Files[0];
                if (_file.ContentLength <= 0)
                    throw new Exception("Arquivo vazio ou carregado incorretamente");

                _file.SaveAs(this.filePath);

                string line = System.IO.File.ReadAllLines(this.filePath).First();
                var modelImport = createPP(line);

                IDictionary<string, Object> param = new ExpandoObject();
                param.Add("@ORCAMENTO", modelImport.ORCAMENTO);
                param.Add("@LANCAMENTO", modelImport.LANCAMENTO);
                param.Add("@COD_FORNEC", modelImport.COD_FORNEC);
                param.Add("@VALOR", modelImport.VALOR);
                param.Add("@VENCIMENTO", modelImport.VENCIMENTO);

                var model = this.OrderRepository.Find("", param);

                return JsonConvert.SerializeObject(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                try
                {
                    if (System.IO.File.Exists(this.filePath))
                        System.IO.File.Delete(this.filePath);
                }
                catch (Exception)
                {
                }

                return JsonConvert.SerializeObject(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public string ConfirmImport()
        {
            var json = string.Empty;
            var _userData = this.LoginUser;
            try
            {
                var msgLogger = $"Importação de Pagamentos (Em: {DateTime.Now} - Por User: {_userData.name}): ";
                string[] lines = System.IO.File.ReadAllLines(this.filePath);
                if (lines == null || lines.Count() <= 0)
                    throw new Exception("Falha na leitura do arquivo. O Arquivo não pode ser lido. Proceda novamente o carregamento do mesmo");

                var ResultList = new List<PedidoProducaoViewModel>();

                for (int i = 0; i < lines.Length; i++)
                {
                    try
                    {
                        var modelImport = createPP(lines[i]);
                        var result = this.mediator.Send(new PedidoProducaoRegisterCommand
                        {
                            COD_FORNEC = modelImport.COD_FORNEC,
                            LANCAMENTO = modelImport.LANCAMENTO,
                            ORCAMENTO = modelImport.ORCAMENTO,
                            VALOR = modelImport.VALOR,
                            VENCIMENTO = modelImport.VENCIMENTO,
                        }).Result;

                        modelImport.status = "Falha";
                        modelImport.description = result.description;
                        modelImport.linenumber = i + 1;

                        if (result.success)
                            modelImport.status = "Sucesso";

                        ResultList.Add(modelImport);
                    }
                    catch (Exception ex)
                    {
                        ResultList.Add(new PedidoProducaoViewModel { linenumber = i, status = "fail", description = ex.Message });
                        this.logger.Debug($"Erro na Importação de Pagamentos (Em: {DateTime.Now} - Por User: {_userData.name}) Linha {i}: {ex.Message}");
                    }
                }

                var objResult = new
                {
                    resume = new
                    {
                        Tot = ResultList.Count,
                        Ok = ResultList.Count(f => f.status == "Sucesso"),
                        Fail = ResultList.Count(f => f.status == "Falha")
                    },
                    detail = ResultList,
                };

                msgLogger += $"Total de Linhas: {objResult.resume.Tot}, Importados com sucesso: {objResult.resume.Ok}, Falha na importacao: {objResult.resume.Fail}";
                this.logger.Info(msgLogger);
                json = JsonConvert.SerializeObject(new { success = true, data = objResult });

            }
            catch (Exception ex)
            {
                json = JsonConvert.SerializeObject(new { success = false, message = ex.Message });
                this.logger.Error($"Falha na Importação de Pagamentos(Em: { DateTime.Now} - Por User: { _userData.name}): {ex.Message}");
            }

            try
            {
                if (System.IO.File.Exists(this.filePath))
                    System.IO.File.Delete(this.filePath);
            }
            catch (Exception)
            {
            }

            return json;
        }
    }
}
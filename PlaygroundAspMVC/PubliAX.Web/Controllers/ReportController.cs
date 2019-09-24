using PubliAX.Domain.DTO;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Models.Report;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Logger;
using VMRCPACK.Infrastructure.Interfaces.Message;

namespace PubliAX.Web.Controllers
{
    public class ReportController : Controller
    {
        private IDapper<PgtosConsecutivosDto> PgtosConsecutivosDto { get; set; }
        private IDapper<UserDto> UserRepository { get; set; }
        private IDapper<NotificationDto> NotificRepository { get; }
        private IMsgService msgService { get; set; }
        private ILogger logger { get; set; }

        public string[] MailsFornChangeNotification { get; }


        public ReportController(IDapper<UserDto> UserRepository,
            IDapper<NotificationDto> NotificRepository,
           IDapper<PgtosConsecutivosDto> PgtosConsecutivosDto,
           ILogger logger,
           IMsgService msgService)
        {
            this.NotificRepository = NotificRepository;
            this.UserRepository = UserRepository;
            this.PgtosConsecutivosDto = PgtosConsecutivosDto;
            this.msgService = msgService;
            this.logger = logger;

            var mails = ConfigurationManager.AppSettings["MailsFornChangeNotification"];
            if (!string.IsNullOrEmpty(mails))
                this.MailsFornChangeNotification = mails.Split(';');
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

        private IList<PgtosConsecutivosDto> getDataSequentPay(DateTime date)
        {
            IDictionary<string, Object> param = new ExpandoObject();
            param.Add("Data", date);

            var Pgtos = this.PgtosConsecutivosDto.FindAll(string.Empty, param);

            return Pgtos;
        }


        public ActionResult index()
        {
            return View();
        }

        [Authorize]
        public ActionResult sequentPay()
        {
            ViewData["LoginUser"] = LoginUser;
            return View();
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult sequentPayReport(DateTime date)
        {
            ViewData["LoginUser"] = LoginUser;
            ViewBag.Ref = date;
            var result = getDataCompleteSequentPay(date);
            return View(result);
        }

        [Authorize]
        public void ExportToExcel(DateTime date)
        {
            string fileName = $"rel_pgtoconsec_ref_{date.ToString("MM-yyyy")}.xls";
            var list = getDataCompleteSequentPay(date);

            DataGrid dg = new DataGrid();
            dg.AllowPaging = false;
            dg.DataSource = list;
            dg.DataBind();

            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Buffer = true;
            System.Web.HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            System.Web.HttpContext.Current.Response.Charset = "";
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition","attachment; filename=" + fileName);

            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlTextWriter = new System.Web.UI.HtmlTextWriter(stringWriter);
            dg.RenderControl(htmlTextWriter);
            System.Web.HttpContext.Current.Response.Write(stringWriter.ToString());
            System.Web.HttpContext.Current.Response.End();
        }

        
        private IList<PgtoConsecutivoViewModel> getDataCompleteSequentPay(DateTime date)
        {
            var Pgtos = this.getDataSequentPay(date);
            IList<PgtoConsecutivoViewModel> result = null;
            if (Pgtos != null && Pgtos.Count > 0)
            {
                result = new List<PgtoConsecutivoViewModel>();
                Parallel.For(0, Pgtos.Count, i =>
                {
                    var user = this.UserRepository.Find(f => f.codforn == Pgtos[i].COD_FORNEC);
                    if (user != null)
                    {
                        result.Add(new PgtoConsecutivoViewModel
                        {
                            CNPJ = user.cnpfj,
                            FORNECEDOR = user.name,
                            PG_CONSEC = Pgtos[i].D90 ? "30/60/90" : "30/60",
                        });
                    }
                });
            }
            return result; 
        }

        public ActionResult SendAllertPgtosConsecutivos()
        {
            try
            {
                string html = @"<html><head></head><body>
	                           <h2>Relatório de Pagamentos Consecutivos</h2>
                               <hr/>
                               <ul>
                                    {0}
                               </ul>
                            </body ></html >";

                var Pgtos = this.getDataSequentPay(DateTime.Now);

                StringBuilder sb = new StringBuilder();

                foreach (var pg in Pgtos)
                {
                    var user = this.UserRepository.Find(f => f.codforn == pg.COD_FORNEC);
                    if (user != null)
                    {
                        string type = user.type == Domain.Enum.personType.business ? "CNPJ" : "CPF";
                        string msg = $"<li>O promotor {user.name.ToUpper()} ({type}: {user.cnpfj}) já teve pagamentos em 30";
                        if (pg.D90)
                            msg += ",60 e 90 ";
                        else
                            msg += " e 60 ";

                        msg += $"dias consecutivos com base o mês corrente ({DateTime.Now.ToString("MM")} de {DateTime.Now.ToString("yyyy")})</li>";
                        sb.AppendLine(msg);
                    }
                }

                if (Pgtos != null && Pgtos.Count > 0 &&  !string.IsNullOrEmpty(sb.ToString()))
                {
                    this.msgService.sendMail("Relatório de Pagamentos Consecutivos",
                                              string.Format(html, sb.ToString()),
                                              this.MailsFornChangeNotification 
                                            );

                }
            }
            catch (Exception ex)
            {
                if (this.logger != null)
                    this.logger.Error(VMRCPACK.Infrastructure.Helpers.Tools.Util.CompileExceptionObjectOnOneString(ex, string.Empty));
            }


            return RedirectToAction("index");
        }
    }
}

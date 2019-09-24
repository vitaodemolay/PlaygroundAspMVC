using MediatR;
using PubliAX.CQRS.Notification.Register;
using PubliAX.CQRS.Notification.Update;
using PubliAX.Domain.DTO;
using PubliAX.Web.Configs.authentication;
using PubliAX.Web.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using VMRCPACK.Infrastructure.Interfaces.Dapper;
using VMRCPACK.Infrastructure.Interfaces.Logger;

namespace PubliAX.Web.Controllers
{
    [Authorize]
    public class NoticationController : Controller
    {
        private IDapper<UserDto> UserRepository { get; set; }
        private IDapper<NotificationDto> NotificRepository { get; set; }
        private IMediator mediator { get; set; }
        private ILogger logger { get; set; }


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

        private void GerateBagUsers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            Expression<Func<UserDto, bool>> filter = null;
            if (this.LoginUser.role == "Admin")
                filter = f => f.status != Domain.Enum.entityStatus.excluded && f.userId != this.LoginUser.userId;
            else
                filter = f => f.status != Domain.Enum.entityStatus.excluded && f.userId != this.LoginUser.userId && f.role == "Admin";

            var users = this.UserRepository.FindAll(filter);
            if (users != null)
                users.ToList().ForEach(f =>
                {
                    list.Add(new SelectListItem
                    {
                        Text = f.name,
                        Value = f.login,
                    });
                });

            ViewBag.Users = list;
        }

        public NoticationController(IDapper<UserDto> UserRepository,
           IDapper<NotificationDto> NotificRepository,
           IMediator mediator,
           ILogger logger)
        {
            this.UserRepository = UserRepository;
            this.NotificRepository = NotificRepository;
            this.mediator = mediator;
            this.logger = logger;

        }




        // GET: Notication
        public ActionResult Index()
        {
            var model = this.NotificRepository.FindAll(f => f.userId == LoginUser.userId && f.status != Domain.Enum.entityStatus.excluded);

            ViewData["LoginUser"] = LoginUser;
            return View(model);
        }



        public ActionResult Create()
        {
            GerateBagUsers();
            ViewData["LoginUser"] = LoginUser;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SendNotificationViewModel model)
        {
            if (TryValidateModel(model))
            {
                var user = this.UserRepository.Find(f => f.login == model.recipient);

                var result = await this.mediator.Send(new NotificationRegisterCommand
                {
                    userIdDest = user.userId,
                    body = model.body,
                    subject = model.subject,
                    sender = this.LoginUser.login,
                });

                if (result.success)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "Falha no envio da notificação: " + result.description);
            }

            ViewData["LoginUser"] = LoginUser;
            return View(model);
        }



        public void setReaded(Guid id)
        {
            var result = this.mediator.Send(new NotificationUpdateCommand
            {
                notificationId = id,
                status = Domain.Enum.entityStatus.disable,
                userId = LoginUser.userId
            }).Result;
        }



        public ActionResult setReadedAction(Guid id)
        {
            setReaded(id);
            return RedirectToAction("Index");
        }


        public async Task<ActionResult> setNoReaded(Guid id)
        {
            var result = await this.mediator.Send(new NotificationUpdateCommand
            {
                notificationId = id,
                status = Domain.Enum.entityStatus.enable,
                userId = LoginUser.userId
            });

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> delete(Guid id)
        {
            var result = await this.mediator.Send(new NotificationUpdateCommand
            {
                notificationId = id,
                status = Domain.Enum.entityStatus.excluded,
                userId = LoginUser.userId
            });

            return RedirectToAction("Index");
        }
    }
}
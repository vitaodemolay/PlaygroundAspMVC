using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PlaygroundAspMVC.MvcAuthTeste.Config.Authentication;
using PlaygroundAspMVC.MvcAuthTeste.Config.Authorization;
using PlaygroundAspMVC.MvcAuthTeste.Config.Identity;
using PlaygroundAspMVC.MvcAuthTeste.Config.Parameters;
using PlaygroundAspMVC.MvcAuthTeste.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace PlaygroundAspMVC.MvcAuthTeste.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly HttpClient _httpClient;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private async Task RegisterLogin(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);

            SerializeCustomPrincipal serializemodel = new SerializeCustomPrincipal
            {
                name = user.UserName,
                role = user.role,
                userId = Guid.Parse(user.Id),
                email = user.email,
            };

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            string userData = serializer.Serialize(serializemodel);

            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                                                1,
                                                                user.UserName,
                                                                DateTime.Now,
                                                                DateTime.Now.AddMinutes(20),
                                                                false,
                                                                userData);
            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;

            _httpClient = new HttpClient();
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager;
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager;
            }
            private set
            {
                _userManager = value;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<ActionResult> AuthGetToken(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                var email = (User as CustomPrincipal).email;
                var password = SecurityParameters.SystemPasswordForRequestTokenApiAuthorization.ToString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}/api/token"),
                    Headers =
                    {
                        { HttpRequestHeader.ContentType.ToString(), "application/x-www-form-urlencoded" },
                    },
                };

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("username", email));
                keyValues.Add(new KeyValuePair<string, string>("password", password));
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));

                request.Content = new FormUrlEncodedContent(keyValues);

                var response = await _httpClient.SendAsync(request);
                var authResult = await response.Content.ReadAsAsync<AuthorizationResult>();

                if(authResult != null)
                {
                    return RedirectToLocal($"{returnUrl}{authResult.access_token}");
                }

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login", "Account", new { returnUrl = $"{Request.Url.OriginalString}" });
        }


        public ActionResult ReceiveToken(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:

                    await RegisterLogin(model.Email);

                    return RedirectToLocal(returnUrl);

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }


        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Name, email = model.Email, role = "User" };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await RegisterLogin(user.email);

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            return View(model);
        }



        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}




// $"{Request.Url.Scheme}://{Request.Url.Authority}/api/token"

/*
 * var client = new RestClient("http://localhost:60047/api/token");
 * var request = new RestRequest(Method.POST);
 * request.AddHeader("cache-control", "no-cache");
 * request.AddHeader("Connection", "keep-alive");
 * request.AddHeader("Cookie", "__RequestVerificationToken=cHxCAE0YDP4U7ItyafJP5gRS9jmhwdmzNr8aOpFNwXvq62M6nCheb-QHhftusjwpExQwrXvW9Q87_ZxcwOhS7cB4womjMXTdlZ9As8VTC1Y1");
 * request.AddHeader("Content-Length", "69");
 * request.AddHeader("Accept-Encoding", "gzip, deflate");
 * request.AddHeader("Host", "localhost:60047");
 * request.AddHeader("Postman-Token", "ba55f1d6-7084-4c5f-9b6a-89fa7b6e39c3,09eeeaf8-4a7e-4420-bdd6-ea3a44d07340");
 * request.AddHeader("Cache-Control", "no-cache");
 * request.AddHeader("User-Agent", "PostmanRuntime/7.17.1");
 * request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
 * request.AddParameter("undefined", "username=vitor.marcos%40gmail.com&password=123456&grant_type=password", ParameterType.RequestBody);
 * IRestResponse response = client.Execute(request);
*/

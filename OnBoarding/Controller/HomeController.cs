using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnBoarding.Models;
using OnBoarding.Services;
using OnBoarding.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;

namespace OnBoarding.Web.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public HomeController()
        {

        }

        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
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
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // GET: /Home/TermsAndConditions
        [AllowAnonymous]
        public ActionResult TermsAndConditions()
        {
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserAccountModel model, string returnUrl)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.loginModel.Email, model.loginModel.Password, model.loginModel.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindAsync(model.loginModel.Email, model.loginModel.Password);
                    var roles = await UserManager.GetRolesAsync(user.Id);

                    using (DBModel db = new DBModel())
                    {
                        var userRole = db.AspNetUserRoles.SingleOrDefault(e => e.UserId == user.Id);
                        var menuId = (from a in db.SystemMenuAccess
                                      join b in db.SystemMenus on a.menuId equals b.Id
                                      where a.roleId == userRole.RoleId && b.isHomePage == true && a.Status == 1
                                      orderby a.displayId ascending
                                      select a.menuId).SingleOrDefault();

                        var homePage = db.SystemMenus.SingleOrDefault(c => c.Id == menuId);
                        //Check if users password has expired and redirect to ChangePassword
                        if (user.LastPasswordChangedDate.AddDays(Properties.Settings.Default.PasswordExpiryDays) < DateTime.UtcNow)
                        {
                            return RedirectToAction("ChangePassword", "Account");
                        }
                        else
                        {
                            return RedirectToAction(homePage.action, homePage.controller);
                        }
                    }

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.loginModel.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt: Your username or password is incorrect");
                    return View("Index", model);
            }
        }
       
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        public ActionResult Register(UserAccountModel model)
        {
            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //Check if Email or Account number provided Exists
                    var CheckEmailExists = db.RegisteredClients.Any(i => i.EmailAddress == model.registerModel.EmailAddress.ToLower());
                    var CheckAccountExists = db.RegisteredClients.Any(i => i.AccountNumber == model.registerModel.StanbicAccountNumber);
                    var CheckUserAccountExists = db.AspNetUsers.Any(i => i.Email == model.registerModel.EmailAddress);
                    var _action = "Register";
                    //1. Email and Account details exist //Already completed registration process
                    if (CheckEmailExists == true || CheckUserAccountExists == true || CheckAccountExists == true)
                    {
                        ModelState.AddModelError(string.Empty, "Email address or Account number is already registered.");
                    }

                    //2. Email Exists and User account does not exist // For uploaded clients
                   
                    //3. New Users
                    else
                    {
                        //If not Create Client with AccountNumber, Email and Company Name
                        int lastInsertId = db.RegisteredClients.Max(p => p.Id);
                        var activationCode = string.Concat((lastInsertId + 1) + OTPGeneratorReG.GetUniqueKey(6));
                        string mixedOriginal = Shuffle.StringMixer(activationCode.Substring(2, 6));

                        //Update RegisteredClient Details
                        try
                        {
                            var newClient = db.RegisteredClients.Create();
                            newClient.AccountNumber = model.registerModel.StanbicAccountNumber;
                            newClient.EmailAddress = model.registerModel.EmailAddress.ToLower();
                            newClient.OTP = Functions.GenerateMD5Hash(mixedOriginal);
                            newClient.OTPDateCreated = DateTime.Now;
                            newClient.AcceptedTerms = model.registerModel.terms;
                            newClient.Surname = model.registerModel.Surname;
                            newClient.OtherNames = model.registerModel.Othernames;
                            newClient.AcceptedUserTerms = false;
                            db.RegisteredClients.Add(newClient);
                            var savedClient = db.SaveChanges();

                            if (savedClient > 0)
                            {
                                //Create New DefaultCompany
                                int lastInsertedClientId = db.RegisteredClients.Max(p => p.Id);
                                var newClientCompany = db.ClientCompanies.Create();
                                newClientCompany.CompanyName = model.registerModel.CompanyBusinessName;
                                newClientCompany.ClientId = lastInsertedClientId;
                                newClientCompany.Status = 1;
                                newClientCompany.CreatedBy = model.registerModel.EmailAddress;
                                db.ClientCompanies.Add(newClientCompany);
                                var savedClientCompany = db.SaveChanges();
                                if (savedClientCompany > 0)
                                {
                                    //Send Success Email New
                                    var callbackUrl = Url.Action("ConfirmRegistration", "Account", null, Request.Url.Scheme);
                                    string EmailBody = string.Empty;
                                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientRegistrationEmail.html")))
                                    {
                                        EmailBody = reader.ReadToEnd();
                                    }
                                    EmailBody = EmailBody.Replace("{CompanyName}", model.registerModel.Othernames);
                                    EmailBody = EmailBody.Replace("{ActivationCode}", mixedOriginal);
                                    EmailBody = EmailBody.Replace("{Url}", callbackUrl);

                                    var CompleteRegistrationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.registerModel.EmailAddress.ToLower(), "Confirm Registration", EmailBody);
                                    if (CompleteRegistrationEmail == true)
                                    {
                                        //Log email sent notification
                                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.registerModel.EmailAddress.ToLower(), _action);
                                    }
                                    else
                                    {
                                        //Log Email failed notification
                                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.registerModel.EmailAddress.ToLower(), _action);
                                    }
                                    //Redirect to Complete Registration/Confirm OTP Page
                                    return RedirectToAction("ConfirmRegistration", "Account");
                                }
                                else
                                {
                                    //Remove Client Details
                                    db.RegisteredClients.RemoveRange(db.RegisteredClients.Where(r => r.EmailAddress == model.registerModel.EmailAddress));
                                    db.SaveChanges();

                                    // Send Error to model
                                    ModelState.AddModelError(string.Empty, "System Error! Unable to create your default company, please try again later.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "System Error! Unable to create your account, please try again later.");
                            }
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError(string.Empty, "System Error! Unable to create your account, please try again later.");
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Index", model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
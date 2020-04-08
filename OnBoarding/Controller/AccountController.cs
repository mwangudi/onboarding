using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OnBoarding.ViewModels;
using OnBoarding.Services;
using OnBoarding.Models;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;

namespace OnBoarding.Controllers
{
    public class AccountController : Controller
    {
        public ApplicationSignInManager _signInManager;
        public ApplicationUserManager _userManager;

        public AccountController() {}

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
            // return RedirectToAction("Register", "Account");
        }

        //GET /Account/Home
        [AllowAnonymous]
        public ActionResult Login()
        {
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/CompleteRegistration
        [AllowAnonymous]
        public ActionResult CompleteRegistration()
        {
            return View();
        }

        // POST: /Account/CompleteRegistration
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public ActionResult CompleteRegistration(CompleteRegistration model)
        public async Task<ActionResult> CompleteRegistration(CompleteRegistrationViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //Check for OTP
                    var EncryptedOTP = Functions.GenerateMD5Hash(model.OTP);
                    var emailToConfirm = model.RegistrationEmail;
                    var _action = "CompleteRegistration";

                    //Check if user exists
                    var _userExists = db.AspNetUsers.Any(u => u.Email == model.RegistrationEmail);
                    if(_userExists)
                    {
                        ModelState.AddModelError(string.Empty, "Your have already completed registration. Please login.");
                    }
                    else {
                        
                        //Added Email verification for OTP Recycling issue
                        var IsThereOTP = db.RegisteredClients.Any(i => i.OTP == EncryptedOTP && i.EmailAddress == model.RegistrationEmail);
                        if (IsThereOTP == false)
                        {
                            ModelState.AddModelError(string.Empty, "Incorrect OTP/Email Address");
                        }

                        else
                        {
                            //Create User with Password after confirmation with OTP
                            //Check if already completed registration process
                            var DBOTPToconfirm = db.RegisteredClients.First(f => f.OTP == EncryptedOTP && f.EmailAddress == model.RegistrationEmail);
                            TimeSpan diff = DateTime.Now - DBOTPToconfirm.DateCreated;
                            if (DBOTPToconfirm.Status == 1)
                            {
                                ModelState.AddModelError(string.Empty, "Incorrect OTP/Email Address");
                            }

                            //Check expiry of OTP currently Web.config Setting name = ClientOTPExpiry
                            else if (diff.Hours > Properties.Settings.Default.ClientOTPExpiry)
                            {
                                ModelState.AddModelError(string.Empty, "Your OTP has expired. Kindly request for another");
                            }
                            else
                            {
                                var user = new ApplicationUser { UserName = DBOTPToconfirm.EmailAddress, Email = DBOTPToconfirm.EmailAddress, CompanyName = DBOTPToconfirm.Surname +" "+ DBOTPToconfirm.OtherNames, LastPasswordChangedDate = DateTime.Now };
                                var result = await UserManager.CreateAsync(user, model.Password);
                                if (result.Succeeded)
                                {
                                    //Add EMT user role
                                    UserManager.AddToRole(user.Id, "EMTUser");

                                    //Update Registered Account details
                                    var ClientToUpdate = db.RegisteredClients.SingleOrDefault(b => b.EmailAddress == DBOTPToconfirm.EmailAddress);
                                    if (ClientToUpdate != null)
                                    {
                                        try
                                        {
                                            ClientToUpdate.CreatedBy = user.Id;
                                            ClientToUpdate.Status = 1; //0. For Registration not confirmed 1. For Registration Confirmed
                                            ClientToUpdate.RegistrationConfirmationDate = DateTime.Now;
                                            ClientToUpdate.UserAccountID = user.Id;
                                            db.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            throw (ex);
                                        }
                                    }

                                    //Send Success Email New
                                    string EmailBody = string.Empty;
                                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ConfirmRegistration.html")))
                                    {
                                        EmailBody = reader.ReadToEnd();
                                    }
                                    EmailBody = EmailBody.Replace("{CompanyName}", DBOTPToconfirm.OtherNames);
                                    var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, DBOTPToconfirm.EmailAddress, "Registration Complete", EmailBody);

                                    if (SendRegistrationCompleteEmail == true)
                                    {
                                        //Log email sent notification
                                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, DBOTPToconfirm.EmailAddress, _action);
                                    }
                                    else
                                    {
                                        //Log Email failed notification
                                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, DBOTPToconfirm.EmailAddress, _action);
                                    }

                                    //Send Email to Digital Desk Users
                                    var DDUserRole = (from p in db.AspNetUserRoles
                                                      join e in db.AspNetUsers on p.UserId equals e.Id
                                                      where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc" && e.Status == 1
                                                      select new
                                                      {
                                                          EmailID = e.Email
                                                      }).ToList();
                                    foreach (var email in DDUserRole)
                                    {
                                        var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client has successfully completed registration. <br/>" +
                                                      "Full Names: " + DBOTPToconfirm.Surname +" "+ DBOTPToconfirm.OtherNames + ", <br/> Email Address: " + DBOTPToconfirm.EmailAddress + " " +
                                                      "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                                        var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Client Registration Complete", DDMessageBody);
                                        if (SendDDNotificationEmail == true)
                                        {
                                            //Log email sent notification
                                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, DDMessageBody, email.EmailID, _action);
                                        }
                                        else
                                        {
                                            //Log Email failed notification
                                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, DDMessageBody, email.EmailID, _action);
                                        }
                                    }

                                    //SignIn User to landing page
                                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                    //return RedirectToLocal(returnUrl);
                                    //return RedirectToAction("Index", "Client");
                                    var userRole = db.AspNetUserRoles.SingleOrDefault(e => e.UserId == user.Id);
                                    var menuId = (from a in db.SystemMenuAccess
                                                    join b in db.SystemMenus on a.menuId equals b.Id
                                                    where a.roleId == userRole.RoleId && b.isHomePage == true
                                                    select a.menuId).SingleOrDefault();

                                    var homePage = db.SystemMenus.SingleOrDefault(c => c.Id == menuId);
                                    return RedirectToAction(homePage.action, homePage.controller);
                                }
                            }
                        }
                    }
                }
            }
            return View();
        }

        //
        // GET: /Account/CompleteRegistration
        [AllowAnonymous]
        public ActionResult UploadedClientCompleteRegistration()
        {
            return View();
        }

        // POST: /Account/CompleteRegistration
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public ActionResult CompleteRegistration(CompleteRegistration model)
        public async Task<ActionResult> UploadedClientCompleteRegistration(UploadedClientCompleteRegistrationViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //Check OTP
                    var EncryptedOTP = Functions.GenerateMD5Hash(model.OTP);
                    var _action = "UploadedClientCompleteRegistration";
                    //Added Email verification for OTP Recycling issue
                    var IsThereOTP = db.RegisteredClients.Any(i => i.OTP == EncryptedOTP && i.EmailAddress == model.UploadedEmail);
                    if (IsThereOTP == false)
                    {
                        ModelState.AddModelError(string.Empty, "Incorrect OTP/Email Address");
                    }
                    else
                    {
                        //Create User with Password after confirmation with OTP
                        //Check if already completed registration process
                        var DBOTPToconfirm = db.RegisteredClients.First(f => f.OTP == EncryptedOTP && f.EmailAddress == model.UploadedEmail);
                        TimeSpan diff = DateTime.Now - DBOTPToconfirm.DateCreated;
                        if (DBOTPToconfirm.Status == 1)
                        {
                            ModelState.AddModelError(string.Empty, "Incorrect OTP/Email Address");
                        }
                        //Check expiry of OTP currently as set in web.config setting name="ClientOTPExpiry"
                        else if (diff.Hours > Properties.Settings.Default.ClientOTPExpiry)
                        {
                            ModelState.AddModelError(string.Empty, "Your OTP has expired. Kindly Request for another");
                        }
                        else
                        {
                            var user = new ApplicationUser { UserName = DBOTPToconfirm.EmailAddress, Email = DBOTPToconfirm.EmailAddress, CompanyName = DBOTPToconfirm.Surname + "" + DBOTPToconfirm.OtherNames };
                            var result = await UserManager.CreateAsync(user, model.Password);
                            if (result.Succeeded)
                            {
                                //Add EMT user role
                                UserManager.AddToRole(user.Id, "EMTUser");

                                //Update Registered Account details
                                var ClientToUpdate = db.RegisteredClients.SingleOrDefault(b => b.EmailAddress == DBOTPToconfirm.EmailAddress);
                                if (ClientToUpdate != null)
                                {
                                    try
                                    {
                                        ClientToUpdate.CreatedBy = user.Id;
                                        ClientToUpdate.Status = 1; //0. For Registration not confirmed 1. For Registration Confirmed
                                        ClientToUpdate.AccountNumber = model.StanbicAccountNumber;
                                        ClientToUpdate.RegistrationConfirmationDate = DateTime.Now;
                                        ClientToUpdate.UserAccountID = user.Id;
                                        ClientToUpdate.AcceptedTerms = model.terms;
                                        db.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw (ex);
                                    }
                                }

                                //Send Success Email
                                string EmailBody = string.Empty;
                                using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ConfirmRegistration.html")))
                                {
                                    EmailBody = reader.ReadToEnd();
                                }
                                EmailBody = EmailBody.Replace("{CompanyName}", DBOTPToconfirm.OtherNames);
                                var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, DBOTPToconfirm.EmailAddress, "Registration Complete", EmailBody);

                                if (SendRegistrationCompleteEmail == true)
                                {
                                    //Log email sent notification
                                    LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, DBOTPToconfirm.EmailAddress, _action);
                                }
                                else
                                {
                                    //Log Email failed notification
                                    LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, DBOTPToconfirm.EmailAddress, _action);
                                }

                                //Send Email to Digital Desk Users
                                var DDUserRole = (from p in db.AspNetUserRoles
                                                  join e in db.AspNetUsers on p.UserId equals e.Id
                                                  where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc" && e.Status == 1
                                                  select new
                                                  {
                                                      EmailID = e.Email
                                                  }).ToList();
                                foreach (var email in DDUserRole)
                                {
                                    var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client has successfully completed registration. <br/>" +
                                                  "FullNames: " + DBOTPToconfirm.Surname + " " + DBOTPToconfirm.OtherNames + ", <br/> EmailAddress: " + DBOTPToconfirm.EmailAddress + " " +
                                                  "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                                    var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Client Registration Complete", DDMessageBody);
                                    if (SendDDNotificationEmail == true)
                                    {
                                        //Log email sent notification
                                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, DDMessageBody, email.EmailID, _action);
                                    }
                                    else
                                    {
                                        //Log Email failed notification
                                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, DDMessageBody, email.EmailID, _action);
                                    }
                                }

                                //SignIn User to landing page
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                var userRole = db.AspNetUserRoles.SingleOrDefault(e => e.UserId == user.Id);
                                var menuId = (from a in db.SystemMenuAccess
                                              join b in db.SystemMenus on a.menuId equals b.Id
                                              where a.roleId == userRole.RoleId && b.isHomePage == true
                                              select a.menuId).SingleOrDefault();

                                var homePage = db.SystemMenus.SingleOrDefault(c => c.Id == menuId);
                                return RedirectToAction(homePage.action, homePage.controller);
                            }
                        }
                    }
                }
            }
            return View();
        }

        //
        //POST: OTPHasExpired
        [HttpPost]
        public JsonResult OTPHasExpired(string OTPCode, string Email)
        {
            using (DBModel db = new DBModel())
            {
                var EncryptedOTP = Functions.GenerateMD5Hash(OTPCode);
                var OTPExists = db.RegisteredClients.Any(f => f.OTP == EncryptedOTP && f.EmailAddress == Email && f.Status == 0);
                if(OTPExists)
                {
                    var DBOTPToconfirm = db.RegisteredClients.SingleOrDefault(f => f.OTP == EncryptedOTP && f.EmailAddress == Email && f.Status == 0);
                    var hours = (DateTime.Now - DBOTPToconfirm.DateCreated).TotalHours;
                    var OTPHasExpired = db.RegisteredClients.Any(f => f.OTP == EncryptedOTP && f.EmailAddress == Email && f.Status == 0 && hours > Properties.Settings.Default.ClientOTPExpiry);
                    if (OTPHasExpired)
                    {
                        return Json(DBOTPToconfirm.Id, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("fail", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("fail", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST LoadResendOTP
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResendOTP(int getClientId)
        {
            using (DBModel db = new DBModel())
            {
                var getUserInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == getClientId);

                //Data For View Display
                ViewData["UserId"] = getUserInfo.Id;
                ViewData["EmailAddress"] = getUserInfo.EmailAddress;
                ViewData["CompanyName"] = getUserInfo.OtherNames;
                //ViewData["CompanyName"] = getUserInfo.CompanyName;
            }

            return PartialView();
        }

        //
        // POST SubmitResendOTP
        [HttpPost]
        public JsonResult SubmitResendOTP(ResendOTPViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    //Generate new OTP
                    var _activationCode = string.Concat(OTPGenerator.GetUniqueKey(6));
                    string activationCode = Shuffle.StringMixer(_activationCode);
                    var _action = "SubmitResendOTP";

                    //Update Client account details
                    var ClientToUpdate = db.RegisteredClients.SingleOrDefault(b => b.Id == model.UserId);
                    ClientToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                    ClientToUpdate.DateCreated = DateTime.Now;
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        //Send Success Email New
                        string EmailBody = string.Empty;
                        var callbackUrl = Url.Action("UploadedClientCompleteRegistration", "Account", null, Request.Url.Scheme);
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RequestResendOTP.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);
                        EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                        EmailBody = EmailBody.Replace("{NewOTP}", activationCode);

                        var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.EmailAddress, "Confirm Registration", EmailBody);

                        if (SendRegistrationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.EmailAddress, _action);
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.EmailAddress, _action);
                            return Json("Error! Unable to resend your email.", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to update your account OTP details", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json("" + ex.Message+ "", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        // GET: /Account/SignatoryConfirmation
        [AllowAnonymous]
        public ActionResult SignatoryConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ConfirmSignatory
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignatoryConfirmation(SignatoryConfirmationViewModel model, string returnUrl = null)
        {
            using (DBModel db = new DBModel())
            {
                var EncryptedOTP = Functions.GenerateMD5Hash(model.OTP);
                var IsThereOTP = db.ClientSignatories.Any(i => i.OTP == EncryptedOTP && i.EmailAddress == model.SignatoryEmail);
                var _userExists = db.AspNetUsers.Any(u => u.Email == model.SignatoryEmail);

                if (IsThereOTP == false)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect OTP/Email Address");
                }
                //Check if user exists
                else if (_userExists)
                {
                    ModelState.AddModelError(string.Empty, "Your have already completed registration. Please login.");
                }
                else
                {
                    //Create User with Password after confirmation with OTP
                    var DBOTPToconfirm = db.ClientSignatories.First(f => f.OTP == EncryptedOTP && f.EmailAddress == model.SignatoryEmail);
                    TimeSpan diff = DateTime.Now - DBOTPToconfirm.DateCreated;
                    if (DBOTPToconfirm.Status == 1)
                    {
                        ModelState.AddModelError(string.Empty, "You have completed your confirmation. Please login");
                    }

                    //Check expiry of OTP currently as set in web.config setting name="SignatoryOTPExpiry"
                    else if (diff.Hours > Properties.Settings.Default.SignatoryOTPExpiry)
                    {
                        ModelState.AddModelError(string.Empty, "Your OTP has expired. Reset it by providing it in the OTP field below");
                    }
                    else
                    {
                        var SignatoryToUpdate = db.ClientSignatories.SingleOrDefault(b => b.EmailAddress == DBOTPToconfirm.EmailAddress && b.OTP == EncryptedOTP);
                        var SignatoryClient = db.RegisteredClients.SingleOrDefault(b => b.Id == SignatoryToUpdate.ClientID);
                        var user = new ApplicationUser { UserName = DBOTPToconfirm.EmailAddress, Email = DBOTPToconfirm.EmailAddress, CompanyName = DBOTPToconfirm.OtherNames, LastPasswordChangedDate = DateTime.Now };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            //Update Signatory Confirmation Date 
                            if (SignatoryToUpdate != null)
                            {
                                try
                                {
                                    SignatoryToUpdate.ConfirmationDate = DateTime.Now;
                                    SignatoryToUpdate.Status = 1;
                                    SignatoryToUpdate.UserAccountID = user.Id;
                                    SignatoryToUpdate.AcceptedTerms = model.terms;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw (ex);
                                }
                            }
                            //Add Signatory user role
                            UserManager.AddToRole(user.Id, "Signatory");
                            //SignIn Signatory to landing page
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            //return RedirectToLocal(returnUrl);
                            //return RedirectToAction("Index", "Signatory");
                            var userRole = db.AspNetUserRoles.SingleOrDefault(e => e.UserId == user.Id);
                            var menuId = (from a in db.SystemMenuAccess
                                          join b in db.SystemMenus on a.menuId equals b.Id
                                          where a.roleId == userRole.RoleId && b.isHomePage == true
                                          select a.menuId).SingleOrDefault();

                            var homePage = db.SystemMenus.SingleOrDefault(c => c.Id == menuId);
                            return RedirectToAction(homePage.action, homePage.controller);
                        }
                    }
                }

                return View();
            }
        }

        //
        //POST: Signatory OTPHasExpired
        [HttpPost]
        public JsonResult SignatoryOTPHasExpired(string OTPCode, string Email)
        {
            using (DBModel db = new DBModel())
            {
                var EncryptedOTP = Functions.GenerateMD5Hash(OTPCode);
                var OTPExists = db.ClientSignatories.Any(f => f.OTP == EncryptedOTP && f.EmailAddress == Email && f.Status == 0);
                if (OTPExists)
                {
                    var DBOTPToconfirm = db.ClientSignatories.SingleOrDefault(f => f.OTP == EncryptedOTP && f.EmailAddress == Email && f.Status == 0);
                    TimeSpan diff = DateTime.Now - DBOTPToconfirm.DateCreated;
                    var hours = (DateTime.Now - DBOTPToconfirm.DateCreated).TotalHours;
                    var OTPHasExpired = db.ClientSignatories.Any(f => f.OTP == EncryptedOTP && f.EmailAddress == Email && f.Status == 0 && hours > Properties.Settings.Default.SignatoryOTPExpiry);
                    if (OTPHasExpired)
                    {
                        return Json(DBOTPToconfirm.Id, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("fail", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("fail", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST LoadResendSignatoryOTP
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResendSignatoryOTP(int getSignatoryId)
        {
            using (DBModel db = new DBModel())
            {
                var getUserInfo = db.ClientSignatories.SingleOrDefault(c => c.Id == getSignatoryId);

                //Data For View Display
                ViewData["UserId"] = getUserInfo.Id;
                ViewData["EmailAddress"] = getUserInfo.EmailAddress;
                ViewData["CompanyName"] = getUserInfo.OtherNames;
            }

            return PartialView();
        }

        //POST //SubmitResendSignatoryOTP
        [HttpPost]
        public JsonResult SubmitResendSignatoryOTP(ResendOTPViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                //Generate new OTP
                var _activationCode = string.Concat(OTPGenerator.GetUniqueKey(6));
                string activationCode = Shuffle.StringMixer(_activationCode);
                var _action = "SubmitResendSignatoryOTP";

                //Update Client OTP account details
                var ClientToUpdate = db.ClientSignatories.SingleOrDefault(b => b.Id == model.UserId);
                ClientToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                ClientToUpdate.DateCreated = DateTime.Now;
                var recordSaved = db.SaveChanges();
                if(recordSaved > 0)
                {
                    //Send Email with New OTP
                    string EmailBody = string.Empty;
                    var callbackUrl = Url.Action("SignatoryConfirmation", "Account", null, Request.Url.Scheme);
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RequestResendOTP.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);
                    EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                    EmailBody = EmailBody.Replace("{NewOTP}", activationCode);

                    var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.EmailAddress, "Confirm Registration", EmailBody);

                    if (SendRegistrationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.EmailAddress, _action);
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.EmailAddress, _action);
                        return Json("Error! Unable to resend your email.", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Error! Unable to update your OTP account details", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        // GET: /Account/SignatoryConfirmation
        [AllowAnonymous]
        public ActionResult DesignatedUserConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ConfirmSignatory
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DesignatedUserConfirmation(DesignatedUserConfirmationViewModel model, string returnUrl = null)
        {
            using (DBModel db = new DBModel())
            {
                var EncryptedOTP = Functions.GenerateMD5Hash(model.OTP);
                var IsThereOTP = db.DesignatedUsers.Any(i => i.OTP == EncryptedOTP && i.Email == model.SignatoryEmail);
                var _userExists = db.AspNetUsers.Any(u => u.Email == model.SignatoryEmail);

                if (IsThereOTP == false)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect OTP/Email Address");
                }
                //Check if user exists
                else if (_userExists)
                {
                    ModelState.AddModelError(string.Empty, "Your have already completed registration. Please login.");
                }
                else
                {
                    //Create User with Password after confirmation with OTP
                    var DBOTPToconfirm = db.DesignatedUsers.FirstOrDefault(f => f.OTP == EncryptedOTP && f.Email == model.SignatoryEmail);
                    TimeSpan diff = DateTime.Now - DBOTPToconfirm.DateCreated;
                    if (DBOTPToconfirm.Status == 1)
                    {
                        ModelState.AddModelError(string.Empty, "You have completed your confirmation. Please login");
                    }
                    
                    //Check expiry of OTP currently as set in web.config setting name RepresentativeOTPExpiry
                    else if (diff.Hours > Properties.Settings.Default.RepresentativeOTPExpiry)
                    {
                        ModelState.AddModelError(string.Empty, "Your OTP has expired. Reset it by providing it in the OTP field below");
                    }

                    else
                    {
                        var user = new ApplicationUser { UserName = DBOTPToconfirm.Email, Email = DBOTPToconfirm.Email, CompanyName = DBOTPToconfirm.Othernames, LastPasswordChangedDate = DateTime.Now };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            //Update Signatory Confirmation Date 
                            var DesignatedUserToUpdate = db.DesignatedUsers.SingleOrDefault(b => b.Email == DBOTPToconfirm.Email && b.OTP == EncryptedOTP);
                            if (DesignatedUserToUpdate != null)
                            {
                                try
                                {
                                    DesignatedUserToUpdate.ConfirmationDate = DateTime.Now;
                                    DesignatedUserToUpdate.Status = 1;
                                    DesignatedUserToUpdate.UserAccountID = user.Id;
                                    DesignatedUserToUpdate.AcceptedTerms = model.terms;
                                    db.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    ModelState.AddModelError(string.Empty, "Error! Unable to update representative details");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Error! Unable to update representative details");
                            }

                            //Add Signatory user role
                            UserManager.AddToRole(user.Id, "DesignatedUser");
                            //SignIn Signatory to landing page
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            //return RedirectToLocal(returnUrl);
                            //return RedirectToAction("Index", "DesignatedUser");
                            var userRole = db.AspNetUserRoles.SingleOrDefault(e => e.UserId == user.Id);
                            var menuId = (from a in db.SystemMenuAccess
                                          join b in db.SystemMenus on a.menuId equals b.Id
                                          where a.roleId == userRole.RoleId && b.isHomePage == true
                                          select a.menuId).SingleOrDefault();

                            var homePage = db.SystemMenus.SingleOrDefault(c => c.Id == menuId);
                            return RedirectToAction(homePage.action, homePage.controller);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Error! Unable to update representative user account details");
                        }
                    }
                }

                return View();
            }
        }

        //
        //POST: Signatory OTPHasExpired
        [HttpPost]
        public JsonResult RepresentativeOTPHasExpired(string OTPCode, string Email)
        {
            using (DBModel db = new DBModel())
            {
                var EncryptedOTP = Functions.GenerateMD5Hash(OTPCode);
                var OTPExists = db.DesignatedUsers.Any(f => f.OTP == EncryptedOTP && f.Email == Email && f.Status == 0);
                if (OTPExists)
                {
                    var DBOTPToconfirm = db.DesignatedUsers.SingleOrDefault(f => f.OTP == EncryptedOTP && f.Email == Email && f.Status == 0);
                    TimeSpan diff = DateTime.Now - DBOTPToconfirm.DateCreated;
                    var hours = (DateTime.Now - DBOTPToconfirm.DateCreated).TotalHours;
                    var OTPHasExpired = db.DesignatedUsers.Any(f => f.OTP == EncryptedOTP && f.Email == Email && f.Status == 0 && hours > Properties.Settings.Default.RepresentativeOTPExpiry);
                    if (OTPHasExpired)
                    {
                        return Json(DBOTPToconfirm.Id, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("fail", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("fail", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST LoadResendSignatoryOTP
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResendRepresentativeOTP(int getRepresentativeId)
        {
            using (DBModel db = new DBModel())
            {
                var getUserInfo = db.DesignatedUsers.SingleOrDefault(c => c.Id == getRepresentativeId);

                //Data For View Display
                ViewData["UserId"] = getUserInfo.Id;
                ViewData["EmailAddress"] = getUserInfo.Email;
                ViewData["CompanyName"] = getUserInfo.Othernames;
            }

            return PartialView();
        }

        //POST //SubmitResendSignatoryOTP
        [HttpPost]
        public JsonResult SubmitResendRepresentativeOTP(ResendOTPViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    //Generate new OTP
                    var _activationCode = string.Concat(OTPGenerator.GetUniqueKey(6));
                    string activationCode = Shuffle.StringMixer(_activationCode);
                    var _action = "SubmitResendRepresentativeOTP";

                    //Update Client account details
                    var ClientToUpdate = db.DesignatedUsers.SingleOrDefault(b => b.Id == model.UserId);
                    ClientToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                    ClientToUpdate.DateCreated = DateTime.Now;
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        //Send Email with New OTP
                        string EmailBody = string.Empty;
                        var callbackUrl = Url.Action("DesignatedUserConfirmation", "Account", null, Request.Url.Scheme);
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RequestResendOTP.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);
                        EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                        EmailBody = EmailBody.Replace("{NewOTP}", activationCode);

                        var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.EmailAddress, "Confirm Registration", EmailBody);

                        if (SendRegistrationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.EmailAddress, _action);
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.EmailAddress, _action);
                            return Json("Error! Unable to resend your email.", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to save your OTP account details", JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    return Json("" + ex.Message + "", JsonRequestBehavior.AllowGet);
                }
            }
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // GET: /Account/ResetAccount
        [AllowAnonymous]
        public ActionResult ResetAccount()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                var _action = "ForgotPassword";
                //if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed //Only return view
                    return View("ForgotPasswordConfirmation", model);
                }
                else
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    string _code = OTPGenerator.GetUniqueKey(6);
                    string code = Shuffle.StringMixer(_code);
                    
                    //Insert Code id AspnetUsers table
                    using (DBModel db = new DBModel())
                    {
                        try
                        {
                            var updateUser = db.AspNetUsers.SingleOrDefault(c => c.Email == model.Email);
                            updateUser.PasswordResetCode = Functions.GenerateMD5Hash(code);
                            updateUser.AccessFailedCount = 0;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError(string.Empty, "Error! Unable to complete password reset. Please contact administrator or try again later.");
                        }
                    }

                    var callbackUrl = Url.Action("ResetPassword", "Account", null, protocol: Request.Url.Scheme);
                    string EmailBody = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/PasswordReset.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{EmailAddress}", model.Email);
                    EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                    EmailBody = EmailBody.Replace("{ResetCode}", code);

                    var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.Email, "Password Reset", EmailBody);

                    if (SendRegistrationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.Email, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.Email, _action);
                        ModelState.AddModelError(string.Empty, "Error! Unable to send your password reset code.");
                    }

                    return View("ForgotPasswordConfirmation");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return View();
            //return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //var user = await UserManager.FindByNameAsync(model.Email);
                    var decodedCode = Functions.GenerateMD5Hash(model.PasswordResetCode);
                    var _action = "ResetPassword";
                    var UserExists = db.AspNetUsers.SingleOrDefault(c => c.Email == model.Email && c.PasswordResetCode == decodedCode);
                    if (UserExists != null)
                    {
                        //var result = await UserManager.UpdateAsync(user.Id);
                        string code = await UserManager.GeneratePasswordResetTokenAsync(UserExists.Id);
                        var result = await UserManager.ResetPasswordAsync(UserExists.Id, code, model.Password);
                        if (result.Succeeded)
                        {
                            //Update last password changed date
                            var UserToUpdate = db.AspNetUsers.SingleOrDefault(b => b.Id == UserExists.Id);
                            UserToUpdate.LastPasswordChangedDate = DateTime.Now;
                            var _passChanged = db.SaveChanges();

                            //Send Password reset success email
                            var callbackUrl = Url.Action("Index", "Home", null, protocol: Request.Url.Scheme);
                            string EmailBody = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/PasswordResetSuccess.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{EmailAddress}", model.Email);
                            EmailBody = EmailBody.Replace("{Url}", callbackUrl);

                            var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.Email, "Password Reset", EmailBody);

                            if (SendRegistrationCompleteEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.Email, _action);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.Email, _action);
                                ModelState.AddModelError(string.Empty, "Error! Unable to complete your password reset process.");
                            }

                            return RedirectToAction("ResetPasswordConfirmation", "Account");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Error changing your password please contact system administrator or try again later");
                        }
                    }
                    else
                    {
                        // Don't reveal that the user does not exist
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                }
            }
           
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        //
        // Added for confirm Registration
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmRegistration()
        {
            return View();
        }

        //
        // GET: /Account/ChangePassword
        [AllowAnonymous]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        //POST: Change Password
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeUserPassword(ChangePasswordViewModel model, string returnUrl)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (!ModelState.IsValid)
            {
                return View("ChangePassword", model);
            }

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.CurrentPassword, model.ConfirmPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    using (DBModel db = new DBModel())
                    {
                        // get user and update LastPasswordChangedDate
                        var userId = User.Identity.GetUserId();
                        var UserToUpdate = db.AspNetUsers.SingleOrDefault(b => b.Id == userId);
                        UserToUpdate.LastPasswordChangedDate = DateTime.Now;
                        var _passChanged = db.SaveChanges();
                        
                        if (_passChanged > 0)
                        {
                            //Send email confirming password change
                            var _action = "ChangePassword";
                            string EmailBody = string.Empty;
                            var deviceIP = (from address in NetworkInterface.GetAllNetworkInterfaces().Select(x => x.GetIPProperties()).SelectMany(x => x.UnicastAddresses).Select(x => x.Address)
                                                where !IPAddress.IsLoopback(address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                                                select address).FirstOrDefault();
                            var callbackUrl = Url.Action("ResetAccount", "Account", null, Request.Url.Scheme);

                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/PasswordChange.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{CompanyName}", UserToUpdate.CompanyName);
                            EmailBody = EmailBody.Replace("{DatePasswordChanged}", DateTime.Now.ToString());
                            EmailBody = EmailBody.Replace("{DeviceName}", System.Environment.MachineName);
                            EmailBody = EmailBody.Replace("{IP}", deviceIP.ToString());
                            EmailBody = EmailBody.Replace("{ResetUrl}", callbackUrl);

                            var CompleteRegistrationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, UserToUpdate.Email.ToLower(), "Password Changed", EmailBody);
                            if (CompleteRegistrationEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, UserToUpdate.Email.ToLower(), _action);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, UserToUpdate.Email.ToLower(), _action);
                            }

                            //Clear all existing session items
                            Session.Abandon();
                            return RedirectToAction("/PasswordChanged");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Error! Unable to change your password");
                            return View(model);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Error! Unable to change your password");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Error! Unable to change your password");
                return View(model);
            }
        }

        //
        // GET: /Account/PasswordChanged
        [AllowAnonymous]
        public ActionResult PasswordChanged()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
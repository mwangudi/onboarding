using Microsoft.AspNet.Identity;
using OnBoarding.Models;
using OnBoarding.Services;
using OnBoarding.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnBoarding.Controllers
{
    public class SignatoryController : Controller
    {
        // GET: Signatory
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                {
                    var _userDetails = db.AspNetUsers.SingleOrDefault(e => e.Id == currentUserId);
                    var userInfo = db.ClientSignatories.FirstOrDefault(c => c.EmailAddress == _userDetails.Email);
                    if (userInfo != null)
                    {
                        var SignatoriesCount = db.ClientSignatories.Count(c => c.ClientID == userInfo.ClientID);
                        var ApplicationNominations = db.RegisteredClients.Count(a => a.Id == userInfo.ClientID);
                        
                        ViewData["Signatories"] = SignatoriesCount;
                        ViewData["Nominations"] = ApplicationNominations;
                    }
                }
            }

            return View();
        }

        //Get Approvals
        public ActionResult ViewAll()
        {
            return View(GetAllApplications());
        }

        //
        //Get All Applications from table
        private IEnumerable<ClientApplicationsViewModel> GetAllApplications()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var _userDetails = db.AspNetUsers.SingleOrDefault(e => e.Id == currentUserId);

                var Query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT c.ApplicationID, c.ClientID, c.CompanyID, o.CompanyName Client, c.NominationType, c.NominationStatus, CAST(c.DateCreated AS DATETIME) DateCreated FROM ApplicationNominations c INNER JOIN ClientCompanies o ON o.Id = c.CompanyID WHERE c.NomineeEmail = " + "'" + _userDetails.Email + "'" + " ORDER BY c.Id DESC");
                return Query.ToList();
            }
        }

        //
        // GET: /Account/SignatoryConfirmation
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewNominationDetails(int applicationId, int companyId)
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var _userDetails = db.AspNetUsers.SingleOrDefault(e => e.Id == currentUserId);
                var signatoryId = db.ClientSignatories.SingleOrDefault(a => a.EmailAddress == _userDetails.Email && a.CompanyID == companyId);
                
                //Check if application has been approved
                var ApprovedApplications = db.SignatoryApprovals.Any(e => e.SignatoryID == signatoryId.Id && e.ApplicationID == applicationId);
                var DeclinedApplication = db.EMarketApplications.Any(e => e.Id == applicationId && e.Status == 4);
                if (ApprovedApplications || DeclinedApplication)
                {
                    ViewData["Approved"] = 1;
                }
                else
                {
                    ViewData["Approved"] = 0;
                }

                ViewData["SignatoryPhone"] = signatoryId.PhoneNumber;

                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId && c.CompanyID == companyId);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                var clientCompanyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == companyId);
                ViewBag.CompanyInfo = clientCompanyDetails;
                ViewBag.ClientInfo = clientDetails;
                ViewData["ApplicationId"] = applicationId;

                //Get the list of all signatories
                List<ClientSignatory>SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID && a.CompanyID == companyId).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Get the list of authorized representatives
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID && a.CompanyID == companyId).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.Status = 1 AND s.ClientID =  " + "'" + clientID + "'" + " AND s.CompanyID =  " + "'" + getApplicationInfo.CompanyID + "'" + "  AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();
            }
            return PartialView();
        }

        //
        // POST: Approve Nomination
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ApproveNomination(ConfirmApproveViewModel model, HttpPostedFileBase inputFile)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    var currentUserId = User.Identity.GetUserId();
                    var _userDetails = db.AspNetUsers.SingleOrDefault(e => e.Id == currentUserId);
                    var _action = "ApproveNomination";

                    //Upload Signature
                    if (inputFile != null)
                    {
                        string pic = DateTime.Now.ToString("yyyyMMdd") + currentUserId + System.IO.Path.GetFileName(inputFile.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), pic);
                        // Upload file
                        inputFile.SaveAs(path);

                        //Save File name to Database
                        var SignatoryToUpdate = db.ClientSignatories.First(c => c.EmailAddress == _userDetails.Email && c.CompanyID == model.CompanyID);
                        SignatoryToUpdate.Signature = pic;
                        SignatoryToUpdate.UserAccountID = _userDetails.Id;
                        SignatoryToUpdate.PhoneNumber = model.VerifyPhone; //Update phone number
                        db.SaveChanges();
                    }

                    //Check if signatory is also an authorized representative
                    var getUserInfo = db.AspNetUsers.SingleOrDefault(c => c.Id == currentUserId);
                    var signatoryIsARepresentative = db.DesignatedUsers.Any(c => c.Email == getUserInfo.Email && c.CompanyID == model.CompanyID);
                    if (signatoryIsARepresentative)
                    {
                        var signatoryClientId = db.ClientSignatories.First(c => c.UserAccountID == currentUserId);

                        //Update representative's signature
                        var RepresentativeToUpdate = db.DesignatedUsers.First(c => c.Email == signatoryClientId.EmailAddress && c.CompanyID == model.CompanyID);
                        RepresentativeToUpdate.Signature = signatoryClientId.Signature;
                        RepresentativeToUpdate.Mobile = model.VerifyPhone; //Update phone number
                        db.SaveChanges();

                        //Log Signatory Approval
                        try
                        {
                            var LogApproval = db.SignatoryApprovals.Create();
                            LogApproval.ApplicationID = model.ApplicationID;
                            LogApproval.SignatoryID = signatoryClientId.Id;
                            LogApproval.AcceptedTerms = model.terms;
                            LogApproval.DateApproved = DateTime.Now;
                            db.SignatoryApprovals.Add(LogApproval);
                            var savedItem = db.SaveChanges();
                            if(savedItem > 0)
                            { 
                                //Log signatory approval
                                var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == signatoryClientId.EmailAddress && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 1);
                                nominationToUpdate.NominationStatus = 1;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to log signatory approval", JsonRequestBehavior.AllowGet);
                        }

                        //Log Representative Approval
                        try
                        {
                            var userClientId = db.DesignatedUsers.First(c => c.Email == getUserInfo.Email && c.CompanyID == model.CompanyID);
                            var LogDesignatedUserApproval = db.DesignatedUserApprovals.Create();
                            LogDesignatedUserApproval.ApplicationID = model.ApplicationID;
                            LogDesignatedUserApproval.UserID = userClientId.Id;
                            LogDesignatedUserApproval.AcceptedTerms = model.terms;
                            LogDesignatedUserApproval.DateApproved = DateTime.Now;
                            db.DesignatedUserApprovals.Add(LogDesignatedUserApproval);
                            var savedItem = db.SaveChanges();
                            if (savedItem > 0)
                            {
                                //Log signatory approval
                                var repNominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == userClientId.Email && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 2);
                                repNominationToUpdate.NominationStatus = 1;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to log representative approval", JsonRequestBehavior.AllowGet);
                        }

                        //Update application Id
                        var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID && c.CompanyID == model.CompanyID);
                        if (ApplicationUpdate != null)
                        {
                            try
                            {
                                var SignatoryApprovals = ApplicationUpdate.SignatoriesApproved;
                                var UsersApprovals = ApplicationUpdate.UsersApproved;
                                ApplicationUpdate.SignatoriesApproved = SignatoryApprovals + 1;
                                ApplicationUpdate.UsersApproved = UsersApprovals + 1;
                                ApplicationUpdate.SignatoriesDateApproved = DateTime.Now;
                                ApplicationUpdate.UsersDateApproved = DateTime.Now;
                                db.SaveChanges();
                            }
                            catch (Exception)
                            {
                                return Json("Error! Unable to update application details", JsonRequestBehavior.AllowGet);
                            }

                            //Check if all signatories have approved
                            if (ApplicationUpdate.SignatoriesApproved >= ApplicationUpdate.Signatories)
                            {
                                //Send Emails to representatives for approval excluding the existing users
                                foreach (var email in db.DesignatedUsers.Where(c => c.ClientID == signatoryClientId.ClientID && c.CompanyID == model.CompanyID).ToList())
                                {
                                    var emailExists = db.AspNetUsers.Any(x => x.Email.ToLower() == email.Email.ToLower());
                                    if(!emailExists)
                                    {
                                        //1. Update Designated User with OTP to Login
                                        var _OTPCode = OTPGenerator.GetUniqueKey(6);
                                        string OTPCode = Shuffle.StringMixer(_OTPCode);
                                        var UserToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == email.Email && c.CompanyID == model.CompanyID);
                                        UserToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                                        UserToUpdate.DateCreated = DateTime.Now;
                                        db.SaveChanges();

                                        //2. Send Email To Representatives with OTP
                                        var callbackUrl = Url.Action("DesignatedUserConfirmation", "Account", null, Request.Url.Scheme);
                                        string EmailBodyRep = string.Empty;
                                        
                                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeNomination.html")))
                                        {
                                            EmailBodyRep = reader.ReadToEnd();
                                        }
                                        EmailBodyRep = EmailBodyRep.Replace("{RepresentativeName}", email.Surname);
                                        EmailBodyRep = EmailBodyRep.Replace("{ActivationCode}", OTPCode);
                                        EmailBodyRep = EmailBodyRep.Replace("{URL}", callbackUrl);

                                        var EmailToRepresentative = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.Email.ToLower(), "Authorized Representative Confirmation", EmailBodyRep);
                                        if (EmailToRepresentative == true)
                                        {
                                            //Log email sent notification
                                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                        else
                                        {
                                            //Log Email failed notification
                                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                    }
                                    else
                                    {
                                        //Send Email To Representatives without OTP
                                        var callbackUrl = Url.Action("Index", "Home", null, Request.Url.Scheme);
                                        string EmailBodyRep = string.Empty;

                                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ExistingRepresentativeNomination.html")))
                                        {
                                            EmailBodyRep = reader.ReadToEnd();
                                        }
                                        EmailBodyRep = EmailBodyRep.Replace("{RepresentativeName}", email.Surname);
                                        EmailBodyRep = EmailBodyRep.Replace("{URL}", callbackUrl);

                                        var EmailToRepresentative = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.Email.ToLower(), "Authorized Representative Confirmation", EmailBodyRep);
                                        if (EmailToRepresentative == true)
                                        {
                                            //Log email sent notification
                                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                        else
                                        {
                                            //Log Email failed notification
                                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Json("Error! Unable to update representative details!", JsonRequestBehavior.AllowGet);
                        }

                        //Send email to signatory after approval
                        string EmailBody = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/SignatoryRepresentativeApproval.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{Othernames}", signatoryClientId.OtherNames);
                        EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                        var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, signatoryClientId.EmailAddress, "Signatory/Authorized Representative Approval", EmailBody);

                        if (SendRegistrationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, signatoryClientId.EmailAddress, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, signatoryClientId.EmailAddress, _action);
                        }
                        
                        //Send email to company
                        string EmailBody2 = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientConfirmationApproval.html")))
                        {
                            EmailBody2 = reader.ReadToEnd();
                        }
                        EmailBody2 = EmailBody2.Replace("{Othernames}", model.CompanySurname);
                        EmailBody2 = EmailBody2.Replace("{ApproversName}", signatoryClientId.Surname + " " + signatoryClientId.OtherNames);

                        var SendClientConfirmationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Signatory/Authorized Representative Approval", EmailBody2);

                        if (SendClientConfirmationEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody2, model.CompanyEmail, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody2, model.CompanyEmail, _action);
                        }
                    }

                    //When signatory is not in representative list
                    else
                    {
                        var signatoryClientId = db.ClientSignatories.First(c => c.EmailAddress == _userDetails.Email && c.CompanyID == model.CompanyID);
                        //Log Signatory Approval
                        try
                        {
                            var LogApproval = db.SignatoryApprovals.Create();
                            LogApproval.ApplicationID = model.ApplicationID;
                            LogApproval.SignatoryID = signatoryClientId.Id;
                            LogApproval.AcceptedTerms = model.terms;
                            LogApproval.DateApproved = DateTime.Now;
                            db.SignatoryApprovals.Add(LogApproval);
                            var savedItem = db.SaveChanges();
                            if (savedItem > 0)
                            {
                                var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == signatoryClientId.EmailAddress && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 1);
                                nominationToUpdate.NominationStatus = 1;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to log signatory approval", JsonRequestBehavior.AllowGet);
                        }

                        //Update application Id
                        var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID);
                        if (ApplicationUpdate != null)
                        {
                            try
                            {
                                var SignatoryApprovals = ApplicationUpdate.SignatoriesApproved;
                                ApplicationUpdate.SignatoriesApproved = SignatoryApprovals + 1;
                                ApplicationUpdate.SignatoriesDateApproved = DateTime.Now;
                                db.SaveChanges();
                            }
                            catch (Exception)
                            {
                                return Json("Error! Unable to update application details", JsonRequestBehavior.AllowGet);
                            }

                            //Check if all signatories have approved
                            if (ApplicationUpdate.SignatoriesApproved >= ApplicationUpdate.Signatories)
                            {
                                //Send Emails to representatives for approval excluding the existing users
                                foreach (var email in db.DesignatedUsers.Where(c => c.ClientID == signatoryClientId.ClientID && c.CompanyID == model.CompanyID).ToList())
                                {
                                    var emailExists = db.AspNetUsers.Any(x => x.Email.ToLower() == email.Email.ToLower());
                                    if (!emailExists)
                                    {
                                        //1. Update Designated User with OTP to Login
                                        var _OTPCode = OTPGenerator.GetUniqueKey(6);
                                        string OTPCode = Shuffle.StringMixer(_OTPCode);
                                        var UserToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == email.Email && c.CompanyID == model.CompanyID);
                                        UserToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                                        UserToUpdate.DateCreated = DateTime.Now;
                                        db.SaveChanges();

                                        //2. Send Email To Representatives with OTP
                                        var callbackUrl = Url.Action("DesignatedUserConfirmation", "Account", null, Request.Url.Scheme);
                                        string EmailBodyRep = string.Empty;

                                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeNomination.html")))
                                        {
                                            EmailBodyRep = reader.ReadToEnd();
                                        }
                                        EmailBodyRep = EmailBodyRep.Replace("{RepresentativeName}", email.Surname);
                                        EmailBodyRep = EmailBodyRep.Replace("{ActivationCode}", OTPCode);
                                        EmailBodyRep = EmailBodyRep.Replace("{URL}", callbackUrl);

                                        var EmailToRepresentative = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.Email.ToLower(), "Authorized Representative Confirmation", EmailBodyRep);
                                        if (EmailToRepresentative == true)
                                        {
                                            //Log email sent notification
                                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                        else
                                        {
                                            //Log Email failed notification
                                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                    }
                                    else
                                    {
                                        //Send Email To Representatives without OTP
                                        var callbackUrl = Url.Action("Index", "Home", null, Request.Url.Scheme);
                                        string EmailBodyRep = string.Empty;

                                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ExistingRepresentativeNomination.html")))
                                        {
                                            EmailBodyRep = reader.ReadToEnd();
                                        }
                                        EmailBodyRep = EmailBodyRep.Replace("{RepresentativeName}", email.Surname);
                                        EmailBodyRep = EmailBodyRep.Replace("{URL}", callbackUrl);

                                        var EmailToRepresentative = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.Email.ToLower(), "Authorized Representative Confirmation", EmailBodyRep);
                                        if (EmailToRepresentative == true)
                                        {
                                            //Log email sent notification
                                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                        else
                                        {
                                            //Log Email failed notification
                                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email.ToLower(), _action);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Json("Unable to Update Details!", JsonRequestBehavior.AllowGet);
                        }

                        //Send email to signatory
                        string EmailBody = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/SignatoryApproval.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{Othernames}", signatoryClientId.OtherNames);
                        EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                        var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, signatoryClientId.EmailAddress, "Signatory Approval", EmailBody);

                        if (SendRegistrationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, signatoryClientId.EmailAddress, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, signatoryClientId.EmailAddress, _action);
                        }

                        //Send email to company
                        string EmailBody2 = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientConfirmationApproval.html")))
                        {
                            EmailBody2 = reader.ReadToEnd();
                        }
                        EmailBody2 = EmailBody2.Replace("{Othernames}", model.CompanySurname);
                        EmailBody2 = EmailBody2.Replace("{ApproversName}", signatoryClientId.Surname + " " + signatoryClientId.OtherNames);

                        var SendClientConfirmationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Signatory Approval", EmailBody2);

                        if (SendClientConfirmationEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody2, model.CompanyEmail, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody2, model.CompanyEmail, _action);
                        }
                    }
                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Model Invalid! " + errors + " ", JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: Decline Nomination
        [HttpPost]
        [AllowAnonymous]
        public JsonResult DeclineNomination(DeclineNominationViewModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                //Upload Signature
                using (DBModel db = new DBModel())
                {
                    //1. Log approval
                    var currentUserId = User.Identity.GetUserId();
                    var signatoryClientId = db.ClientSignatories.First(c => c.UserAccountID == currentUserId);
                    var LogApproval = db.SignatoryApprovals.Create();
                    var _action = "DeclineNomination";
                    LogApproval.ApplicationID = model.ApplicationID;
                    LogApproval.SignatoryID = signatoryClientId.Id;
                    LogApproval.DateApproved = DateTime.Now;
                    LogApproval.Comments = model.Comments;
                    LogApproval.Status = 4;
                    db.SignatoryApprovals.Add(LogApproval);
                    var savedItem = db.SaveChanges();
                    if (savedItem > 0)
                    {
                        //Log signatory approval
                        var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == signatoryClientId.EmailAddress && c.ApplicationID == model.ApplicationID && c.CompanyID == signatoryClientId.CompanyID && c.NominationType == 1);
                        nominationToUpdate.NominationStatus = 2;
                        db.SaveChanges();
                    }

                    //2. Update application Id
                    var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID);
                    var Approvals = ApplicationUpdate.SignatoriesApproved;
                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            ApplicationUpdate.Status = 4;
                            ApplicationUpdate.SignatoriesDateApproved = DateTime.Now;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        return Json("Unable to Update Application Status Details!", JsonRequestBehavior.AllowGet);
                    }

                    //3. Send email to signatory
                    string EmailBody = string.Empty;
                    using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/SignatoryDecline.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{Othernames}", signatoryClientId.OtherNames);
                    EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                    var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, signatoryClientId.EmailAddress, "Signatory Decline", EmailBody);

                    if (SendRegistrationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, signatoryClientId.EmailAddress, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, signatoryClientId.EmailAddress, _action);
                    }

                    //4. Send email to client company
                    string EmailBody2 = string.Empty;
                    using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientConfirmationSignatoryDecline.html")))
                    {
                        EmailBody2 = reader.ReadToEnd();
                    }
                    EmailBody2 = EmailBody2.Replace("{SignatoryName}", signatoryClientId.Surname + " " + signatoryClientId.OtherNames);
                    EmailBody2 = EmailBody2.Replace("{CompanyName}", model.CompanyName);

                    var SendClientConfirmationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Signatory Decline", EmailBody2);

                    if (SendClientConfirmationEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody2, model.CompanyEmail, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody2, model.CompanyEmail, _action);
                    }

                    return Json("success", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Model Invalid! " + errors + " ", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
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
    public class DesignatedUserController : Controller
    {
        // GET: DesignatedUser
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                {
                    var userInfo = db.DesignatedUsers.SingleOrDefault(c => c.UserAccountID == currentUserId);
                    if (userInfo != null)
                    {
                        var NominationsCount = db.DesignatedUsers.Count(c => c.UserAccountID == currentUserId);
                        var ApplicationNominator = db.RegisteredClients.FirstOrDefault(a => a.Id == userInfo.ClientID);

                        //ViewData["CompanyName"] = ApplicationNominator.CompanyName;
                        ViewData["Nominations"] = NominationsCount;
                    }
                }
            }

            return View();
        }

        //
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
                var userClientId = db.DesignatedUsers.FirstOrDefault(c => c.Email == _userDetails.Email);
                var ApprovedApplications = db.DesignatedUserApprovals.Count(e => e.UserID == userClientId.Id);

                if (ApprovedApplications >= 1)
                {
                    ViewData["Approved"] = 1;
                }
                else
                {
                    ViewData["Approved"] = null;
                }

                var Query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT c.ApplicationID, c.ClientID, c.CompanyID, o.CompanyName Client, c.NominationType, c.NominationStatus, CAST(c.DateCreated AS DATETIME) DateCreated FROM ApplicationNominations c INNER JOIN ClientCompanies O ON O.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.Id = c.ApplicationID WHERE e.Status = 1 AND c.NomineeEmail = " + "'" + _userDetails.Email + "'" + " ORDER BY c.DateCreated DESC, c.NominationType ASC");
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
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId && c.CompanyID == companyId);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                var companyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == companyId);
                ViewBag.ApplicationInfo = clientDetails;
                ViewBag.CompanyInfo = companyDetails;

                var currentUserId = User.Identity.GetUserId();
                var _userDetails = db.AspNetUsers.SingleOrDefault(e => e.Id == currentUserId);
                var representativeDetails = db.DesignatedUsers.SingleOrDefault(d => d.Email == _userDetails.Email && d.CompanyID == companyId);
                
                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                ViewData["CompanySurname"] = clientDetails.AccountName;
                ViewData["RepresentativePhone"] = representativeDetails.Mobile;

                //Get the list of Designated Users
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID && a.CompanyID == companyId).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.Status = 1 AND s.ClientID =  " + "'" + clientID + "'" + " AND s.CompanyID =  " + "'" + getApplicationInfo.CompanyID + "'" + "  AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Check if application has been approved
                var ApprovedApplications = db.DesignatedUserApprovals.Any(e => e.UserID == representativeDetails.Id && e.ApplicationID == applicationId);
                var DeclinedApplication = db.EMarketApplications.Any(e => e.Id == applicationId && e.Status == 4);
                if (ApprovedApplications || DeclinedApplication)
                {
                    ViewData["Approved"] = 1;
                }
                else
                {
                    ViewData["Approved"] = 0;
                }
            }

            return PartialView();
        }

        //
        // POST: Approve Designated User
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
                    var userClientId = db.DesignatedUsers.First(c => c.Email == _userDetails.Email && c.CompanyID == model.CompanyID);
                                       
                    //1. Upload Signature
                    if (inputFile != null)
                    {
                        string pic = DateTime.Now.ToString("yyyyMMdd") + currentUserId + System.IO.Path.GetFileName(inputFile.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), pic);
                        // Upload file
                        inputFile.SaveAs(path);
                        try
                        {
                            //Save File name to Database  //Update Representative Details
                            var DesignatedUserToUpdate = db.DesignatedUsers.FirstOrDefault(c => c.Email == userClientId.Email && c.CompanyID == model.CompanyID);
                            DesignatedUserToUpdate.Signature = pic;
                            DesignatedUserToUpdate.Mobile = model.VerifyPhone;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to update representative details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to upload representative signature", JsonRequestBehavior.AllowGet);
                    }

                    //2. Check if authorized representative is also a signatory
                    var getUserInfo = db.AspNetUsers.SingleOrDefault(c => c.Id == currentUserId);
                    var representativeIsASignatory = db.ClientSignatories.Any(c => c.EmailAddress == getUserInfo.Email && c.CompanyID == model.CompanyID);
                    if (representativeIsASignatory)
                    {
                        var representativeClientId = db.DesignatedUsers.First(c => c.Email == getUserInfo.Email && c.CompanyID == model.CompanyID);

                        //1. Update signatory's signature
                        var SignatoryToUpdate = db.ClientSignatories.First(c => c.EmailAddress == representativeClientId.Email && c.CompanyID == model.CompanyID);
                        SignatoryToUpdate.Signature = representativeClientId.Signature;
                        SignatoryToUpdate.PhoneNumber = model.VerifyPhone; //Update phone number
                        db.SaveChanges();

                        //2. Log Representative's Approval
                        try
                        {
                            var LogApproval = db.DesignatedUserApprovals.Create();
                            LogApproval.ApplicationID = model.ApplicationID;
                            LogApproval.UserID = representativeClientId.Id;
                            LogApproval.AcceptedTerms = model.terms;
                            LogApproval.DateApproved = DateTime.Now;
                            db.DesignatedUserApprovals.Add(LogApproval);
                            var savedItem = db.SaveChanges();
                            if (savedItem > 0)
                            {
                                //Log Representative approval
                                var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == userClientId.Email && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 2);
                                nominationToUpdate.NominationStatus = 1;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to log representative's details", JsonRequestBehavior.AllowGet);
                        }

                        //3. Log Signatory Approval
                        try
                        {
                            var signatoryClientId = db.ClientSignatories.First(c => c.EmailAddress == _userDetails.Email && c.CompanyID == model.CompanyID);
                            var LogSigApproval = db.SignatoryApprovals.Create();
                            LogSigApproval.ApplicationID = model.ApplicationID;
                            LogSigApproval.SignatoryID = signatoryClientId.Id;
                            LogSigApproval.AcceptedTerms = model.terms;
                            LogSigApproval.DateApproved = DateTime.Now;
                            db.SignatoryApprovals.Add(LogSigApproval);
                            var savedItem = db.SaveChanges();
                            if (savedItem > 0)
                            {
                                //Log nomination
                                var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == userClientId.Email && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 1);
                                nominationToUpdate.NominationStatus = 1;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json("Error! Unable to log signatory approval" + ex.Message, JsonRequestBehavior.AllowGet);
                        }

                        //4. Update Application ID
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
                        }
                        else
                        {
                            return Json("Unable to update application details!", JsonRequestBehavior.AllowGet);
                        }

                        //5. Send email to authorized representative
                        string EmailBody = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeApproval.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{Othernames}", userClientId.Othernames);
                        EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                        var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, userClientId.Email, "Authorised Representative/Signatory Approval", EmailBody);

                        if (SendRegistrationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, userClientId.Email, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, userClientId.Email, _action);
                        }

                        //6. Send email to company
                        string EmailBody2 = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientConfirmationApproval.html")))
                        {
                            EmailBody2 = reader.ReadToEnd();
                        }
                        EmailBody2 = EmailBody2.Replace("{Othernames}", model.CompanySurname);
                        EmailBody2 = EmailBody2.Replace("{ApproversName}", userClientId.Surname + " " + userClientId.Othernames);

                        var SendClientConfirmationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Authorised Representative/Signatory Approval", EmailBody2);

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

                        //7. Check if all signatories have approved and send email to representatives
                        if (ApplicationUpdate.SignatoriesApproved >= ApplicationUpdate.Signatories)
                        {
                            //Send Emails to representatives for approval excluding the signatory and sole signatory if in list
                            var _dontSendEmail = db.RegisteredClients.Select(x => x.EmailAddress).ToList();
                            foreach (var email in db.DesignatedUsers.Where(c => c.ClientID == userClientId.ClientID && c.CompanyID == model.CompanyID && !_dontSendEmail.Contains(c.Email)).ToList())
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

                                    //2. Send Email To Representatives
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
                                    //Check if he has already approved
                                    var userHasApproved = db.ApplicationNominations.Any(c => c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NomineeEmail.ToLower() == email.Email.ToLower() && c.NominationType == 2 && c.NominationStatus == 1);
                                    if (!userHasApproved)
                                    {
                                        //1. Send Email To Representatives
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

                        //8. Check if all Representatives have approved and send complete email to digital desk
                        var ApplicationToCheck = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID && c.CompanyID == model.CompanyID);
                        if (ApplicationToCheck.UsersApproved == ApplicationToCheck.DesignatedUsers)
                        {
                            //Send Email to Digital Desk and Ops for Approval process
                            var DDUserRole = (from p in db.AspNetUserRoles
                                              join e in db.AspNetUsers on p.UserId equals e.Id
                                              where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc" && p.RoleId == "05bdc847-b94d-4d2f-957e-8de1d563106a"
                                              select new
                                              {
                                                  EmailID = e.Email
                                              }).ToList();
                            var companyInfo = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                            foreach (var email in DDUserRole)
                            {
                                var DDMessageBody = "Dear Team <br/><br/> Kindly note that the all signatories and representatives of " + companyInfo.CompanyName + " have approved their nomination. <br/>" +
                                              "<br/><br/> Kind Regards, <br /><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                                var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Application Completed", DDMessageBody);
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
                        }
                    }
                    else
                    {
                        //If representative is not a signtory
                        //1. Log Representative's Approval
                        try
                        {
                            var LogApproval = db.DesignatedUserApprovals.Create();
                            LogApproval.ApplicationID = model.ApplicationID;
                            LogApproval.UserID = userClientId.Id;
                            LogApproval.AcceptedTerms = model.terms;
                            LogApproval.DateApproved = DateTime.Now;
                            db.DesignatedUserApprovals.Add(LogApproval);
                            var savedItem = db.SaveChanges();
                            if (savedItem > 0)
                            {
                                //Log Representative approval
                                var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == userClientId.Email && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 2);
                                nominationToUpdate.NominationStatus = 1;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to log representatives details", JsonRequestBehavior.AllowGet);
                        }

                        //2. Update application ID
                        var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID && c.CompanyID == model.CompanyID);
                        if (ApplicationUpdate != null)
                        {
                            try
                            {
                                var SignatoryApprovals = ApplicationUpdate.SignatoriesApproved;
                                var UsersApprovals = ApplicationUpdate.UsersApproved;
                                //ApplicationUpdate.SignatoriesApproved = SignatoryApprovals + 1;
                                ApplicationUpdate.UsersApproved = UsersApprovals + 1;
                                ApplicationUpdate.SignatoriesDateApproved = DateTime.Now;
                                ApplicationUpdate.UsersDateApproved = DateTime.Now;
                                db.SaveChanges();
                            }
                            catch (Exception)
                            {
                                return Json("Error! Unable to update application details", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("Unable to update application details!", JsonRequestBehavior.AllowGet);
                        }

                        //5. Send email to authorized representative
                        string EmailBody = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeApproval.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{Othernames}", userClientId.Othernames);
                        EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                        var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, userClientId.Email, "Authorised Representative Approval", EmailBody);

                        if (SendRegistrationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, userClientId.Email, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, userClientId.Email, _action);
                        }

                        //6. Send email to company
                        string EmailBody2 = string.Empty;
                        using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientConfirmationApproval.html")))
                        {
                            EmailBody2 = reader.ReadToEnd();
                        }
                        EmailBody2 = EmailBody2.Replace("{Othernames}", model.CompanySurname);
                        EmailBody2 = EmailBody2.Replace("{ApproversName}", userClientId.Surname + " " + userClientId.Othernames);

                        var SendClientConfirmationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Authorised Representative Approval", EmailBody2);

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

                        //7. Check if all Representatives have approved and send complete email to digital desk and ops
                        var ApplicationToCheck = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID && c.CompanyID == model.CompanyID);
                        if (ApplicationToCheck.UsersApproved == ApplicationToCheck.DesignatedUsers)
                        {
                            //Send Email to Digital Desk and Ops for Approval process
                            var DDUserRole = (from p in db.AspNetUserRoles
                                              join e in db.AspNetUsers on p.UserId equals e.Id
                                              where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc" && p.RoleId == "05bdc847-b94d-4d2f-957e-8de1d563106a"
                                              select new
                                              {
                                                  EmailID = e.Email
                                              }).ToList();
                            var companyInfo = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                            foreach (var email in DDUserRole)
                            {
                                var DDMessageBody = "Dear Team <br/><br/> Kindly note that the all signatories and representatives of " + companyInfo.CompanyName + " have approved their nomination. <br/>" +
                                              "<br/><br/> Kind Regards, <br /><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                                var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Application Completed", DDMessageBody);
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
        public JsonResult DeclineNomination(UserDeclineNominationViewModel model, HttpPostedFileBase inputFile)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                //Upload Signature
                using (DBModel db = new DBModel())
                {
                    var currentUserId = User.Identity.GetUserId();
                    var userClientId = db.DesignatedUsers.First(c => c.UserAccountID == currentUserId && c.CompanyID == model.CompanyID);
                    var _action = "RepresentativeDeclineNomination";
                    //Log approval
                    //Upload Signature
                    if (inputFile != null)
                    {
                        string pic = DateTime.Now.ToString("yyyyMMdd") + currentUserId + System.IO.Path.GetFileName(inputFile.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), pic);
                        // Upload file
                        inputFile.SaveAs(path);

                        //Save File name to Database  //Update Representative Details
                        var DesignatedUserToUpdate = db.DesignatedUsers.FirstOrDefault(c => c.UserAccountID == currentUserId && c.CompanyID == model.CompanyID);
                        DesignatedUserToUpdate.Signature = pic;
                        DesignatedUserToUpdate.Mobile = model.VerifyPhone;
                        db.SaveChanges();
                    }

                    //Log Approval
                    var LogApproval = db.DesignatedUserApprovals.Create();
                    LogApproval.ApplicationID = model.ApplicationID;
                    LogApproval.UserID = userClientId.Id;
                    LogApproval.DateApproved = DateTime.Now;
                    LogApproval.Comments = model.Comments;
                    LogApproval.Status = 4;
                    db.DesignatedUserApprovals.Add(LogApproval);
                    var savedItem = db.SaveChanges();
                    if (savedItem > 0)
                    {
                        //Log signatory approval
                        var nominationToUpdate = db.ApplicationNominations.SingleOrDefault(c => c.NomineeEmail == userClientId.Email && c.ApplicationID == model.ApplicationID && c.CompanyID == model.CompanyID && c.NominationType == 2);
                        nominationToUpdate.NominationStatus = 2;
                        db.SaveChanges();
                    }

                    //Update application Id
                    var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID);
                    var Approvals = ApplicationUpdate.UsersApproved;
                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            ApplicationUpdate.Status = 4;
                            ApplicationUpdate.UsersDateApproved = DateTime.Now;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to update application status details!", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to update application status details!", JsonRequestBehavior.AllowGet);
                    }

                    //1. Send email to Authorized Representative
                    string EmailBody = string.Empty;
                    using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeDecline.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{Othernames}", userClientId.Othernames);
                    EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                    var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, userClientId.Email, "Authorised Representative Decline", EmailBody);

                    if (SendRegistrationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, userClientId.Email, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, userClientId.Email, _action);
                    }

                    //2. Send confirmation email to client company
                    string EmailBody2 = string.Empty;
                    using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientConfirmationRepresentativeDecline.html")))
                    {
                        EmailBody2 = reader.ReadToEnd();
                    }
                    EmailBody2 = EmailBody2.Replace("{RepresentativeName}", userClientId.Surname + " " + userClientId.Othernames);
                    EmailBody2 = EmailBody2.Replace("{CompanyName}", model.CompanyName);

                    var SendClientConfirmationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Authorised Representative Decline", EmailBody2);

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
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Model Invalid! " + errors + " ", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
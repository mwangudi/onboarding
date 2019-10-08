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
                var signatoryClientId = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == currentUserId);

                var Query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT e.Id ApplicationID, c.ClientID, c.CompanyID, o.CompanyName Client, t.StatusName Status, c.AcceptedTerms AcceptedTAC, CAST(c.DateCreated AS DATETIME) DateCreated FROM ClientSignatories c INNER JOIN ClientCompanies o ON o.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.CompanyID = c.CompanyID  INNER JOIN tblStatus t ON t.Id = e.Status WHERE c.EmailAddress = " + "'" + _userDetails.Email + "'" + "");
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
                var DeclinedApplication = db.EMarketApplications.Any(e => e.Id == applicationId && e.Status == 2);
                if (ApprovedApplications || DeclinedApplication)
                {
                    ViewData["Approved"] = 1;
                }
                else
                {
                    ViewData["Approved"] = 0;
                }

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
                        db.SaveChanges();
                    }

                    //Check if signatory is also an authorized representative
                    var getUserInfo = db.AspNetUsers.SingleOrDefault(c => c.Id == currentUserId);
                    var signatoryIsARepresentative = db.DesignatedUsers.Any(c => c.Email == getUserInfo.Email && c.CompanyID == model.CompanyID);
                    if (signatoryIsARepresentative)
                    {
                        //var signatoryClientId = db.ClientSignatories.First(c => c.UserAccountID == currentUserId);
                        var signatoryClientId = db.ClientSignatories.First(c => c.EmailAddress == _userDetails.Email && c.CompanyID == model.CompanyID);

                        //Update representative's signature
                        var RepresentativeToUpdate = db.DesignatedUsers.First(c => c.Email == signatoryClientId.EmailAddress && c.CompanyID == model.CompanyID);
                        RepresentativeToUpdate.Signature = signatoryClientId.Signature;
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
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
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
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }

                        //Update application Id
                        var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID);
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
                            catch (Exception ex)
                            {
                                throw (ex);
                            }

                            //Check if all signatories have approved
                            if (ApplicationUpdate.SignatoriesApproved >= ApplicationUpdate.Signatories)
                            {
                                //Send Emails to representatives for approval excluding the signatory and sole signatory if in list
                                var UserToExclude = db.RegisteredClients.SingleOrDefault(c => c.Id == signatoryClientId.ClientID);
                                var UserToExclude2 = getUserInfo.Email;
                                foreach (var email in db.DesignatedUsers.Where(c => c.ClientID == signatoryClientId.ClientID && c.CompanyID == model.CompanyID && (c.Email != UserToExclude.EmailAddress && c.Email != UserToExclude2)).ToList())
                                {
                                    //Update Designated User with OTP to Login
                                    var _OTPCode = OTPGenerator.GetUniqueKey(6);
                                    string OTPCode = Shuffle.StringMixer(_OTPCode);
                                    var UserToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == email.Email);
                                    UserToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                                    UserToUpdate.DateCreated = DateTime.Now;
                                    db.SaveChanges();

                                    var userInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == signatoryClientId.ClientID);
                                    var CompanyName = userInfo.Surname;

                                    //Send Email To Representatives
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
                                    var EmailToDesignatedUsers = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.Email, "Authorized Representative Confirmation", EmailBodyRep);
                                    if (EmailToDesignatedUsers == true)
                                    {
                                        //Log email sent notification
                                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email, _action);
                                    }
                                    else
                                    {
                                        //Log Email failed notification
                                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBodyRep, email.Email, _action);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Json("Unable to Update Details!", JsonRequestBehavior.AllowGet);
                        }

                        //Send email to signatory after approval
                        var ApprovalCompleteEmailMessage = "Dear " + signatoryClientId.Surname + ", <br/><br/> Thank you for accepting nomination as signatory and authorized representative from " + model.CompanyName + ".<br/>" +
                            "You have also accepted our terms and conditions for trading on eMarket Trader. <br/>" +
                            "Thank you for your continued custom." +
                            "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var ApprovalCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, signatoryClientId.EmailAddress, "Signatory/Authorized Representative Confirmation", ApprovalCompleteEmailMessage);
                        if (ApprovalCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, signatoryClientId.EmailAddress, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, signatoryClientId.EmailAddress, _action);
                        }

                        //Send email to company
                        var ApprovalCompleteEmailMessage2 = "Dear " + model.CompanySurname + ", <br/><br/> " + signatoryClientId.Surname + " " + signatoryClientId.OtherNames + " has confirmed the information submitted and completed the process.<br/>" +
                                        "Thank you for your continued custom." +
                                        "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var ApprovalCompleteEmail2 = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Signatory/Authorized Representative Confirmation", ApprovalCompleteEmailMessage2);
                        if (ApprovalCompleteEmail2 == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage2, model.CompanyEmail, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage2, model.CompanyEmail, _action);
                        }
                    }

                    //When not in representative list
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
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
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
                            catch (Exception ex)
                            {
                                throw (ex);
                            }

                            //Check if all signatories have approved
                            if (ApplicationUpdate.SignatoriesApproved >= ApplicationUpdate.Signatories)
                            {
                                //Send Emails to representatives for approval
                                var UserToExclude = db.RegisteredClients.SingleOrDefault(c => c.Id == signatoryClientId.ClientID);
                                foreach (var email in db.DesignatedUsers.Where(c => c.ClientID == signatoryClientId.ClientID && c.CompanyID == model.CompanyID && c.Email != UserToExclude.EmailAddress).ToList())
                                {
                                    //Update representatives with OTP for Login
                                    var _OTPCode = OTPGenerator.GetUniqueKey(6);
                                    string OTPCode = Shuffle.StringMixer(_OTPCode);
                                    var UserToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == email.Email && c.CompanyID == model.CompanyID);
                                    UserToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                                    db.SaveChanges();

                                    var userInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == signatoryClientId.ClientID);
                                    var CompanyName = userInfo.Surname;

                                    //Send Email to representatives
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
                            }
                        }
                        else
                        {
                            return Json("Unable to Update Details!", JsonRequestBehavior.AllowGet);
                        }

                        //Send email to signatory
                        var ApprovalCompleteEmailMessage = "Dear " + signatoryClientId.OtherNames + ", <br/><br/> Thank you for accepting nomination as a signatory from " + model.CompanyName + ".<br/>" +
                            "You have also accepted our terms and conditions for trading on the portal. <br/>" +
                            "Thank you for your continued custom." +
                            "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var ApprovalCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, signatoryClientId.EmailAddress, "Signatory Confirmation", ApprovalCompleteEmailMessage);
                        if (ApprovalCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, signatoryClientId.EmailAddress, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, signatoryClientId.EmailAddress, _action);
                        }

                        //Send email to company
                        var ApprovalCompleteEmailMessage2 = "Dear " + model.CompanySurname + ", <br/><br/> " + signatoryClientId.Surname + " " + signatoryClientId.OtherNames + " has confirmed the information submitted and completed the process.<br/>" +
                          "Thank you for your continued custom. <br/><br/>" +
                          "Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var ApprovalCompleteEmail2 = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Signatory Confirmation", ApprovalCompleteEmailMessage2);
                        if (ApprovalCompleteEmail2 == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage2, model.CompanyEmail, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage2, model.CompanyEmail, _action);
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
                    //Log approval
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
                    db.SaveChanges();

                    //Update application Id
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

                    //Send email to signatory
                    var ApprovalCompleteEmailMessage = "Dear " + signatoryClientId.Surname + ", <br/><br/> You have declined your nomination as signatory from " + model.CompanyName + ".<br/>" +
                         "Thank you for your continued custom. <br/><br/>" +
                         "Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                    var ApprovalCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, signatoryClientId.EmailAddress, "Signatory Decline", ApprovalCompleteEmailMessage);
                    if (ApprovalCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, signatoryClientId.EmailAddress, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, signatoryClientId.EmailAddress, _action);
                    }

                    //Send email to client company
                    var ApprovalCompleteEmailMessage2 = "Dear " + model.CompanyName + ", <br/><br/> " + signatoryClientId.Surname + " " + signatoryClientId.OtherNames + " has declined your nomination as signatory.<br/>" +
                         "Thank you for your continued custom." +
                         "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                    var ApprovalCompleteEmail2 = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Signatory Decline", ApprovalCompleteEmailMessage2);
                    if (ApprovalCompleteEmail2 == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage2, model.CompanyEmail, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage2, model.CompanyEmail, _action);
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
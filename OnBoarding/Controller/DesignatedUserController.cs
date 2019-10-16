using Microsoft.AspNet.Identity;
using OnBoarding.Models;
using OnBoarding.Services;
using OnBoarding.ViewModels;
using System;
using System.Collections.Generic;
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
                var userClientId = db.DesignatedUsers.First(c => c.UserAccountID == currentUserId);
                var ApprovedApplications = db.DesignatedUserApprovals.Count(e => e.UserID == userClientId.Id);

                if (ApprovedApplications >= 1)
                {
                    ViewData["Approved"] = 1;
                }
                else
                {
                    ViewData["Approved"] = null;
                }

                var Query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT e.Id ApplicationID, c.ClientID, c.CompanyID, o.CompanyName Client, t.StatusName Status, c.AcceptedTerms AcceptedTAC, CAST(c.DateCreated AS DATETIME) DateCreated FROM DesignatedUsers c INNER JOIN ClientCompanies o ON o.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.CompanyID = c.CompanyID  INNER JOIN tblStatus t ON t.Id = e.Status WHERE c.Email = " + "'" + userClientId.Email + "'" + "");
               
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
                var representativeDetails = db.DesignatedUsers.SingleOrDefault(d => d.UserAccountID == currentUserId && d.CompanyID == companyId);
                
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
                //Upload Signature
                using (DBModel db = new DBModel())
                {
                    var currentUserId = User.Identity.GetUserId();
                    var userClientId = db.DesignatedUsers.First(c => c.UserAccountID == currentUserId && c.CompanyID == model.CompanyID);
                    var _action = "ApproveNomination";

                    try
                    {
                        //1. Log approval
                        var LogApproval = db.DesignatedUserApprovals.Create();
                        LogApproval.ApplicationID = model.ApplicationID;
                        LogApproval.UserID = userClientId.Id;
                        LogApproval.AcceptedTerms = model.terms;
                        LogApproval.DateApproved = DateTime.Now;
                        db.DesignatedUserApprovals.Add(LogApproval);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Json("Error! Unable to update representatives details", JsonRequestBehavior.AllowGet);
                    }

                    //2. Upload Signature
                    if (inputFile != null)
                    {
                        string pic = DateTime.Now.ToString("yyyyMMdd") + currentUserId + System.IO.Path.GetFileName(inputFile.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), pic);
                        // Upload file
                        inputFile.SaveAs(path);
                        try
                        {
                            //Save File name to Database  //Update Representative Details
                            var DesignatedUserToUpdate = db.DesignatedUsers.FirstOrDefault(c => c.UserAccountID == currentUserId);
                            DesignatedUserToUpdate.Signature = pic;
                            DesignatedUserToUpdate.Mobile = model.VerifyPhone;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to update signature details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to upload representative signature", JsonRequestBehavior.AllowGet);
                    }

                    //3. Update application Id
                    var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID && c.CompanyID == model.CompanyID);
                    var Approvals = ApplicationUpdate.UsersApproved;
                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            ApplicationUpdate.UsersApproved = Approvals + 1;
                            ApplicationUpdate.UsersDateApproved = DateTime.Now;
                            db.SaveChanges();
                            
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to update application details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Unable to Update Details!", JsonRequestBehavior.AllowGet);
                    }

                    //4. Check if all Representatives have approved and send complete email to digital desk
                    var ApplicationToCheck = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID && c.CompanyID == model.CompanyID);
                    if (ApplicationToCheck.UsersApproved == ApplicationUpdate.DesignatedUsers)
                    {
                        //Send Email to Digital Desk
                        var DDUserRole = (from p in db.AspNetUserRoles
                                          join e in db.AspNetUsers on p.UserId equals e.Id
                                          where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc"
                                          select new
                                          {
                                              EmailID = e.Email
                                          }).ToList();
                        var ClientInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == ApplicationUpdate.ClientID);
                        foreach (var email in DDUserRole)
                        {
                            var DDMessageBody = "Dear Team <br/><br/> Kindly note that the all signatories and representatives of the following client have all approved their nomination. <br/>" +
                                          "Company Name: " + ClientInfo.Surname + ", Company Email: " + ClientInfo.EmailAddress + ", Company PhoneNumber: " + ClientInfo.PhoneNumber + " " +
                                          "<br/><br/> Kind Regards, <br /><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                            var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Signatories/Representatives Completed Approval", DDMessageBody);
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

                    //5. Send email to authorized representative
                    var ApprovalCompleteEmailMessage = "Dear " + userClientId.Surname + ", <br/><br/> Thank you for accepting nomination as an authorized representative from " + model.CompanyName + ".<br/>" +
                        "You have also accepted our terms and conditions for trading on the portal. <br/><br/>" +
                         "Thank you for your continued custom." +
                        "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                    var ApprovalCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, userClientId.Email, "Authorised Representative Approval", ApprovalCompleteEmailMessage);
                    if (ApprovalCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, userClientId.Email, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, userClientId.Email, _action);
                    }

                    //6. Send email to company
                    var ApprovalCompleteEmailMessage2 = "Dear " + model.CompanySurname + ", <br/><br/> " + userClientId.Surname + " " + userClientId.Othernames + " has confirmed the information submitted and completed the process.<br/> Thank you for your continued custom." +
                    "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                    var ApprovalCompleteEmail2 = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Authorised Representative Approval", ApprovalCompleteEmailMessage2);
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
                    var _action = "DeclineNomination";
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
                    db.SaveChanges();

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
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        return Json("Unable to Update Application Status Details!", JsonRequestBehavior.AllowGet);
                    }

                    //Send email to Authorized User
                    var ApprovalCompleteEmailMessage = "Dear " + userClientId.Surname + ", <br/><br/> Thank you for declining nomination as an authorized user from " + model.CompanyName + ".<br/>" +
                         "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                    var ApprovalCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, userClientId.Email, "Authorized User Decline", ApprovalCompleteEmailMessage);
                    if (ApprovalCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, userClientId.Email, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApprovalCompleteEmailMessage, userClientId.Email, _action);
                    }

                    //Send email to client company
                    var ApprovalCompleteEmailMessage2 = "Dear " + model.CompanyName + ", <br/><br/> " + userClientId.Surname + " " + userClientId.Othernames + " has declined your nomination as  an authorized representative.<br/><br/>" +
                        "Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                    var ApprovalCompleteEmail2 = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Authorized User Decline", ApprovalCompleteEmailMessage2);
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
using Microsoft.AspNet.Identity;
using OnBoarding.Models;
using OnBoarding.Services;
using OnBoarding.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace OnBoarding.Controllers
{
    //
    //GET //Index
    [Authorize]
    public class OpsController : Controller
    {
        // GET: Ops //Index
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var TotalApprovals = db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == true || a.OPSDeclined == true)).Count();
                var TotalApprovedAndCompleted = db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == true || a.POAApproved == true)).Count();
                var PendingApprovals = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == false && a.OPSDeclined == false));
                ViewData["Approvals"] = TotalApprovals;
                ViewData["PendingApprovals"] = PendingApprovals;
                ViewData["TotalApprovedAndCompleted"] = TotalApprovedAndCompleted;
            }

            return View();
        }

        //
        //Get Approvals list
        public ActionResult ViewAll()
        {
            return View();
        }

        //
        //GET //Count
        public int GetPendingApprovalsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.OPSApproved == false && a.Status == 1).Count();
            }
        }

        //
        //GET /Get Pending List
        public List<ClientApplicationsViewModel> GetPendingApprovalsList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 ORDER BY " + jtSorting  + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName,  c.StatusName, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }

                else
                {
                    query = query.OrderByDescending(p => p.ApplicationID);
                }

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("DateCreated ASC"))
                {
                    query = query.OrderBy(p => p.DateCreated);
                }
                else if (jtSorting.Equals("DateCreated DESC"))
                {
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                else
                {
                    query = query.OrderByDescending(p => p.ApplicationID); //Default!  
                }

                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetPendingApprovals(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetPendingApprovalsList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetPendingApprovalsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        // POST: /Admin/ViewApproval
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewApproval(int applicationId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId && c.Status == 1);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                var companyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == getApplicationInfo.CompanyID);
                ViewBag.ApplicationInfo = clientDetails;
                ViewBag.CompanyInfo = companyDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID && a.CompanyID == getApplicationInfo.CompanyID).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID && a.CompanyID == getApplicationInfo.CompanyID).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + getApplicationInfo.ClientID + "'" + " AND s.CompanyID =  " + "'" + getApplicationInfo.CompanyID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;
                
                //Can approve application
                var OPSHasApproved = getApplicationInfo.OPSApproved;
                var OPSHasDeclined = getApplicationInfo.OPSDeclined;
                if (OPSHasApproved == true || OPSHasDeclined == true)
                {
                    ViewData["OPSCanApprove"] = 0;
                }
                else
                {
                    ViewData["OPSCanApprove"] = 1;
                }
            }

            return PartialView();
        }

        //
        // POST: Approve Application
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ApproveApplication(AdminApproveViewModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //1. Update application Details
                    var currentUserId = User.Identity.GetUserId();
                    var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID);
                    var ClientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == ApplicationUpdate.ClientID);
                    var CompanyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == ApplicationUpdate.CompanyID);
                    var _action = "ApproveApplication";

                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            //2. Update application status
                            ApplicationUpdate.OPSApproved = true;
                            ApplicationUpdate.OPSDateApproved = DateTime.Now;
                            ApplicationUpdate.OPSWhoApproved = currentUserId;
                            ApplicationUpdate.OPSApprovalStatus = 1;
                            ApplicationUpdate.OPSComments = model.Comments;
                            var savedApplicationDetails = db.SaveChanges();
                            if (savedApplicationDetails > 0)
                            {
                                //3. Send email notification to Poa for approval
                                var DDUserRole = (from p in db.AspNetUserRoles
                                                  join e in db.AspNetUsers on p.UserId equals e.Id
                                                  where p.RoleId == "1f477b75-8a56-4662-b4d1-48551bed6111"
                                                  select new
                                                  {
                                                      EmailID = e.Email
                                                  }).ToList();
                                foreach (var email in DDUserRole)
                                {
                                    var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client's application has been approved by the Operations Team. <br/>" +
                                                  "Company Name: " + CompanyDetails.CompanyName + ", Company Email: " + CompanyDetails.BusinessEmailAddress + " " +
                                                  "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                                    var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Ops Approved Application", DDMessageBody);
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
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        return Json("Unable to approve application!", JsonRequestBehavior.AllowGet);
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
        // DeclineApplication
        [HttpPost]
        [AllowAnonymous]
        public JsonResult DeclineApplication(DeclineApplicationViewModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //Update application Details
                    var currentUserId = User.Identity.GetUserId();
                    var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationId);
                    var ClientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == ApplicationUpdate.ClientID);
                    var CompanyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == ApplicationUpdate.CompanyID);
                    var _action = "DeclineApplication";
                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            //1. Update application status
                            ApplicationUpdate.OPSDeclined = true;
                            ApplicationUpdate.OPSDateApproved = DateTime.Now;
                            ApplicationUpdate.OPSWhoDeclined = currentUserId;
                            ApplicationUpdate.OPSApprovalStatus = 4;
                            ApplicationUpdate.Status = 4;
                            ApplicationUpdate.OPSComments = model.Comments;
                            var deleteApplication = db.SaveChanges();

                            //2. Mark HasApplication False for Client Company
                            var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == ApplicationUpdate.CompanyID);
                            updateClientCompany.HasApplication = false;
                            db.SaveChanges();

                            //3. Delete Signatories
                            db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(c => c.ClientID == ClientDetails.Id && c.CompanyID == ApplicationUpdate.CompanyID));
                            var deleteSignatories = db.SaveChanges();

                            //4. Delete Representatives
                            db.DesignatedUsers.RemoveRange(db.DesignatedUsers.Where(c => c.ClientID == ClientDetails.Id && c.CompanyID == ApplicationUpdate.CompanyID));
                            var deleteUsers = db.SaveChanges();

                            //5. Delete Settlement Accounts
                            db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(c => c.ClientID == ClientDetails.Id && c.CompanyID == ApplicationUpdate.CompanyID));
                            db.SaveChanges();

                            if (deleteApplication > 0 && deleteUsers > 0 && deleteSignatories > 0)
                            {
                                //6. Send email notification to Client Company
                                string EmailBody = string.Empty;
                                using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ApplicationDeclined.html")))
                                {
                                    EmailBody = reader.ReadToEnd();
                                }
                                EmailBody = EmailBody.Replace("{CompanyName}", CompanyDetails.CompanyName);

                                var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, ClientDetails.EmailAddress, "Application Declined", EmailBody);

                                if (SendRegistrationCompleteEmail == true)
                                {
                                    //Log email sent notification
                                    LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, ClientDetails.EmailAddress, _action);
                                }
                                else
                                {
                                    //Log Email failed notification
                                    LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, ClientDetails.EmailAddress, _action);
                                }

                                //7. Send email notification to Digital Desk
                                var DDUserRole = (from p in db.AspNetUserRoles
                                                  join e in db.AspNetUsers on p.UserId equals e.Id
                                                  where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc"
                                                  select new
                                                  {
                                                      EmailID = e.Email
                                                  }).ToList();
                                foreach (var email in DDUserRole)
                                {
                                    var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client's application has been declined by the Operations Team. <br/>" +
                                                  "Company Name: " + ClientDetails.Surname + ", Company Email: " + ClientDetails.EmailAddress + ", Company PhoneNumber: " + ClientDetails.PhoneNumber + " " +
                                                  "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                                    var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Declined Application - Client: " + ClientDetails.Surname + " ", DDMessageBody);
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
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("Unable to update application details", JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        return Json("No application details to update", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json("Model Invalid! " + errors + " ", JsonRequestBehavior.AllowGet);
            }
        }

        //
        //Get Approvals list
        public ActionResult OpsApprovals()
        {
            return View();
        }

        //
        //GET //Count
        public int GetOpsApprovalsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == true || a.OPSDeclined == true)).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetOpsApprovalsList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.OPSDeclined, s.POADeclined, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 1 OR s.OPSDeclined = 1) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.OPSDeclined, s.POAApproved, s.POADeclined, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 1 OR s.OPSDeclined = 1) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }

                else
                {
                    query = query.OrderByDescending(p => p.ApplicationID);
                }

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("DateCreated ASC"))
                {
                    query = query.OrderBy(p => p.DateCreated);
                }
                else if (jtSorting.Equals("DateCreated DESC"))
                {
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else if (jtSorting.Equals("OPSApproved ASC"))
                {
                    query = query.OrderBy(p => p.OPSApproved);
                }
                else if (jtSorting.Equals("OPSApproved DESC"))
                {
                    query = query.OrderByDescending(p => p.OPSApproved);
                }
                else if (jtSorting.Equals("POAApproved ASC"))
                {
                    query = query.OrderBy(p => p.POAApproved);
                }
                else if (jtSorting.Equals("POAApproved DESC"))
                {
                    query = query.OrderByDescending(p => p.POAApproved);
                }
                else
                {
                    query = query.OrderByDescending(p => p.ApplicationID); //Default!  
                }

                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetOpsApprovals(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetOpsApprovalsList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 1 OR s.OPSDeclined = 1) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetOpsApprovalsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }
    }
}
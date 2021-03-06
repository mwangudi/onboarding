﻿using Microsoft.AspNet.Identity;
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
    [Authorize]
    public class PoaController : Controller
    {
        // GET: Ops //Index
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var PendingApprovals = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.OPSApproved == true && a.POAApproved == false && a.Status == 1);
                var TotalApprovals = db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.POAApproved == true || a.POADeclined == true)).Count();
                var TotalDeclined = db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSDeclined == true || a.POADeclined == true)).Count();
                var TotalApprovedAndCompleted = db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == true || a.POAApproved == true)).Count();
                ViewData["Approvals"] = TotalApprovals;
                ViewData["Declined"] = TotalDeclined;
                ViewData["PendingApprovals"] = PendingApprovals;
                ViewData["TotalApprovedAndCompleted"] = TotalApprovedAndCompleted;

                // Bar Graph Statistics
                var janStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 1 && e.DateCreated.Year == DateTime.Now.Year);
                var febStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 2 && e.DateCreated.Year == DateTime.Now.Year);
                var marStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 3 && e.DateCreated.Year == DateTime.Now.Year);
                var aprStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 4 && e.DateCreated.Year == DateTime.Now.Year);
                var mayStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 5 && e.DateCreated.Year == DateTime.Now.Year);
                var junStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 6 && e.DateCreated.Year == DateTime.Now.Year);
                var julStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 7 && e.DateCreated.Year == DateTime.Now.Year);
                var augStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 8 && e.DateCreated.Year == DateTime.Now.Year);
                var sepStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 9 && e.DateCreated.Year == DateTime.Now.Year);
                var octStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 10 && e.DateCreated.Year == DateTime.Now.Year);
                var novStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 11 && e.DateCreated.Year == DateTime.Now.Year);
                var decStats = db.EMarketApplications.Count(e => e.Status == 1 && e.OPSApproved == true && e.POAApproved == true && e.DateCreated.Month == 12 && e.DateCreated.Year == DateTime.Now.Year);

                //Chart ViewData
                ViewData["Chart1Data"] = "[" + janStats + ", " + febStats + ", " + marStats + ", " + aprStats + ", " + mayStats + ", " + junStats + ", " + julStats + ", " + augStats + ", " + sepStats + ", " + octStats + ", " + novStats + ", " + decStats + "]";

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
        public int GetCompletedApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.OPSApproved == true && a.POAApproved == false && a.Status == 1).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetCompletedApplicationsList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.POAApproved = 0 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName,  c.StatusName, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.POAApproved = 0 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

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
        public JsonResult GetCompletedApplications(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetCompletedApplicationsList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.POAApproved = 0 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetCompletedApplicationsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        //Get Approvals list
        public ActionResult ApprovedApplications()
        {
            return View();
        }

        //
        //GET //Count
        public int GetApprovedApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.OPSApproved == true && a.POAApproved == true && a.Status == 1).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetApprovedApplicationsList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

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
        public JsonResult GetApprovedApplications(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetApprovedApplicationsList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetCompletedApplicationsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        // POST: /POA/ViewApproval
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewApproval(int applicationId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId);
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
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.Status = 1 AND s.ClientID =  " + "'" + clientID + "'" + " AND s.CompanyID =  " + "'" + getApplicationInfo.CompanyID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["OpsComments"] = getApplicationInfo.OPSComments;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;

                //Get the person who approved
                var OpsApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoApproved);
                var OpsDeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoDeclined);
                ViewData["OPSNames"] = OpsApproved.CompanyName;
                ViewData["OPSEmail"] = OpsApproved.Email;
                ViewData["OPSPhone"] = OpsApproved.PhoneNumber;
                DateTime dtByUser = DateTime.Parse(getApplicationInfo.OPSDateApproved.ToString());
                ViewData["OPSDateApproved"] = dtByUser.ToString("dd/MM/yyyy hh:mm:ss tt");

                //Can approve application
                var POAHasApproved = getApplicationInfo.POAApproved;
                var POAHasDeclined = getApplicationInfo.POADeclined;
                if (POAHasApproved == true || POAHasDeclined == true)
                {
                    ViewData["POACanApprove"] = 0;
                }
                else
                {
                    ViewData["POACanApprove"] = 1;
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
                    //Update application Details
                    var currentUserId = User.Identity.GetUserId();
                    var ApplicationUpdate = db.EMarketApplications.SingleOrDefault(c => c.Id == model.ApplicationID);
                    var _action = "ApproveApplication";
                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            ApplicationUpdate.POAApproved = true;
                            ApplicationUpdate.POADateApproved = DateTime.Now;
                            ApplicationUpdate.POAWhoApproved = currentUserId;
                            ApplicationUpdate.POAApprovalStatus = 1;
                            ApplicationUpdate.POAComments = model.Comments;
                            db.SaveChanges();

                            //Send Email to Client
                            string EmailBody = string.Empty;
                            using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ApplicationApproved.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{CompanyName}", model.CompanyName);

                            var SendRegistrationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.CompanyEmail, "Application Approved", EmailBody);

                            if (SendRegistrationCompleteEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, model.CompanyEmail, _action);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, model.CompanyEmail, _action);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to approve application!", JsonRequestBehavior.AllowGet);
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
                                                  where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc" && e.Status == 1
                                                  select new
                                                  {
                                                      EmailID = e.Email
                                                  }).ToList();
                                foreach (var email in DDUserRole)
                                {
                                    var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client's application has been declined by POA. <br/>" +
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
        public ActionResult PoaApprovals()
        {
            return View();
        }

        //
        //GET //Count
        public int GetPoaApprovalsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.POAApproved == true || a.POADeclined == true)).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetPoaApprovalsList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.OPSDeclined, s.POADeclined, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.POAApproved = 1 OR s.POADeclined = 1) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.OPSDeclined, s.POAApproved, s.POADeclined, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.POAApproved = 1 OR s.POADeclined = 1) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
        public JsonResult GetPoaApprovals(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetPoaApprovalsList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.POAApproved = 1 OR s.POADeclined = 1) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetPoaApprovalsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }
    }
}
using Microsoft.AspNet.Identity;
using OnBoarding.Models;
using OnBoarding.Services;
using OnBoarding.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnBoarding.Controllers
{
    public class DigitalDeskController : Controller
    {
        //
        // GET: Index
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var RegisteredClients = db.RegisteredClients.Count(a => a.Status == 1);
                var CompletedApplications = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved);
                var InCompleteApplications = db.EMarketApplications.Count(a => (a.Signatories != a.SignatoriesApproved || a.DesignatedUsers != a.UsersApproved) && (a.Status == 1 && a.OPSApproved == false && a.POAApproved == false));
                var Approvals = db.EMarketApplications.Count(a => (a.OPSApproved == true && a.POAApproved == true));
                var PendingOpsApprovals = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == false && a.OPSDeclined == false));
                var PendingPOAApprovals = db.EMarketApplications.Count(a => (a.OPSApproved == true && a.Status == 1 && (a.POAApproved == false && a.POADeclined == false)));
                var PendingApprovals = PendingOpsApprovals + PendingPOAApprovals;
                var DeclinedApplications = db.EMarketApplications.Count(a => a.Status == 4);

                ViewData["RegisteredClients"] = RegisteredClients;
                ViewData["CompletedApplications"] = CompletedApplications;
                ViewData["Approvals"] = Approvals;
                ViewData["PendingApprovals"] = PendingApprovals;
                ViewData["PendingOpsApprovals"] = PendingOpsApprovals;
                ViewData["PendingPOAApprovals"] = PendingPOAApprovals;
                ViewData["InCompleteApplications"] = InCompleteApplications;
                ViewData["DeclinedApplications"] = DeclinedApplications;

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
                ViewData["Chart2Data"] = "[" + Approvals + ", " + PendingApprovals + ", " + InCompleteApplications + ", " + DeclinedApplications + "]";

                //Pending Applications
                var ApprovedApplications = (from a in db.EMarketApplications
                                            join b in db.RegisteredClients on a.ClientID equals b.Id
                                            where a.OPSApproved == true && a.POAApproved == true
                                            select new
                                            {
                                                ApplicationId = a.Id,
                                                //Company = b.CompanyName,
                                                OpsDateApproved = a.OPSDateApproved,
                                                PoaDateApproved = a.POADateApproved
                                            }).OrderByDescending(x => x.ApplicationId).ToList();
                ViewBag.ApprovedApplications = ApprovedApplications;

                //Get the list of all messages
                var userId = User.Identity.GetUserId();
                var UserEmail = db.AspNetUsers.SingleOrDefault(a => a.Id == userId);
                var Query = db.Database.SqlQuery<Notification>("SELECT TOP 1 * FROM Notifications WHERE [To] = '" + UserEmail.Email + "' ORDER BY Id DESC");
                ViewBag.SystemNotifications = Query.ToList();

            }

            return View();
        }

        //
        //GET //Manage System Notifications
        public ActionResult Notifications()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetNotificationsCount()
        {
            using (var db = new DBModel())
            {
                return db.Notifications.Count();
            }
        }

        //
        //GET //Get Notifications List
        public List<NotificationsViewModel> GetSystemNotificationsList(string searchMessage, string searchDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<NotificationsViewModel> query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated, n.Action FROM Notifications n ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated, n.Action FROM Notifications n WHERE n.[To] LIKE '%" + searchMessage + "%' ORDER BY n.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (!string.IsNullOrEmpty(searchDate) && string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated, n.Action FROM Notifications n WHERE CAST(n.DateCreated AS date) = " + "'" + searchDate + "'" + " ORDER BY n.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchDate))
                {
                    query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated, n.Action FROM Notifications n WHERE CAST(n.DateCreated AS date) = " + "'" + searchDate + "'" + " AND n.[To] LIKE '%" + searchMessage + "%' ORDER BY n.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
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
                else if (jtSorting.Equals("To ASC"))
                {
                    query = query.OrderBy(p => p.To);
                }
                else if (jtSorting.Equals("To DESC"))
                {
                    query = query.OrderByDescending(p => p.To);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id); //Default!  
                }

                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetSystemNotifications(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetSystemNotificationsList(searchMessage, searchDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) count FROM Notifications n WHERE n.[To] LIKE '%" + searchMessage + "%';").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchDate) && string.IsNullOrEmpty(searchMessage))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) FROM Notifications n WHERE CAST(n.DateCreated AS date) = " + "'" + searchDate + "'" + ";").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT  COUNT(n.Id) FROM Notifications n WHERE CAST(n.DateCreated AS date) = " + "'" + searchDate + "'" + " AND n.[To] LIKE '%" + searchMessage + "%';").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetNotificationsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        //POST //LoadResendNotification
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResendNotification(int getNotificationId)
        {
            using (DBModel db = new DBModel())
            {
                var getMessageInfo = db.Notifications.SingleOrDefault(c => c.Id == getNotificationId);

                //Data For View Display
                ViewData["EmailTo"] = getMessageInfo.To;
                ViewData["MessageId"] = getMessageInfo.Id;
                ViewData["MessageBody"] = getMessageInfo.MessageBody;
            }

            return PartialView();
        }

        //POST //Resend Message
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ResendNotification(ResendMessageViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var currentUserId = User.Identity.GetUserId();

                    //ReSend Email
                    var EmailMessage = (model.ResendMessage).Trim();
                    var Action = "ResendNotification";
                    var EmailResent = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.ResendEmail, "Resent Email", EmailMessage);

                    if (EmailResent == true)
                    {
                        //Log email sent notification
                        int lastInsertId = db.Notifications.Max(p => p.Id);
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailMessage, model.ResendEmail, Action);
                        var LogAuditTrail = Functions.LogAuditTrail(lastInsertId, "Resend Email", "Notifications", null, currentUserId, model.ResendEmail, null, null);
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailMessage, model.ResendEmail, Action);
                        return Json("Error! Unable to resend your email.", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        // POST: Admin RegisteredClients
        public ActionResult RegisteredClients()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetRegisteredClientsCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.RegisteredClients.Where(e => e.Status == 1).Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<RegisteredClient> GetRegisteredClientsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<RegisteredClient> query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients r LEFT JOIN ClientCompanies c ON c.ClientId = r.Id WHERE r.Status < 2;");

                //Search
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n LEFT JOIN ClientCompanies c ON c.ClientId = n.Id WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%' OR c.CompanyName LIKE '%" + searchMessage + "%') AND n.Status = 1 AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE));");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n LEFT JOIN ClientCompanies c ON c.ClientId = n.Id WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%' OR c.CompanyName LIKE '%" + searchMessage + "%') AND n.Status = 1;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n LEFT JOIN ClientCompanies c ON c.ClientId = n.Id WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%' OR c.CompanyName LIKE '%" + searchMessage + "%') AND n.Status = 1 AND n.DateCreated >= CAST('" + searchFromDate + "' AS DATE);");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n LEFT JOIN ClientCompanies c ON c.ClientId = n.Id WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%' OR c.CompanyName LIKE '%" + searchMessage + "%') AND n.Status = 1 AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE);");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n LEFT JOIN ClientCompanies c ON c.ClientId = n.Id WHERE n.Status = 1 AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE));");
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
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
                else if (jtSorting.Equals("EmailAddress ASC"))
                {
                    query = query.OrderBy(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("EmailAddress DESC"))
                {
                    query = query.OrderByDescending(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else if (jtSorting.Equals("AcceptedTAC ASC"))
                {
                    query = query.OrderBy(p => p.AcceptedTerms);
                }
                else if (jtSorting.Equals("AcceptedTAC DESC"))
                {
                    query = query.OrderByDescending(p => p.AcceptedTerms);
                }
                else
                {
                    //Default!
                    query = query.OrderByDescending(p => p.Id);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging  
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetRegisteredClients(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetRegisteredClientsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetRegisteredClientsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //
        //GET //ClientCompanyList For Child Items
        [HttpPost]
        public JsonResult ClientCompanyList(int jtStartIndex, int jtPageSize, string jtSorting, int ClientId)
        {
            try
            {
                //Get data from database
                using (var db = new DBModel())
                {
                    int companiesCount = db.ClientCompanies.Where(a => a.ClientId == ClientId).Count();
                    var Query = db.Database.SqlQuery<ClientCompany>("SELECT * FROM ClientCompanies s WHERE s.ClientId =  " + "'" + ClientId + "'" + " AND s.Status = 1 ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                    var companies = Query.ToList();

                    //Return result to jTable
                    return Json(new { Result = "OK", Records = companies, TotalRecordCount = companiesCount });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        //
        // POST: //ViewClient
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewClient(int clientID)
        {
            using (DBModel db = new DBModel())
            {
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                ViewBag.RegisteredClientInfo = clientDetails;

                var companyDetails = db.ClientCompanies.SingleOrDefault(s => s.ClientId == clientID);
                ViewBag.CompanyInfo = companyDetails;

                //Signatories List
                /*List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID && a.CompanyID == companyDetails.Id).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID && a.CompanyID == companyDetails.Id).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;*/

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber, s.OtherCurrency FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientID + "'" + " AND s.CompanyID =  " + "'" + companyDetails.Id + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                var clientHasApplication = db.EMarketApplications.Any(s => s.ClientID == clientID);
                ViewBag.clientHasApplication = clientHasApplication;
            }

            return PartialView();
        }

        //
        //POST: //Manage Currencies
        public ActionResult ManageCurrencies()
        {
            return View();
        }

        //
        //Post //AddNewCurrency
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewCurrency(PostCurrencyViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    var currentUserId = User.Identity.GetUserId();

                    //Create New Currency
                    var newCurrency = db.Currencies.Create();
                    newCurrency.CurrencyName = model.CurrencyName;
                    newCurrency.CurrencyShort = model.CurrencyShortName;
                    newCurrency.Status = 1;
                    newCurrency.CreatedBy = User.Identity.GetUserId();
                    db.Currencies.Add(newCurrency);
                    var recordSaved = db.SaveChanges();
                    if(recordSaved > 0)
                    {
                        int lastInsertId = db.Currencies.Max(p => p.Id);
                        var LogAuditTrail = Functions.LogAuditTrail(lastInsertId, "Add", "Currencies", null, currentUserId, model.CurrencyName + " " + model.CurrencyShortName, null, null);
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! Unable to add currency.", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json("Error! Unable to add currency.", JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /Admin/EditCurrency
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadEditCurrency(int getCurrencyId)
        {
            using (DBModel db = new DBModel())
            {
                var getCurrencyInfo = db.Currencies.SingleOrDefault(c => c.Id == getCurrencyId);

                //Data For View Display
                ViewData["CurrencyId"] = getCurrencyInfo.Id;
                ViewData["CurrencyName"] = getCurrencyInfo.CurrencyName;
                ViewData["CurrencyShortName"] = getCurrencyInfo.CurrencyShort;
                ViewData["Status"] = getCurrencyInfo.Status;
            }

            return PartialView();
        }

        //
        //EditSystemCurrency
        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSystemCurrency(EditCurrencyViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var currentUserId = User.Identity.GetUserId();
                    var getCurrencyUpdate = db.Currencies.SingleOrDefault(c => c.Id == model.EditId);
                    getCurrencyUpdate.CurrencyName = model.EditCurrencyName;
                    getCurrencyUpdate.Status = model.EditTModeStatus;
                    getCurrencyUpdate.CurrencyShort = model.EditCurrencyShortName;
                    getCurrencyUpdate.Status = model.EditStatus;
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        int lastInsertId = db.Currencies.Max(p => p.Id);
                        var LogAuditTrail = Functions.LogAuditTrail(model.EditId, "Edit", "Currencies", null, currentUserId, model.EditCurrencyName + " " + model.EditCurrencyShortName, null, null);
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! Unable to add currency.", JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    return Json("" + ex.Message + "", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //GET /Get Notifications Count
        public int GetCurrenciesCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.Currencies.Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<Currency> GetCurrencyList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<Currency> query = db.Database.SqlQuery<Currency>("SELECT * FROM Currencies n");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<Currency>("SELECT * FROM Currencies n WHERE (n.CurrencyShort LIKE '%" + searchMessage + "%' OR n.CurrencyName LIKE '%" + searchMessage + "%') AND n.Status = 1;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("CurrencyShort ASC"))
                {
                    query = query.OrderBy(p => p.CurrencyShort);
                }
                else if (jtSorting.Equals("CurrencyShort DESC"))
                {
                    query = query.OrderByDescending(p => p.CurrencyShort);
                }
                else
                {
                    //Default!
                    query = query.OrderByDescending(p => p.Id);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging  
            }
        }

        //
        //GET //Gets Currencies  
        [HttpPost]
        public JsonResult GetSystemCurrencies(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetCurrencyList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM Currencies n WHERE (n.CurrencyShort LIKE '%" + searchMessage + "%' OR n.CurrencyName LIKE '%" + searchMessage + "%') AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetCurrenciesCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //
        //POST //Client Signatories
        public ActionResult ClientSignatories()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetClientSignatoriesCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.ClientSignatories.Where(a => a.Status == 1).Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<ClientSignatoriesViewModel> GetClientSignatoriesList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<ClientSignatoriesViewModel> query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ',a.OtherNames) AS Names, a.EmailAddress AS Email, a.PhoneNumber, a.AcceptedTerms, s.StatusName Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM ClientSignatories a INNER JOIN ClientCompanies r ON r.Id = a.CompanyID INNER JOIN tblStatus s ON s.Id = a.Status");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ',a.OtherNames) AS Names, a.EmailAddress AS Email, a.PhoneNumber, a.AcceptedTerms, s.StatusName Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM ClientSignatories a INNER JOIN ClientCompanies r ON r.Id = a.CompanyID INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.OtherNames LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%');");
                }

                else
                {
                    query = query.OrderByDescending(p => p.SignatoryId);
                }

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("Names ASC"))
                {
                    query = query.OrderBy(p => p.Names);
                }
                else if (jtSorting.Equals("Names DESC"))
                {
                    query = query.OrderByDescending(p => p.Names);
                }
                else if (jtSorting.Equals("DateCreated ASC"))
                {
                    query = query.OrderBy(p => p.DateCreated);
                }
                else if (jtSorting.Equals("DateCreated DESC"))
                {
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                else if (jtSorting.Equals("ClientName ASC"))
                {
                    query = query.OrderBy(p => p.ClientName);
                }
                else if (jtSorting.Equals("ClientName DESC"))
                {
                    query = query.OrderByDescending(p => p.ClientName);
                }
                else if (jtSorting.Equals("Email ASC"))
                {
                    query = query.OrderBy(p => p.Email);
                }
                else if (jtSorting.Equals("Email DESC"))
                {
                    query = query.OrderByDescending(p => p.Email);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else if (jtSorting.Equals("AcceptedTAC ASC"))
                {
                    query = query.OrderBy(p => p.AcceptedTAC);
                }
                else if (jtSorting.Equals("AcceptedTAC DESC"))
                {
                    query = query.OrderByDescending(p => p.AcceptedTAC);
                }
                else
                {
                    //Default!
                    query = query.OrderByDescending(p => p.SignatoryId);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging  
            }
        }

        //
        //GET //Gets Signatories  
        [HttpPost]
        public JsonResult GetClientSignatories(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {

            using (var db = new DBModel())
            {
                var data = GetClientSignatoriesList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(a.Id) AS COUNT FROM ClientSignatories a INNER JOIN RegisteredClients r ON r.Id = a.ClientID INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.OtherNames LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%');").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetClientSignatoriesCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //
        //Delete ClientSignatories
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteClient(int clientId)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var UserEmail = db.AspNetUsers.SingleOrDefault(a => a.Id == userId);
                    var getClientInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == clientId);

                    //LogAuditTrail
                    var LogAuditTrail = Functions.LogAuditTrail(clientId, "Delete", "RegisteredClients", null, UserEmail.Email, getClientInfo.Surname + " " + getClientInfo.OtherNames, getClientInfo.EmailAddress, getClientInfo.PhoneNumber);

                    if (LogAuditTrail)
                    {
                        //Delete RegisteredClient/Company
                        db.ClientCompanies.RemoveRange(db.ClientCompanies.Where(r => r.ClientId == clientId));
                        var deletedCompany = db.SaveChanges();
                        if (deletedCompany > 0)
                        {
                            db.RegisteredClients.RemoveRange(db.RegisteredClients.Where(r => r.Id == clientId));
                            var deletedClient = db.SaveChanges();
                            if (deletedClient > 0)
                            {
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("Error! Unable to delete client details", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("Error! Unable to delete company details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to delete client", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json("Error! Unable to delete user details", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST LoadDeleteSignatory
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadDeleteSignatory(int getSignatoryId)
        {
            using (DBModel db = new DBModel())
            {
                var getClientInfo = db.ClientSignatories.SingleOrDefault(c => c.Id == getSignatoryId);

                //Data For View Display
                ViewData["FullNames"] = getClientInfo.Surname + " " + getClientInfo.OtherNames;
                ViewData["EmailAddress"] = getClientInfo.EmailAddress;
                ViewData["SignatoryId"] = getClientInfo.Id;
                ViewData["UserAccountId"] = getClientInfo.UserAccountID;
            }

            return PartialView();
        }

        //
        //Delete ClientSignatories
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteSignatory(DeleteSignatoryViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var UserEmail = db.AspNetUsers.SingleOrDefault(a => a.Id == userId);
                    var getSignatoryInfo = db.ClientSignatories.SingleOrDefault(c => c.Id == model.SignatoryId);

                    //Delete From Registered Client Table
                    var LogAuditTrail = Functions.LogAuditTrail(model.ClientId, "Delete", "RegisteredClients, AspNetUsers", model.UserAccountId, UserEmail.Email, getSignatoryInfo.Surname + " " + getSignatoryInfo.OtherNames, getSignatoryInfo.EmailAddress, getSignatoryInfo.PhoneNumber);

                    if (LogAuditTrail)
                    {
                        db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(r => r.Id == model.SignatoryId));
                        var deletedClient = db.SaveChanges();
                        if (deletedClient > 0)
                        {
                            //Delete User login Account
                            if (getSignatoryInfo.Status == 1)
                            {
                                db.AspNetUsers.RemoveRange(db.AspNetUsers.Where(r => r.Id == model.UserAccountId));
                                var deletedUser = db.SaveChanges();
                                if (deletedUser > 0)
                                {
                                    return Json("success", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json("Error! Unable to delete user account", JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("Error! Unable to delete user account", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to delete user account", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json("Error! Unable to delete user details", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST ViewSignatory
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewSignatory(int signatoryId)
        {
            using (DBModel db = new DBModel())
            {
                var SignatoryInfo = db.ClientSignatories.SingleOrDefault(s => s.Id == signatoryId);
                ViewBag.SignatoryInfo = SignatoryInfo;

                var RegisteredClientInfo = db.ClientCompanies.SingleOrDefault(s => s.Id == SignatoryInfo.CompanyID);
                ViewBag.RegisteredClientInfo = RegisteredClientInfo;
                
            }
            return PartialView();
        }

        //
        //POST //Client Signatories
        public ActionResult ClientRepresentatives()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetClientRepresentativesCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.DesignatedUsers.Where(a => a.Status == 1).Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<ClientSignatoriesViewModel> GetClientRepresentativesList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<ClientSignatoriesViewModel> query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ', a.OtherNames) AS Names, a.Email, a.Mobile AS PhoneNumber, a.AcceptedTerms, s.StatusName AS Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM DesignatedUsers a INNER JOIN ClientCompanies r ON r.Id = a.CompanyID INNER JOIN tblStatus s ON s.Id = a.Status");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ', a.OtherNames) AS Names, a.Email, a.Mobile AS PhoneNumber, a.AcceptedTerms, s.StatusName AS Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM DesignatedUsers a INNER JOIN ClientCompanies r ON r.Id = a.CompanyID INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.Othernames LIKE '%" + searchMessage + "%' OR a.Email LIKE '%" + searchMessage + "%');");
                }

                else
                {
                    query = query.OrderByDescending(p => p.SignatoryId);
                }

                //Sorting Ascending and Descending  
                //DateCreated AcceptedTAC Status Email ClientName Names 
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("Names ASC"))
                {
                    query = query.OrderBy(p => p.Names);
                }
                else if (jtSorting.Equals("Names DESC"))
                {
                    query = query.OrderByDescending(p => p.Names);
                }
                else if (jtSorting.Equals("ClientName ASC"))
                {
                    query = query.OrderBy(p => p.ClientName);
                }
                else if (jtSorting.Equals("ClientName DESC"))
                {
                    query = query.OrderByDescending(p => p.ClientName);
                }
                else if (jtSorting.Equals("DateCreated ASC"))
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
                else if (jtSorting.Equals("AcceptedTAC ASC"))
                {
                    query = query.OrderBy(p => p.AcceptedTAC);
                }
                else if (jtSorting.Equals("AcceptedTAC DESC"))
                {
                    query = query.OrderByDescending(p => p.AcceptedTAC);
                }
                else if (jtSorting.Equals("Email ASC"))
                {
                    query = query.OrderBy(p => p.Email);
                }
                else if (jtSorting.Equals("Email DESC"))
                {
                    query = query.OrderByDescending(p => p.Email);
                }
                else
                {
                    //Default!
                    query = query.OrderByDescending(p => p.SignatoryId);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging  
            }
        }

        //
        //GET //Gets Signatories  
        [HttpPost]
        public JsonResult GetClientRepresentatives(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetClientRepresentativesList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM DesignatedUsers n WHERE (n.Surname LIKE '%" + searchMessage + "%' OR n.Othernames LIKE '%" + searchMessage + "%' OR n.Email LIKE '%" + searchMessage + "%');").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetClientRepresentativesCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //POST ViewRepresentative
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewRepresentative(int representativeId)
        {
            using (DBModel db = new DBModel())
            {
                var representativesDetails = db.DesignatedUsers.SingleOrDefault(s => s.Id == representativeId);
                ViewBag.RepresentativeInfo = representativesDetails;

                var RegisteredClientInfo = db.ClientCompanies.SingleOrDefault(s => s.Id == representativesDetails.CompanyID);
                ViewBag.RegisteredClientInfo = RegisteredClientInfo;
            }
            return PartialView();
        }

        //
        //POST LoadDeleteSignatory
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadDeleteRepresentative(int getSignatoryId)
        {
            using (DBModel db = new DBModel())
            {
                var getClientInfo = db.DesignatedUsers.SingleOrDefault(c => c.Id == getSignatoryId);

                //Data For View Display
                ViewData["FullNames"] = getClientInfo.Surname + " " + getClientInfo.Othernames;
                ViewData["EmailAddress"] = getClientInfo.Email;
                ViewData["SignatoryId"] = getClientInfo.Id;
                ViewData["UserAccountId"] = getClientInfo.UserAccountID;
            }

            return PartialView();
        }

        //
        //Delete ClientSignatories
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteRepresentative(DeleteSignatoryViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                //Delete Client Signatories
                db.DesignatedUsers.RemoveRange(db.DesignatedUsers.Where(r => r.Id == model.SignatoryId));
                //var getSignatoryToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Id == model.SignatoryId);
                //getSignatoryToUpdate.Status = 2; //Mark Status as deleted (2)
                db.SaveChanges();

                //Delete User login Account
                db.AspNetUsers.RemoveRange(db.AspNetUsers.Where(r => r.Id == model.UserAccountId));
                //var getUserToDelete = db.AspNetUsers.SingleOrDefault(c => c.Id == model.UserAccountId);
                //getUserToDelete.Status = 2; //Mark Status as deleted (2)
                db.SaveChanges();
            }
            return RedirectToAction("ClientSignatories");
        }

        //
        //Get SignatoryApprovals
        public ActionResult ViewSignatoryApprovals()
        {
            return View(GetSignatoryApprovals());
        }
        //Revisit
        public IEnumerable<SignatoryApprovalsViewModel> GetSignatoryApprovals()
        {
            using (DBModel db = new DBModel())
            {
                //Query
                var Query = from a in db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved)
                            join b in db.RegisteredClients on a.ClientID equals b.Id
                            join c in db.tblStatus on a.Status equals c.Id
                            select new SignatoryApprovalsViewModel
                            {
                                ApplicationID = a.Id,
                                //Client = b.CompanyName,
                                //Signatory = b.CompanyName,
                                SignatoryEmail = b.EmailAddress,
                                Status = c.StatusName,
                                AcceptedTAC = a.AcceptedTAC,
                                DateCreated = a.DateCreated
                            };
                return Query.OrderByDescending(x => x.ApplicationID).ToList();
            }
        }

        //
        //Get Admin Client Uploads index
        public ActionResult ClientUploads()
        {
            return View();
        }

        // GET: //Digital Desk Upolad Clients
        [HttpPost]
        public JsonResult ClientUploads(RegisteredClient registeredClient, HttpPostedFileBase FileUpload)
        {
            List<string> data = new List<string>();
            string filePath = string.Empty;

            if (FileUpload == null)
            {
                return Json("Error! Please choose a CSV file for upload", JsonRequestBehavior.AllowGet);
            }

            if ((FileUpload.ContentType == "application/csv" || FileUpload.ContentType == "text/tsv" || FileUpload.ContentType == "application/vnd.ms-excel"))
            {
                string path = Server.MapPath("~/Content/clientuploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + DateTime.Now.ToString("yyyyMMdd") + User.Identity.GetUserId() + System.IO.Path.GetFileName(FileUpload.FileName);
                string extension = Path.GetExtension(FileUpload.FileName);
                FileUpload.SaveAs(filePath);

                //Create a DataTable.
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[3] {
                            new DataColumn("ClientEmailAddress".ToLower(), typeof(string)),
                            new DataColumn("ClientName", typeof(string)),
                            new DataColumn("ClientCompanyName", typeof(string))
                });
                dt.Columns.Add("UploadedBy").DefaultValue = User.Identity.GetUserId();

                //Read the contents of CSV file.
                string csvData = System.IO.File.ReadAllText(filePath);

                //Execute a loop over the rows.
                foreach (string row in csvData.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        dt.Rows.Add();
                        int i = 0;

                        //Execute a loop over the columns.
                        foreach (string cell in row.Split(','))
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell;
                            i++;
                        }
                    }
                }

                string consString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(consString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {

                        //Set the database table name.
                        sqlBulkCopy.DestinationTableName = "dbo.RegisteredClients";

                        //[OPTIONAL]: Map the DataTable columns with that of the database table
                        sqlBulkCopy.ColumnMappings.Add("ClientEmailAddress", "EmailAddress");
                        sqlBulkCopy.ColumnMappings.Add("ClientName", "AccountName");
                        sqlBulkCopy.ColumnMappings.Add("ClientCompanyName", "CompanyName");
                        sqlBulkCopy.ColumnMappings.Add("UploadedBy", "UploadedBy");

                        try
                        {
                            con.Open();
                            sqlBulkCopy.WriteToServer(dt);
                        }
                        catch (Exception)
                        {
                            return Json("Error! Uploading your CSV file. Please try again.", JsonRequestBehavior.AllowGet);
                        }
                        finally
                        {
                            con.Close();
                        }

                    }
                    //Return Result
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Error! Please choose a valid CSV file for upload", JsonRequestBehavior.AllowGet);
            }
        }

        //
        //Get All Uploaded Users from table
        public JsonResult SendOTPToUploadedUsers()
        {
            using (DBModel db = new DBModel())
            {
                //Query List
                var userid = User.Identity.GetUserId();
                var Query = from a in db.RegisteredClients.Where(r => r.UploadedBy == userid && DbFunctions.DiffMinutes(r.DateCreated, DateTime.Now) <= 1)
                            select new UploadedUsersViewModel
                            {
                                CompanyName = a.AccountName,
                                EmailAddress = a.EmailAddress.ToLower()
                            };

                foreach (var a in Query.ToList())
                {
                    var _activationCode = OTPGenerator.GetUniqueKey(6);
                    string activationCode = Shuffle.StringMixer(_activationCode);
                    var Action = "SendOTPToUploadedUsers";
                    var getClientToUpdate = db.RegisteredClients.SingleOrDefault(c => c.EmailAddress == a.EmailAddress.ToLower());
                    getClientToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                    getClientToUpdate.EmailAddress = a.EmailAddress.ToLower();
                    db.SaveChanges();

                    //Send Email with the OTP to uploaded emails
                    var callbackUrl = Url.Action("UploadedClientCompleteRegistration", "Account", null, Request.Url.Scheme);
                    string EmailBody = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ClientRegistrationEmail.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{CompanyName}", a.CompanyName);
                    EmailBody = EmailBody.Replace("{ActivationCode}", activationCode);
                    EmailBody = EmailBody.Replace("{Url}", callbackUrl);

                    var CompleteRegistrationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, a.EmailAddress.ToLower(), "Confirm Registration", EmailBody);
                    if (CompleteRegistrationEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, a.EmailAddress.ToLower(), Action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, a.EmailAddress.ToLower(), Action);
                    }
                }
            }
            return Json("OTP Sent to uploaded clients", JsonRequestBehavior.AllowGet);
        }

        //
        //Get All Uploaded Users from table
        private IEnumerable<SystemNotificationsViewModel> GetDigitalDeskNotifications()
        {
            using (DBModel db = new DBModel())
            {
                //Query List
                var userId = User.Identity.GetUserId();
                var UserEmail = db.AspNetUsers.SingleOrDefault(a => a.Id == userId);
                var Query = from a in db.Notifications.Where(r => r.To == UserEmail.Email)
                            select new SystemNotificationsViewModel
                            {
                                Type = a.Type,
                                From = a.From,
                                To = a.To.ToLower(),
                                //MessageBody = a.EmailAddress.ToLower(),
                                DateCreated = a.DateCreated
                            };
                return Query.OrderByDescending(x => x.DateCreated).ToList();
            }
        }

        //
        //Get Admin Client Uploads index
        public ActionResult UploadExistingClients()
        {
            return View();
        }

        // GET: Admin Upolad Clients
        [HttpPost]
        public JsonResult ExistingClientUploads(RegisteredClient registeredClient, HttpPostedFileBase FileUpload)
        {
            List<string> data = new List<string>();
            string filePath = string.Empty;

            if (FileUpload == null)
            {
                return Json("Error! Please choose a CSV file for upload", JsonRequestBehavior.AllowGet);
            }

            if ((FileUpload.ContentType == "application/csv" || FileUpload.ContentType == "text/tsv" || FileUpload.ContentType == "application/vnd.ms-excel"))
            {
                string path = Server.MapPath("~/Content/clientuploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + DateTime.Now.ToString("yyyyMMdd") + User.Identity.GetUserId() + System.IO.Path.GetFileName(FileUpload.FileName);
                string extension = Path.GetExtension(FileUpload.FileName);
                FileUpload.SaveAs(filePath);

                //Create a DataTable.
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[14] {
                            new DataColumn("ColCompanyName", typeof(string)),
                            new DataColumn("ColCompanyEmail".ToUpper(), typeof(string)),
                            new DataColumn("ColAcceptedTerms".ToUpper(), typeof(string)),
                            new DataColumn("ColEMTSignUp".ToUpper(), typeof(string)),
                            new DataColumn("ColSSI".ToUpper(), typeof(string)),
                            new DataColumn("ColAccountNumber", typeof(string)),
                            new DataColumn("ColCurrency", typeof(string)),
                            new DataColumn("ColRepresentativeName", typeof(string)),
                            new DataColumn("ColRepresentativeEmail".ToUpper(), typeof(string)),
                            new DataColumn("ColRepresentativePhonenumber", typeof(string)),
                            new DataColumn("ColIsGM".ToUpper(), typeof(string)),
                            new DataColumn("ColIsEMTUser".ToUpper(), typeof(string)),
                            new DataColumn("ColIsUserLimit", typeof(string)),
                            new DataColumn("ColDateOfContract", typeof(string))
                });
                dt.Columns.Add("ColStatus").DefaultValue = 0;
                dt.Columns.Add("ColDateCreated").DefaultValue = DateTime.Now;
                dt.Columns.Add("ColFileName").DefaultValue = DateTime.Now.ToString("yyyyMMddHH") + System.IO.Path.GetFileName(FileUpload.FileName);
                dt.Columns.Add("ColUploadedBy").DefaultValue = User.Identity.GetUserId();

                //Read the contents of CSV file.
                string csvData = System.IO.File.ReadAllText(filePath);
                //Add to help skip the first column
                Boolean headerRowHasBeenSkipped = false;
                //Execute a loop over the rows.
                foreach (string row in csvData.Split('\n'))
                {
                    if (headerRowHasBeenSkipped)
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            dt.Rows.Add();
                            int i = 0;
                            //Execute a loop over the columns.
                            foreach (string cell in row.Split(','))
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell;
                                i++;
                            }
                        }
                    } // outer if
                    headerRowHasBeenSkipped = true;
                }

                string consString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(consString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {

                        //Set the database table name.
                        sqlBulkCopy.DestinationTableName = "dbo.ExistingClientsUploads";

                        //[OPTIONAL]: Map the DataTable columns with that of the database table
                        sqlBulkCopy.ColumnMappings.Add("ColCompanyName", "CompanyName");
                        sqlBulkCopy.ColumnMappings.Add("ColCompanyEmail", "CompanyEmail");
                        sqlBulkCopy.ColumnMappings.Add("ColAcceptedTerms", "AcceptedTerms");
                        sqlBulkCopy.ColumnMappings.Add("ColEMTSignUp", "EMTSignUp");
                        sqlBulkCopy.ColumnMappings.Add("ColSSI", "SSI");
                        sqlBulkCopy.ColumnMappings.Add("ColAccountNumber", "AccountNumber");
                        sqlBulkCopy.ColumnMappings.Add("ColCurrency", "Currency");
                        sqlBulkCopy.ColumnMappings.Add("ColRepresentativeName", "RepresentativeName");
                        sqlBulkCopy.ColumnMappings.Add("ColRepresentativeEmail", "RepresentativeEmail");
                        sqlBulkCopy.ColumnMappings.Add("ColRepresentativePhonenumber", "RepresentativePhonenumber");
                        sqlBulkCopy.ColumnMappings.Add("ColIsGM", "IsGM");
                        sqlBulkCopy.ColumnMappings.Add("ColIsEMTUser", "IsEMTUser");
                        sqlBulkCopy.ColumnMappings.Add("ColIsUserLimit", "RepresentativeLimit");
                        sqlBulkCopy.ColumnMappings.Add("ColStatus", "Status");
                        sqlBulkCopy.ColumnMappings.Add("ColDateCreated", "DateCreated");
                        sqlBulkCopy.ColumnMappings.Add("ColUploadedBy", "UploadedBy");
                        sqlBulkCopy.ColumnMappings.Add("ColFileName", "FileName");
                        sqlBulkCopy.ColumnMappings.Add("ColDateOfContract", "DateOfContract");

                        try
                        {
                            con.Open();
                            sqlBulkCopy.WriteToServer(dt);

                            //Add audit trail
                            var LogAuditTrail = Functions.LogAuditTrail(1, "Upload Existing Clients", "ExistingClientsUploads", null, User.Identity.GetUserId(), filePath, null, null);
                        }
                        catch (Exception e)
                        {
                            return Json("Error uploading your csv file, please check the individual field formats and reupload", JsonRequestBehavior.AllowGet);
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                }

                //Send email to GMO approvers
                using (DBModel db = new DBModel())
                {
                    var userId = User.Identity.GetUserId();
                    var getUserDetails = db.AspNetUsers.SingleOrDefault(c => c.Id == userId);
                    var _action = "UploadExistingUser";
                    var DDUserRole = (from p in db.AspNetUserRoles
                                      join e in db.AspNetUsers on p.UserId equals e.Id
                                      where p.RoleId == "1ee65809-e6c4-4c6e-9cb4-99c71e7e7516" && e.Status == 1
                                      select new
                                      {
                                          EmailID = e.Email
                                      }).ToList();
                    foreach (var email in DDUserRole)
                    {
                        var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following user has successfully uploaded existing clients for your approval. <br/>" +
                                      "FullNames: " + getUserDetails.CompanyName + ", <br/> Email Address: " + getUserDetails.Email + ", <br/> Filename: " + DateTime.Now.ToString("yyyyMMddHH") + System.IO.Path.GetFileName(FileUpload.FileName) + " " +
                                      "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Existing Clients Upload", DDMessageBody);
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

                //Return Success Result
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error! Please choose a valid CSV file for upload", JsonRequestBehavior.AllowGet);
            }
        }

        //
        //Get //list
        public ActionResult CompleteApplications()
        {
            return View();
        }

        //
        //GET //Count
        public int GetCompletedApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.OPSApproved == false && a.OPSDeclined == false && a.Status == 1).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetCompletedApplicationsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                else if (jtSorting.Equals("Client ASC"))
                {
                    query = query.OrderBy(p => p.Client);
                }
                else if (jtSorting.Equals("Client DESC"))
                {
                    query = query.OrderByDescending(p => p.Client);
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
        public JsonResult GetCompletedApplications(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetCompletedApplicationsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE));").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%');").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE));").First();
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
        //GET //ViewCompletedApplication
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewCompletedApplication(int applicationId)
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
            }

            return PartialView();
        }

        //
        // POST //ExportCompleteApplications
        [HttpGet]
        public void ExportCompleteApplications(string searchText, string DateFrom, string DateTo)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                var searchString = searchText;
                var searchFromDate = DateFrom;
                var searchToDate = DateTo;
                var query = "";
                if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);

                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataColumn column in dt.Columns)
                            {
                                //Add the Header row for CSV file.
                                csv += column.ColumnName + ',';
                            }

                            //Add new line.
                            csv += "\r\n";

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_CompleteApplications.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }

        //
        //
        //Get //Approvals list
        public ActionResult IncompleteApplications()
        {
            return View();
        }

        //
        //GET //Count
        public int GetInCompletedApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => (a.Signatories != a.SignatoriesApproved || a.DesignatedUsers != a.UsersApproved) && a.Status == 1 && a.OPSApproved == false && a.POAApproved == false).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetInCompletedApplicationsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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

                return count > 0 ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                        : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetInCompletedApplications(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetInCompletedApplicationsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE));").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetInCompletedApplicationsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        // POST: /DigitalDesk/ViewInCompleteApplication
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewInCompleteApplication(int applicationId)
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
            }

            return PartialView();
        }

        //
        //GET //ExportSearchResults
        [HttpGet]
        public void ExportIncompleteApplications(string searchText, string DateFrom, string DateTo)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                var searchString = searchText;
                var searchFromDate = DateFrom;
                var searchToDate = DateTo;
                var query = "";
                if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);

                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataColumn column in dt.Columns)
                            {
                                //Add the Header row for CSV file.
                                csv += column.ColumnName + ',';
                            }

                            //Add new line.
                            csv += "\r\n";

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_IncompleteApplications.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }

        //
        //Get //Approvals list
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
                return db.EMarketApplications.Where(a => a.OPSApproved == true && a.POAApproved == true && a.Status == 1).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetApprovedApplicationsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
        public JsonResult GetApprovedApplications(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetApprovedApplicationsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetApprovedApplicationsCount();
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
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                ViewBag.ApplicationInfo = clientDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;
                ViewData["OpsComments"] = getApplicationInfo.OPSComments;
                ViewData["PoaComments"] = getApplicationInfo.POAComments;

                //Get the person who approved
                var OpsApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoApproved);
                var OpsDeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoDeclined);
                var POAApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.POAWhoApproved);
                var POADeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.POAWhoDeclined);
                
                //Ops Approved
                ViewData["OPSNames"] = OpsApproved.CompanyName;
                ViewData["OPSEmail"] = OpsApproved.Email;
                ViewData["OPSPhone"] = OpsApproved.PhoneNumber;
                DateTime dtByUser = DateTime.Parse(getApplicationInfo.OPSDateApproved.ToString());
                ViewData["OPSDateApproved"] = dtByUser.ToString("dd/MM/yyyy hh:mm:ss tt");

                //Poa Approved
                ViewData["POANames"] = POAApproved.CompanyName;
                ViewData["POAEmail"] = POAApproved.Email;
                ViewData["POAPhone"] = POAApproved.PhoneNumber;
                DateTime dtByUserPoa = DateTime.Parse(getApplicationInfo.POADateApproved.ToString());
                ViewData["POADateApproved"] = dtByUserPoa.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
            return PartialView();
        }

        //
        //GET //ExportApprovedApplications
        [HttpGet]
        public void ExportApprovedApplications(string searchText, string DateFrom, string DateTo)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                var searchString = searchText;
                var searchFromDate = DateFrom;
                var searchToDate = DateTo;
                var query = "";
                if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);

                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataColumn column in dt.Columns)
                            {
                                //Add the Header row for CSV file.
                                csv += column.ColumnName + ',';
                            }

                            //Add new line.
                            csv += "\r\n";

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_ApprovedApplications.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }


        //
        //Get //Approvals list
        public ActionResult DeclinedApplications()
        {
            return View();
        }

        //
        //GET //Count
        public int GetDeclinedApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Status == 4).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetDeclinedApplicationsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC, CASE WHEN s.POADeclined = 1 THEN 'POA' WHEN s.OPSDeclined = 1 THEN 'OPS' WHEN s.DigitalDeskDeclined = 1 THEN 'DigitalDesk' ELSE 'SELF' END AS DeclinedBy FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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

                return count > 0 ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                        : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the Datacount
        [HttpPost]
        public JsonResult GetDeclinedApplications(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetDeclinedApplicationsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetDeclinedApplicationsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        //Get //DeclinedApplications
        [HttpGet]
        public void ExportDeclinedApplications(string searchText, string DateFrom, string DateTo)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                var searchString = searchText;
                var searchFromDate = DateFrom;
                var searchToDate = DateTo;
                var query = "";
                if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 4;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 4;)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, r.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4;";
                    query = customquery;
                }

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataColumn column in dt.Columns)
                            {
                                //Add the Header row for CSV file.
                                csv += column.ColumnName + ',';
                            }

                            //Add new line.
                            csv += "\r\n";

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_DeclinedApplications.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }

        //
        // POST: /DigitalDesk/ViewInCompleteApplication
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewDeclinedApplication(int applicationId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId && c.Status == 4);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                ViewBag.ApplicationInfo = clientDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + getApplicationInfo.ClientID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;
            }

            return PartialView();
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
                            ApplicationUpdate.DigitalDeskDeclined = true;
                            ApplicationUpdate.DateDeclined = DateTime.Now;
                            ApplicationUpdate.DigWhoDeclined = currentUserId;
                            ApplicationUpdate.Status = 4;
                            ApplicationUpdate.OPSComments = model.Comments;
                            db.SaveChanges();

                            //2. Mark HasApplication False for Client Company
                            var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == ApplicationUpdate.CompanyID);
                            updateClientCompany.HasApplication = false;
                            db.SaveChanges();

                            //3. Delete Signatories
                            db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(c => c.ClientID == ClientDetails.Id && c.CompanyID == ApplicationUpdate.CompanyID));
                            db.SaveChanges();

                            //4. Delete Representatives
                            db.DesignatedUsers.RemoveRange(db.DesignatedUsers.Where(c => c.ClientID == ClientDetails.Id && c.CompanyID == ApplicationUpdate.CompanyID));
                            db.SaveChanges();

                            //5. Delete Settlement Accounts
                            db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(c => c.ClientID == ClientDetails.Id && c.CompanyID == ApplicationUpdate.CompanyID));
                            db.SaveChanges();

                            //6. Send email notification to Client Company
                            string EmailBody = string.Empty;
                            using (System.IO.StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/DigitalDeskApplicationDeclined.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{CompanyName}", CompanyDetails.CompanyName);
                            var SendEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, ClientDetails.EmailAddress, "Application Declined", EmailBody);

                            if (SendEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, ClientDetails.EmailAddress, _action);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, ClientDetails.EmailAddress, _action);
                            }

                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            //throw (ex);
                            return Json("Error! "+ ex +" ", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to update application details!", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json("Model Invalid! " + errors + " ", JsonRequestBehavior.AllowGet);
            }
        }

        //
        //Get //Pending Approval list
        public ActionResult PendingApprovals()
        {
            return View();
        }

        //
        //GET //Count
        public int GetPendingApprovalsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Status == 1 && a.DesignatedUsers == a.UsersApproved && a.Signatories == a.SignatoriesApproved && (a.OPSApproved == false || a.POAApproved == false)).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetPendingApprovalsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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

                return count > 0 ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                        : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the Datacount
        [HttpPost]
        public JsonResult GetPendingApprovals(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetPendingApprovalsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
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
        //Get //DeclinedApplications
        [HttpGet]
        public void ExportPendingApprovals(string searchText, string DateFrom, string DateTo)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                var searchString = searchText;
                var searchFromDate = DateFrom;
                var searchToDate = DateTo;
                var query = "";
                if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1;)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1;";
                    query = customquery;
                }

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataColumn column in dt.Columns)
                            {
                                //Add the Header row for CSV file.
                                csv += column.ColumnName + ',';
                            }

                            //Add new line.
                            csv += "\r\n";

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_PendingApprovals.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }

        //
        // POST: /DigitalDesk/ViewIncompleteApplications
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewIncompleteApplications(int applicationId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                ViewBag.ApplicationInfo = clientDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;

            }
            return PartialView();
        }

        //
        // POST: /DigitalDesk/ViewPendingOpsApplications
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewPendingOpsApplications(int applicationId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                ViewBag.ApplicationInfo = clientDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;

            }
            return PartialView();
        }

        //
        // POST: /DigitalDesk/ViewPendingOpsApplications
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewPendingPoaApplications(int applicationId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                ViewBag.ApplicationInfo = clientDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;
                ViewData["OpsComments"] = getApplicationInfo.OPSComments;

                //Get the person who approved
                var OpsApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoApproved);
                var OpsDeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoDeclined);

                //Ops Approved
                ViewData["OPSNames"] = OpsApproved.CompanyName;
                ViewData["OPSEmail"] = OpsApproved.Email;
                ViewData["OPSPhone"] = OpsApproved.PhoneNumber;
                DateTime dtByUser = DateTime.Parse(getApplicationInfo.OPSDateApproved.ToString());
                ViewData["OPSDateApproved"] = dtByUser.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
            return PartialView();
        }

        //
        //Get All Applications List from table
        public IEnumerable<ClientApplicationsViewModel> GetApprovedAppplications()
        {
            using (DBModel db = new DBModel())
            {
                var Query = from a in db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.POAApproved == true && a.OPSApproved == true)
                            join b in db.RegisteredClients on a.ClientID equals b.Id
                            join c in db.tblStatus on a.Status equals c.Id
                            select new ClientApplicationsViewModel
                            {
                                ApplicationID = a.Id,
                                //Client = b.CompanyName,
                                Status = c.StatusName,
                                DateCreated = a.DateCreated,
                                OPSApproved = a.OPSApproved,
                                POAApproved = a.POAApproved
                            };
                return Query.OrderByDescending(x => x.ApplicationID).ToList();
            }
        }

        //
        // POST: /Admin/ViewApprovedApplications
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewApprovedApplications(int applicationId)
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

                //Get the OPS who approved
                var OpsApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoApproved);
                var OpsDeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoDeclined);
                ViewData["OPSNames"] = OpsApproved.CompanyName;
                ViewData["OPSEmail"] = OpsApproved.Email;
                ViewData["OPSPhone"] = OpsApproved.PhoneNumber;
                DateTime dtByUser = DateTime.Parse(getApplicationInfo.OPSDateApproved.ToString());
                ViewData["OPSDateApproved"] = dtByUser.ToString("dd/MM/yyyy hh:mm:ss tt");

                //Get the POA who approved
                var POAApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.POAWhoApproved);
                var POADeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.POAWhoDeclined);
                ViewData["POANames"] = POAApproved.CompanyName;
                ViewData["POAEmail"] = POAApproved.Email;
                ViewData["POAPhone"] = POAApproved.PhoneNumber;
                DateTime dtByUser2 = DateTime.Parse(getApplicationInfo.POADateApproved.ToString());
                ViewData["POADateApproved"] = dtByUser2.ToString("dd/MM/yyyy hh:mm:ss tt");

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
        //ResetExpiredOTP
        //Get Approvals list
        public ActionResult ResetExpiredOTP()
        {
            return View();
        }

        //
        //GET //Get Count
        public int GetClientsExpiredOTPCounts()
        {
            using (var db = new DBModel())
            {
                return db.RegisteredClients.Where(a => a.Status == 0 && a.RegistrationConfirmationDate == null && a.OTP != null && (DbFunctions.DiffMinutes(a.DateCreated, DateTime.Now) >= Properties.Settings.Default.ClientOTPExpiry)).Count();
            }
        }

        //
        //GET //Get Notifications List
        public List<ExpiredOtpsViewModel> GetClientsExpiredOTPList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ExpiredOtpsViewModel> query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID, CONCAT(a.Surname,' ', a.OtherNames) CompanyName, s.StatusName Status, a.DateCreated, a.EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM RegisteredClients a INNER JOIN tblStatus s ON s.Id = a.Status " +
                  "WHERE DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.ClientOTPExpiry + "' AND a.Status = 0;");

                //Search 
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID, CONCAT(a.Surname,' ', a.OtherNames) CompanyName, s.StatusName Status, a.DateCreated, a.EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM RegisteredClients a INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.CompanyName LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.ClientOTPExpiry + "' AND a.Status = 0;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.ClientID);
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
                else if (jtSorting.Equals("CompanyName ASC"))
                {
                    query = query.OrderBy(p => p.CompanyName);
                }
                else if (jtSorting.Equals("CompanyName DESC"))
                {
                    query = query.OrderByDescending(p => p.CompanyName);
                }
                else if (jtSorting.Equals("EmailAddress ASC"))
                {
                    query = query.OrderBy(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("EmailAddress DESC"))
                {
                    query = query.OrderByDescending(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else if (jtSorting.Equals("TimeExpired ASC"))
                {
                    query = query.OrderBy(p => p.TimeExpired);
                }
                else if (jtSorting.Equals("TimeExpired DESC"))
                {
                    query = query.OrderByDescending(p => p.TimeExpired);
                }
                else
                {
                    //Default Sort!  
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetClientsExpiredOTP(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetClientsExpiredOTPList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) count FROM RegisteredClients n WHERE n.Status = 0 AND n.RegistrationConfirmationDate = NULL AND (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%' AND a.Status = 0;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetClientsExpiredOTPCounts();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //POST Load Client Details
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResetClientOTP(int getClientId)
        {
            using (DBModel db = new DBModel())
            {
                var getClientInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == getClientId);

                //Data For View Display
                //ViewData["CompanyNames"] = getClientInfo.CompanyName;
                ViewData["EmailAddress"] = getClientInfo.EmailAddress;
                ViewData["ClientId"] = getClientInfo.Id;
            }

            return PartialView();
        }

        //
        //Reset Client OTP
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetClientOTP(ResetClientOTPViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                //Generate new OTP
                var _activationCode = OTPGenerator.GetUniqueKey(6);
                string activationCode = Shuffle.StringMixer(_activationCode);
                var ClientToUpdate = db.RegisteredClients.SingleOrDefault(b => b.Id == model.getClientId);
                var callbackUrl = Url.Action("CompleteRegistration", "Account", null, Request.Url.Scheme);
                var _action = "ResetClientOTP";
                if (ClientToUpdate != null)
                {
                    try
                    {
                        //1. Update Client account details
                        ClientToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                        ClientToUpdate.DateCreated = DateTime.Now;
                        var saveInfo = db.SaveChanges();

                        if (saveInfo > 0)
                        {
                            //2. Send Email with new OTP
                            string EmailBody = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ResendOTP.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{CompanyName}", ClientToUpdate.Surname);
                            EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                            EmailBody = EmailBody.Replace("{ActivationCode}", activationCode);

                            var SendSuccessEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, ClientToUpdate.EmailAddress, "Client Reset OTP", EmailBody);

                            if (SendSuccessEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, ClientToUpdate.EmailAddress, _action);
                                var LogAuditTrail = Functions.LogAuditTrail(model.getClientId, "Reset Clients OTP", "RegisteredClients", null, User.Identity.GetUserId(), ClientToUpdate.Surname +" "+ ClientToUpdate.OtherNames, ClientToUpdate.EmailAddress, null);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, ClientToUpdate.EmailAddress, _action);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            return RedirectToAction("ResetExpiredOTP");
        }

        //
        //Get //ResetSignatoryOTP
        public ActionResult ResetSignatoryOTP()
        {
            return View();
        }

        //
        //GET //Get Count
        public int GetSignatoriesExpiredOTPCounts()
        {
            using (var db = new DBModel())
            {
                return db.ClientSignatories.Where(a => a.Status == 0 && a.ConfirmationDate == null && a.OTP != null && (DbFunctions.DiffMinutes(a.DateCreated, DateTime.Now) >= Properties.Settings.Default.SignatoryOTPExpiry)).Count();
            }
        }

        //
        //GET //Get Notifications List
        public List<ExpiredOtpsViewModel>  GetSignatoriesExpiredOTPList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ExpiredOtpsViewModel> query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID,  CONCAT(a.Surname, ' ', a.OtherNames) CompanyName, s.StatusName Status, a.DateCreated, a.EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM ClientSignatories a INNER JOIN tblStatus s ON s.Id = a.Status WHERE DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.SignatoryOTPExpiry + "' AND a.OTP IS NOT NULL AND a.Status = 0;");

                //Search
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID,  CONCAT(a.Surname, ' ', a.OtherNames) CompanyName, s.StatusName Status, a.DateCreated, a.EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM ClientSignatories a INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.OtherNames LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.SignatoryOTPExpiry + "' AND a.OTP IS NOT NULL AND a.Status = 0;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.ClientID);
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
                else if (jtSorting.Equals("CompanyName ASC"))
                {
                    query = query.OrderBy(p => p.CompanyName);
                }
                else if (jtSorting.Equals("CompanyName DESC"))
                {
                    query = query.OrderByDescending(p => p.CompanyName);
                }
                else if (jtSorting.Equals("EmailAddress ASC"))
                {
                    query = query.OrderBy(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("EmailAddress DESC"))
                {
                    query = query.OrderByDescending(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else if (jtSorting.Equals("TimeExpired ASC"))
                {
                    query = query.OrderBy(p => p.TimeExpired);
                }
                else if (jtSorting.Equals("TimeExpired DESC"))
                {
                    query = query.OrderByDescending(p => p.TimeExpired);
                }
                else
                {
                    //Default Sort!  
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging 
            }
        }

        //
        //GET //POST From URL  
        [HttpPost]
        public JsonResult GetSignatoriesExpiredOTP(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetSignatoriesExpiredOTPList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) count FROM ClientSignatories n WHERE n.Status = 0 AND n.ConfirmationDate = NULL AND (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%' AND a.OTP IS NOT NULL AND a.Status = 0;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetSignatoriesExpiredOTPCounts();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //
        //POST Load Signatory Details
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResetSignatoryOTP(int getSignatoryId)
        {
            using (DBModel db = new DBModel())
            {
                var getSignatoryInfo = db.ClientSignatories.SingleOrDefault(c => c.Id == getSignatoryId);

                //Data For View Display
                ViewData["Names"] = getSignatoryInfo.Surname + " " + getSignatoryInfo.OtherNames;
                ViewData["EmailAddress"] = getSignatoryInfo.EmailAddress;
                ViewData["SignatoryId"] = getSignatoryInfo.Id;
            }

            return PartialView();
        }

        //
        //Reset Signatory OTP
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetSignatoryOTP(ResetSignatoryOTPViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                //Generate new OTP
                var _activationCode = OTPGenerator.GetUniqueKey(6);
                string activationCode = Shuffle.StringMixer(_activationCode);
                var SignatoryToUpdate = db.ClientSignatories.SingleOrDefault(b => b.Id == model.getSignatoryId);
                var callbackUrl = Url.Action("SignatoryConfirmation", "Account", null, Request.Url.Scheme);
                var _action = "ResetSignatoryOTP";
                if (SignatoryToUpdate != null)
                {

                    try
                    {
                        //1. Update Client account details
                        SignatoryToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                        SignatoryToUpdate.DateCreated = DateTime.Now;
                        var saveInfo = db.SaveChanges();

                        if (saveInfo > 0)
                        {
                            //2. Send Email with new OTP
                            string EmailBody = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ResendOTP.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{CompanyName}", SignatoryToUpdate.Surname);
                            EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                            EmailBody = EmailBody.Replace("{ActivationCode}", activationCode);

                            var SendSuccessEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, SignatoryToUpdate.EmailAddress, "Reset Expired OTP", EmailBody);

                            if (SendSuccessEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, SignatoryToUpdate.EmailAddress, _action);
                                var LogAuditTrail = Functions.LogAuditTrail(model.getSignatoryId, "Reset Clients OTP", "RegisteredClients", null, User.Identity.GetUserId(), SignatoryToUpdate.Surname + " " + SignatoryToUpdate.OtherNames, SignatoryToUpdate.EmailAddress, null);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, SignatoryToUpdate.EmailAddress, _action);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            return RedirectToAction("ResetSignatoryOTP");
        }

        //
        //Get //ResetSignatoryOTP
        public ActionResult ResetRepresentativeOTP()
        {
            return View();
        }

        //
        //GET //Count
        public int GetRepresentativesExpiredOTPCounts()
        {
            using (var db = new DBModel())
            {
                return db.DesignatedUsers.Where(a => a.Status == 0 && a.ConfirmationDate == null && a.OTP != null && (DbFunctions.DiffMinutes(a.DateCreated, DateTime.Now) >= Properties.Settings.Default.RepresentativeOTPExpiry)).Count();
            }
        }

        //
        //GET //Get List
        public List<ExpiredOtpsViewModel> GetRepresentativesExpiredOTPList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ExpiredOtpsViewModel> query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID,  CONCAT(a.Surname, ' ', a.OtherNames) CompanyName, s.StatusName Status, a.DateCreated, a.Email EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM DesignatedUsers a INNER JOIN tblStatus s ON s.Id = a.Status WHERE DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.RepresentativeOTPExpiry + "' AND a.OTP IS NOT NULL AND a.Status = 0;");

                //Search
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID,  CONCAT(a.Surname, ' ', a.OtherNames) CompanyName, s.StatusName Status, a.DateCreated, a.Email EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM DesignatedUsers a INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.OtherNames LIKE '%" + searchMessage + "%' OR a.Email LIKE '%" + searchMessage + "%') AND DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.RepresentativeOTPExpiry + "' AND a.OTP IS NOT NULL AND a.Status = 0;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.ClientID);
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
                else if (jtSorting.Equals("CompanyName ASC"))
                {
                    query = query.OrderBy(p => p.CompanyName);
                }
                else if (jtSorting.Equals("CompanyName DESC"))
                {
                    query = query.OrderByDescending(p => p.CompanyName);
                }
                else if (jtSorting.Equals("EmailAddress ASC"))
                {
                    query = query.OrderBy(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("EmailAddress DESC"))
                {
                    query = query.OrderByDescending(p => p.EmailAddress);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else if (jtSorting.Equals("TimeExpired ASC"))
                {
                    query = query.OrderBy(p => p.TimeExpired);
                }
                else if (jtSorting.Equals("TimeExpired DESC"))
                {
                    query = query.OrderByDescending(p => p.TimeExpired);
                }
                else
                {
                    //Default Sort!  
                    query = query.OrderByDescending(p => p.DateCreated);
                }

                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging 
            }
        }

        //
        //GET //POST From URL  
        [HttpPost]
        public JsonResult GetRepresentativesExpiredOTP(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetRepresentativesExpiredOTPList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) count FROM DesignatedUsers n WHERE n.Status = 0 AND n.ConfirmationDate = NULL AND (n.Surname LIKE '%" + searchMessage + "%' OR n.OtherNames LIKE '%" + searchMessage + "%' OR n.Email LIKE '%" + searchMessage + "%' AND a.OTP IS NOT NULL AND a.Status = 0;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetRepresentativesExpiredOTPCounts();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //POST Load Representative Details
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadResetRepresentativesOTP(int getRepresentativeId)
        {
            using (DBModel db = new DBModel())
            {
                var getRepresentativeInfo = db.DesignatedUsers.SingleOrDefault(c => c.Id == getRepresentativeId);

                //Data For View Display
                ViewData["Names"] = getRepresentativeInfo.Surname + " " + getRepresentativeInfo.Othernames;
                ViewData["EmailAddress"] = getRepresentativeInfo.Email;
                ViewData["RepresentativeId"] = getRepresentativeInfo.Id;
            }

            return PartialView();
        }

        //
        //Reset Representative OTP
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetRepresentativeOTP(ResetRepresentativeOTPViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                //Generate new OTP
                var _activationCode = OTPGenerator.GetUniqueKey(6);
                string activationCode = Shuffle.StringMixer(_activationCode);
                var RepresentiveToUpdate = db.DesignatedUsers.SingleOrDefault(b => b.Id == model.getRepresentativeId);
                var callbackUrl = Url.Action("DesignatedUserConfirmation", "Account", null, Request.Url.Scheme);
                var _action = "ResetRepresentativeOTP";
                if (RepresentiveToUpdate != null)
                {
                    //1. Update Client account details
                    try
                    {
                        RepresentiveToUpdate.OTP = Functions.GenerateMD5Hash(activationCode);
                        RepresentiveToUpdate.DateCreated = DateTime.Now;
                        var saveInfo = db.SaveChanges();

                        if (saveInfo > 0)
                        {
                            //1. Send Email with new OTP
                            string EmailBody = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ResendOTP.html")))
                            {
                                EmailBody = reader.ReadToEnd();
                            }
                            EmailBody = EmailBody.Replace("{CompanyName}", RepresentiveToUpdate.Surname);
                            EmailBody = EmailBody.Replace("{Url}", callbackUrl);
                            EmailBody = EmailBody.Replace("{ActivationCode}", activationCode);

                            var SendSuccessEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, RepresentiveToUpdate.Email, "Reset Expired OTP", EmailBody);

                            if (SendSuccessEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, RepresentiveToUpdate.Email, _action);
                                var LogAuditTrail = Functions.LogAuditTrail(model.getRepresentativeId, "Reset Clients OTP", "RegisteredClients", null, User.Identity.GetUserId(), RepresentiveToUpdate.Surname + " " + RepresentiveToUpdate.Othernames, RepresentiveToUpdate.Email, null);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, RepresentiveToUpdate.Email, _action);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            return RedirectToAction("ResetSignatoryOTP");
        }

        //
        //Get //UnlockClients
        public ActionResult UnlockClients()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetLockedUsersCount()
        {
            using (var db = new DBModel())
            {
                var queryCount = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName, u.AccessFailedCount FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.LockoutEndDateUtc IS NOT NULL AND r.Id IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25')").Count();
                return queryCount;
            }
        }

        //
        //GET /Get Notifications List
        public List<UserListViewModel> GetLockedOutUsersList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<UserListViewModel> query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName, u.AccessFailedCount FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.LockoutEndDateUtc IS NOT NULL AND r.Id IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25') ORDER BY u.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY");

                //Search 
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName, u.AccessFailedCount FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.Email LIKE '%" + searchMessage + "%' AND u.LockoutEndDateUtc IS NOT NULL AND r.Id IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25') ORDER BY u.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY");
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("DateCreated ASC"))
                {
                    query = query.OrderBy(p => p.DateCreated);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id); //Default!  
                }

                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetLockedOutUsers(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetLockedOutUsersList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) count FROM AspNetUsers n WHERE n.LockoutEndDateUtc IS NOT NULL AND n.Email LIKE '%" + searchMessage + "%';").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetLockedUsersCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        //POST //LoadResendNotification
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadUnlockUser(string getUserId)
        {
            using (DBModel db = new DBModel())
            {
                var getUserInfo = db.AspNetUsers.SingleOrDefault(c => c.Id == getUserId.ToString());

                //Data For View Display
                ViewData["Email"] = getUserInfo.Email;
                ViewData["UserId"] = getUserInfo.Id;
            }

            return PartialView();
        }

        //
        //POST //Unlock User
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UnlockUser(DeleteUserViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                var getUserToUnlock = db.AspNetUsers.SingleOrDefault(c => c.Id == model.UserId);
                getUserToUnlock.AccessFailedCount = 0;
                getUserToUnlock.LockoutEndDateUtc = null;
                getUserToUnlock.Status = 1;
                var _action = "UnlockUser";
                var recordSaved = db.SaveChanges();

                if (recordSaved > 0)
                {
                    //Send email notification to user
                    var callbackUrl = Url.Action("ResetAccount", "Account", null, Request.Url.Scheme);
                    string EmailBody = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/UnlockAccount.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{CompanyName}", getUserToUnlock.CompanyName);
                    EmailBody = EmailBody.Replace("{Url}", callbackUrl);

                    var unlockEmailSent = MailHelper.SendMailMessage(MailHelper.EmailFrom, getUserToUnlock.CompanyName, "Account Unlocked", EmailBody);
                    if (unlockEmailSent == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, getUserToUnlock.CompanyName, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, getUserToUnlock.CompanyName, _action);
                    }
                }
            }

            return RedirectToAction("UnlockClients");
        }

        //
        //Get //Pending Approvals
        public ActionResult PendingPOAApprovals()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetPendingPOAApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == true && a.OPSDeclined == false) && a.Status == 1 && a.POAApproved == false && a.POADeclined == false).Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<ClientApplicationsViewModel> GetPendingPOAApplicationsList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.OPSDeclined = 0 AND s.POAApproved = 0 AND s.POADeclined = 0 AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.OPSDeclined = 0 AND s.POAApproved = 0 AND s.POADeclined = 0 AND s.Status = 1 (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

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
        public JsonResult GetPendingPOAApplications(string searchMessage = "", string searchDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetPendingPOAApplicationsList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN RegisteredClients r ON r.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.OPSDeclined = 0 AND s.POAApproved = 0 AND s.POADeclined = 0 AND s.Status = 1 (b.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetPendingPOAApplicationsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        // POST: /Admin/ViewOpsApprovedApplication
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewOpsApprovedApplication(int applicationId)
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

                //Get the OPS who approved
                var OpsApproved = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoApproved);
                var OpsDeclined = db.AspNetUsers.FirstOrDefault(a => a.Id == getApplicationInfo.OPSWhoDeclined);
                ViewData["OPSNames"] = OpsApproved.CompanyName;
                ViewData["OPSEmail"] = OpsApproved.Email;
                ViewData["OPSPhone"] = OpsApproved.PhoneNumber;
                DateTime dtByUser = DateTime.Parse(getApplicationInfo.OPSDateApproved.ToString());
                ViewData["OPSDateApproved"] = dtByUser.ToString("dd/MM/yyyy hh:mm:ss tt");
            }

            return PartialView();
        }

        //
        //Get //Count
        public int GetUploadedClientsCount()
        {
            using (DBModel db = new DBModel())
            {
                var query = db.Database.SqlQuery<ExistingClientsUploadViewModel>("SELECT count(a.[FileName]) count FROM ExistingClientsUploads a INNER JOIN AspNetUsers u ON u.Id = a.UploadedBy WHERE a.UploadedBy = '" + User.Identity.GetUserId() + "' GROUP BY a.[FileName];").Count();
                return query;
            }
        }

        //
        //GET /Get Currencies List
        public List<ExistingClientsUploadViewModel> GetUploadedClientsList(int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext
            using (DBModel db = new DBModel())
            {
                IEnumerable<ExistingClientsUploadViewModel> query = db.Database.SqlQuery<ExistingClientsUploadViewModel>("SELECT a.[FileName], u.CompanyName UploadedBy, a.Status, a.DateCreated FROM ExistingClientsUploads a INNER JOIN AspNetUsers u ON u.Id = a.UploadedBy WHERE a.UploadedBy = '" + User.Identity.GetUserId() + "' GROUP BY u.CompanyName, a.[FileName], a.Status, a.DateCreated ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //No Search parameters

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("DateCreated ASC"))
                {
                    query = query.OrderBy(p => p.DateCreated);
                }
                else if (jtSorting.Equals("DateCreated DESC"))
                {
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                else if (jtSorting.Equals("UploadedBy ASC"))
                {
                    query = query.OrderBy(p => p.UploadedBy);
                }
                else if (jtSorting.Equals("UploadedBy DESC"))
                {
                    query = query.OrderByDescending(p => p.UploadedBy);
                }
                else if (jtSorting.Equals("FileName ASC"))
                {
                    query = query.OrderBy(p => p.FileName);
                }
                else if (jtSorting.Equals("FileName DESC"))
                {
                    query = query.OrderByDescending(p => p.FileName);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else
                {
                    //Default!
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging 
            }
        }

        //
        //GET //Gets the list and returns Json object
        [HttpPost]
        public JsonResult GetUploadedClients(int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetUploadedClientsList(jtStartIndex, jtPageSize, count, jtSorting);
                var recordCount = GetUploadedClientsCount();
                return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
            }
        }

        //
        //GET //ViewUploadedClient
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewUploadedClient(string fileName)
        {
            ViewData["FileName"] = fileName;
            return PartialView();
        }

        //
        //POST //Gets the list and returns a Json object
        [HttpPost]
        public JsonResult GetUploadApprovals(int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null, string fileName = null)
        {
            using (var db = new DBModel())
            {
                int recordCount = db.ExistingClientsUploads.Where(a => a.FileName == fileName).Count();
                var Query = db.Database.SqlQuery<ExistingClientsUpload>("SELECT n.* FROM ExistingClientsUploads n WHERE n.[FileName] = " + "'" + fileName + "'" + " ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                var approvals = Query.ToList();
                return Json(new { Result = "OK", Records = approvals, TotalRecordCount = recordCount });
            }
        }

        //
        //Approve records
        [HttpPost]
        public JsonResult DeleteSelected(int Id)
        {
            using (var db = new DBModel())
            {
                try
                {
                    var RecordToUpdate = db.ExistingClientsUploads.SingleOrDefault(c => c.Id == Id);
                    RecordToUpdate.Status = 4;
                    RecordToUpdate.ApprovedBy = User.Identity.GetUserId();
                    RecordToUpdate.DateApproved = DateTime.Now;
                    db.SaveChanges();
                    return Json("success", JsonRequestBehavior.AllowGet);

                }
                catch (Exception)
                {
                    return Json("Error! Unable to delete selected records", JsonRequestBehavior.AllowGet);
                }

            }
        }

        //POST ClientCompanies
        public ActionResult ClientCompanies()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetClientCompaniesCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.ClientCompanies.Where(e => e.Status == 1).Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<ClientCompany> GetClientCompaniesList(string searchMessage, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<ClientCompany> query = db.Database.SqlQuery<ClientCompany>("SELECT * FROM ClientCompanies c LEFT JOIN EMarketApplications e ON c.Id = e.CompanyID WHERE c.Status = 1;");

                //Search
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientCompany>("SELECT * FROM ClientCompanies c LEFT JOIN EMarketApplications e ON c.Id = e.CompanyID WHERE (c.CompanyName LIKE '%" + searchMessage + "%' OR c.CompanyRegNumber LIKE '%" + searchMessage + "%' OR c.BusinessEmailAddress LIKE '%" + searchMessage + "%' OR c.CompanyBuilding LIKE '%" + searchMessage + "%' OR c.AttentionTo LIKE '%" + searchMessage + "%') AND c.Status = 1;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
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
                else if (jtSorting.Equals("CompanyRegNumber ASC"))
                {
                    query = query.OrderBy(p => p.CompanyRegNumber);
                }
                else if (jtSorting.Equals("CompanyRegNumber DESC"))
                {
                    query = query.OrderByDescending(p => p.CompanyRegNumber);
                }
                else if (jtSorting.Equals("CompanyBuilding ASC"))
                {
                    query = query.OrderBy(p => p.CompanyBuilding);
                }
                else if (jtSorting.Equals("CompanyBuilding DESC"))
                {
                    query = query.OrderByDescending(p => p.CompanyBuilding);
                }
                else if (jtSorting.Equals("BusinessEmailAddress ASC"))
                {
                    query = query.OrderBy(p => p.BusinessEmailAddress);
                }
                else if (jtSorting.Equals("BusinessEmailAddress DESC"))
                {
                    query = query.OrderByDescending(p => p.BusinessEmailAddress);
                }
                else if (jtSorting.Equals("HasApplication ASC"))
                {
                    query = query.OrderBy(p => p.HasApplication);
                }
                else if (jtSorting.Equals("HasApplication DESC"))
                {
                    query = query.OrderByDescending(p => p.HasApplication);
                }
                else if (jtSorting.Equals("Status ASC"))
                {
                    query = query.OrderBy(p => p.Status);
                }
                else if (jtSorting.Equals("Status DESC"))
                {
                    query = query.OrderByDescending(p => p.Status);
                }
                else
                {
                    //Default!
                    query = query.OrderByDescending(p => p.Id);
                }
                return count > 0
                           ? query.Skip(jtStartIndex).Take(count).ToList()  //Paging  
                           : query.ToList(); //No paging  
            }
        }

        //
        //GET //Gets the  
        [HttpPost]
        public JsonResult GetClientCompanies(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetClientCompaniesList(searchMessage, jtStartIndex, jtPageSize, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(c.Id) AS COUNT FROM ClientCompanies c WHERE (c.CompanyName LIKE '%" + searchMessage + "%' OR c.CompanyRegNumber LIKE '%" + searchMessage + "%' OR c.BusinessEmailAddress LIKE '%" + searchMessage + "%' OR c.CompanyBuilding LIKE '%" + searchMessage + "%' OR c.AttentionTo LIKE '%" + searchMessage + "%') AND c.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else
                {
                    var count = GetClientCompaniesCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
            }
        }

        //
        //GET //ViewCompletedApplication
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewCompanyDetails(int companyId)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.CompanyID == companyId && c.Status == 1);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                var companyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == companyId);
                ViewBag.ApplicationInfo = clientDetails;
                ViewBag.CompanyInfo = companyDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID && a.CompanyID == companyId).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID && a.CompanyID == companyId).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + getApplicationInfo.ClientID + "'" + " AND s.CompanyID =  " + "'" + companyId + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();
            }

            return PartialView();
        }

        //GET //ExportClientSettlements
        [HttpGet]
        public void ExportClientCompanies(string searchText)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                var searchString = searchText;
                var query = "";
                if (!string.IsNullOrEmpty(searchString))
                {
                    var customquery = "SELECT c.CompanyName, c.CompanyRegNumber, c.CompanyBuilding, c.CompanyStreet, c.CompanyTownCity, c.BusinessEmailAddress, c.AttentionTo, c.PostalAddress, c.PostalCode, c.TownCity, CASE WHEN c.HasApplication = 1 THEN 'HasApplication' ELSE 'NO-Application' END AS HasApplication, e.DateCreated ApplicationDate FROM ClientCompanies c LEFT JOIN EMarketApplications e ON c.Id = e.CompanyID WHERE (c.CompanyName LIKE '%" + searchString + "%' OR c.CompanyRegNumber LIKE '%" + searchString + "%' OR c.BusinessEmailAddress LIKE '%" + searchString + "%' OR c.CompanyBuilding LIKE '%" + searchString + "%' OR c.AttentionTo LIKE '%" + searchString + "%') AND c.Status = 1;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT c.CompanyName, c.CompanyRegNumber, c.CompanyBuilding, c.CompanyStreet, c.CompanyTownCity, c.BusinessEmailAddress, c.AttentionTo, c.PostalAddress, c.PostalCode, c.TownCity, CASE WHEN c.HasApplication = 1 THEN 'HasApplication' ELSE 'NO-Application' END AS HasApplication, e.DateCreated ApplicationDate FROM ClientCompanies c LEFT JOIN EMarketApplications e ON c.Id = e.CompanyID WHERE c.Status = 1;";
                    query = customquery;
                }

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);

                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataColumn column in dt.Columns)
                            {
                                //Add the Header row for CSV file.
                                csv += column.ColumnName + ',';
                            }

                            //Add new line.
                            csv += "\r\n";

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_ClientCompanies.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }
    }
}
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
        private bool SSI;

        // GET: DigitalDesk
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var RegisteredClients = db.RegisteredClients.Count(a => a.Status != 4);
                var CompletedApplications = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved);
                var InCompleteApplications = db.EMarketApplications.Count(a => (a.Signatories != a.SignatoriesApproved || a.DesignatedUsers != a.UsersApproved) && (a.Status == 1 && a.OPSApproved == false && a.POAApproved == false));
                var Approvals = db.EMarketApplications.Count(a => (a.OPSApproved == true && a.POAApproved == true));
                var PendingOpsApprovals = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.OPSApproved == false && a.OPSDeclined == false));
                var PendingPOAApprovals = db.EMarketApplications.Count(a => (a.OPSApproved == true && a.Status == 1 && (a.POAApproved == false && a.POADeclined == false)));
                var PendingApprovals = db.EMarketApplications.Count(a => (a.OPSApproved == false || a.POAApproved == false));
                var DeclinedApplications = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && (a.POADeclined == true || a.OPSDeclined == true));

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
                ViewData["Chart2Data"] = "[" + Approvals + ", " + PendingOpsApprovals + ", " + InCompleteApplications + ", " + DeclinedApplications + "]";

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
                IEnumerable<NotificationsViewModel> query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated FROM Notifications n ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchDate))
                {
                    query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated FROM Notifications n WHERE n.[To] LIKE '%" + searchMessage + "%' ORDER BY n.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (!string.IsNullOrEmpty(searchDate) && string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated FROM Notifications n WHERE CAST(n.DateCreated AS date) = " + "'" + searchDate + "'" + " ORDER BY n.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchDate))
                {
                    query = db.Database.SqlQuery<NotificationsViewModel>("SELECT n.Id, n.[Type], n.[To], n.[From], n.MessageBody, n.[Sent], CONVERT(varchar, n.DateCreated, 120) DateCreated FROM Notifications n WHERE CAST(n.DateCreated AS date) = " + "'" + searchDate + "'" + " AND n.[To] LIKE '%" + searchMessage + "%' ORDER BY n.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                    //ReSend Email
                    var EmailMessage = (model.ResendMessage).Trim();
                    var Action = "ResendNotification";
                    var EmailResent = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.ResendEmail, "Resent Email", EmailMessage);

                    if (EmailResent == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailMessage, model.ResendEmail, Action);
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
                return db.RegisteredClients.Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<RegisteredClient> GetRegisteredClientsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<RegisteredClient> query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients r WHERE r.Status = 1;");

                //Search
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.Status = 1 AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE));");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.Status = 1;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.Status = 1 AND n.DateCreated >= CAST('" + searchFromDate + "' AS DATE);");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.Status = 1 AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE);");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<RegisteredClient>("SELECT * FROM RegisteredClients n WHERE n.Status = 1 AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE));");
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
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = count });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(n.Id) AS COUNT FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchMessage + "%' OR n.EmailAddress LIKE '%" + searchMessage + "%') AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND n.Status = 1;").First();
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
        public PartialViewResult ViewClient(int clientId)
        {
            using (DBModel db = new DBModel())
            {
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientId);
                ViewBag.ApplicationInfo = clientDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientId).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientId).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientId + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                var clientHasApplication = db.EMarketApplications.Any(s => s.ClientID == clientId);
                ViewBag.clientHasApplication = clientHasApplication;

                //Data For Controller Post
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;
                //ViewData["CompanyName"] = clientDetails.CompanyName;
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
                    //Create New Currency
                    var newCurrency = db.Currencies.Create();
                    newCurrency.CurrencyName = model.CurrencyName;
                    newCurrency.CurrencyShort = model.CurrencyShortName;
                    newCurrency.Status = 1;
                    newCurrency.CreatedBy = User.Identity.GetUserId();
                    db.Currencies.Add(newCurrency);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("ManageCurrencies");
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
                var getCurrencyUpdate = db.Currencies.SingleOrDefault(c => c.Id == model.EditId);
                getCurrencyUpdate.CurrencyName = model.EditCurrencyName;
                getCurrencyUpdate.Status = model.EditTModeStatus;
                getCurrencyUpdate.CurrencyShort = model.EditCurrencyShortName;
                getCurrencyUpdate.Status = model.EditStatus;
                db.SaveChanges();
            }

            return RedirectToAction("ManageCurrencies");
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
                IEnumerable<ClientSignatoriesViewModel> query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ',a.OtherNames) AS Names, a.EmailAddress AS Email, a.PhoneNumber, a.AcceptedTerms, s.StatusName Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM ClientSignatories a INNER JOIN RegisteredClients r ON r.Id = a.ClientID INNER JOIN tblStatus s ON s.Id = a.Status");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ',a.OtherNames) AS Names, a.EmailAddress AS Email, a.PhoneNumber, a.AcceptedTerms, s.StatusName Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM ClientSignatories a INNER JOIN RegisteredClients r ON r.Id = a.ClientID INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.OtherNames LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%');");
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
        //POST LoadDeleteClient
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadDeleteClient(int getClientId)
        {
            using (DBModel db = new DBModel())
            {
                var getClientInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == getClientId);

                //Data For View Display
                ViewData["CompanyName"] = getClientInfo.Surname + " " + getClientInfo.Surname;
                ViewData["EmailAddress"] = getClientInfo.EmailAddress;
                ViewData["ClientId"] = getClientInfo.Id;
                ViewData["UserAccountId"] = getClientInfo.UserAccountID;
            }

            return PartialView();
        }

        //
        //Delete ClientSignatories
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteClient(DeleteClientViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var UserEmail = db.AspNetUsers.SingleOrDefault(a => a.Id == userId);

                    var getClientInfo = db.RegisteredClients.SingleOrDefault(c => c.Id == model.ClientId);
                    //Delete From Registered Client Table
                    var newDeletedEntry = db.AuditTrails.Create();
                    newDeletedEntry.EntityId = model.ClientId;
                    newDeletedEntry.ActionType = "Delete";
                    newDeletedEntry.EntityTable = "RegisteredClients, AspNetUsers";
                    newDeletedEntry.EntityUId = model.UserAccountId;
                    newDeletedEntry.DoneBy = UserEmail.Email;
                    newDeletedEntry.DateCreated = DateTime.Now;
                    //newDeletedEntry.EntityName = getClientInfo.Surname + " " + getClientInfo.Surname + " " + getClientInfo.OtherNames;
                    newDeletedEntry.EntityName = getClientInfo.Surname + " " + getClientInfo.OtherNames;
                    newDeletedEntry.EntityEmail = getClientInfo.EmailAddress;
                    //newDeletedEntry.EntityEmail = getClientInfo.EmailAddress + ", " + getClientInfo.BusinessEmailAddress;
                    newDeletedEntry.EntityPhone = getClientInfo.PhoneNumber;
                    db.AuditTrails.Add(newDeletedEntry);
                    var recordDeleted = db.SaveChanges();

                    if (recordDeleted > 0)
                    {
                        db.RegisteredClients.RemoveRange(db.RegisteredClients.Where(r => r.Id == model.ClientId));
                        var deletedClient = db.SaveChanges();
                        if (deletedClient > 0)
                        {
                            //Delete User login Account
                            if (getClientInfo.Status == 1)
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
                    var newDeletedEntry = db.AuditTrails.Create();
                    newDeletedEntry.EntityId = model.ClientId;
                    newDeletedEntry.ActionType = "Delete";
                    newDeletedEntry.EntityTable = "ClientSignatories, AspNetUsers";
                    newDeletedEntry.EntityUId = model.UserAccountId;
                    newDeletedEntry.DoneBy = UserEmail.Email;
                    newDeletedEntry.DateCreated = DateTime.Now;
                    newDeletedEntry.EntityName = getSignatoryInfo.Surname + " " + getSignatoryInfo.OtherNames;
                    newDeletedEntry.EntityEmail = getSignatoryInfo.EmailAddress;
                    newDeletedEntry.EntityPhone = getSignatoryInfo.PhoneNumber;
                    db.AuditTrails.Add(newDeletedEntry);
                    var recordDeleted = db.SaveChanges();

                    if (recordDeleted > 0)
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

                var RegisteredClientInfo = db.RegisteredClients.SingleOrDefault(s => s.Id == SignatoryInfo.ClientID);
                ViewBag.RegisteredClientInfo = RegisteredClientInfo;

                var HasApproval = db.SignatoryApprovals.Any(s => s.SignatoryID == SignatoryInfo.Id);
                ViewBag.ApprovalDetails = HasApproval;
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
                IEnumerable<ClientSignatoriesViewModel> query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ', a.OtherNames) AS Names, a.Email, a.Mobile AS PhoneNumber, a.AcceptedTerms, s.StatusName AS Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM DesignatedUsers a INNER JOIN RegisteredClients r ON r.Id = a.ClientID INNER JOIN tblStatus s ON s.Id = a.Status");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientSignatoriesViewModel>("SELECT a.Id AS SignatoryId, CONCAT(a.Surname,' ', a.OtherNames) AS Names, a.Email, a.Mobile AS PhoneNumber, a.AcceptedTerms, s.StatusName AS Status, r.CompanyName ClientName, a.AcceptedTerms AcceptedTAC, convert(varchar, a.DateCreated, 120) AS DateCreated FROM DesignatedUsers a INNER JOIN RegisteredClients r ON r.Id = a.ClientID INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.Surname LIKE '%" + searchMessage + "%' OR a.Othernames LIKE '%" + searchMessage + "%' OR a.Email LIKE '%" + searchMessage + "%');");
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

                var RegisteredClientInfo = db.RegisteredClients.SingleOrDefault(s => s.Id == representativesDetails.ClientID);
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
                filePath = path + DateTime.Now.ToString("yyyyMMdd") + User.Identity.GetUserId() + Path.GetFileName(FileUpload.FileName);
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
        //
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewUploadedClients()
        {
            return PartialView(GetAllUploadedUsers());
        }

        //Get All Uploaded Users from table
        private IEnumerable<UploadedUsersViewModel> GetAllUploadedUsers()
        {
            using (DBModel db = new DBModel())
            {
                //Query List
                var userid = User.Identity.GetUserId();
                var Query = from a in db.RegisteredClients.Where(r => r.UploadedBy == userid && DbFunctions.DiffMinutes(r.DateCreated, DateTime.Now) <= 1)
                            select new UploadedUsersViewModel
                            {
                                AccountName = a.AccountName,
                                //CompanyName = a.CompanyName,
                                EmailAddress = a.EmailAddress.ToLower(),
                                DateCreated = a.DateCreated
                            };
                return Query.ToList();
            }
        }

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
                filePath = path + DateTime.Now.ToString("yyyyMMdd") + User.Identity.GetUserId() + Path.GetFileName(FileUpload.FileName);
                string extension = Path.GetExtension(FileUpload.FileName);
                FileUpload.SaveAs(filePath);

                //Create a DataTable.
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[10] {
                            new DataColumn("ClientName", typeof(string)),
                            new DataColumn("ClientNo", typeof(string)),
                            new DataColumn("ClientCompanyName", typeof(string)),
                            new DataColumn("ClientCompanyReg", typeof(string)),
                            new DataColumn("ClientBusinessEmailAddress".ToLower(), typeof(string)),
                            new DataColumn("ClientEmailAddress".ToLower(), typeof(string)),
                            new DataColumn("ClientPostalAddress", typeof(string)),
                            new DataColumn("ClientPostalCode", typeof(string)),
                            new DataColumn("ClientTownCity", typeof(string)),
                            new DataColumn("ClientPhoneNumber", typeof(string))
                });
                dt.Columns.Add("Status").DefaultValue = 0;
                dt.Columns.Add("AcceptedTerms").DefaultValue = "True";
                dt.Columns.Add("AcceptedUserTerms").DefaultValue = "False";
                dt.Columns.Add("DateCreated").DefaultValue = DateTime.Now;
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
                        sqlBulkCopy.ColumnMappings.Add("ClientName", "AccountName");
                        sqlBulkCopy.ColumnMappings.Add("ClientNo", "IncorporationNumber");
                        sqlBulkCopy.ColumnMappings.Add("ClientCompanyName", "CompanyName");
                        sqlBulkCopy.ColumnMappings.Add("ClientCompanyReg", "IDRegNumber");
                        sqlBulkCopy.ColumnMappings.Add("ClientBusinessEmailAddress", "BusinessEmailAddress");
                        sqlBulkCopy.ColumnMappings.Add("ClientEmailAddress", "EmailAddress");
                        sqlBulkCopy.ColumnMappings.Add("ClientPostalAddress", "PostalAddress");
                        sqlBulkCopy.ColumnMappings.Add("ClientPostalCode", "PostalCode");
                        sqlBulkCopy.ColumnMappings.Add("ClientTownCity", "CompanyTownCity");
                        sqlBulkCopy.ColumnMappings.Add("ClientPhoneNumber", "PhoneNumber");
                        sqlBulkCopy.ColumnMappings.Add("Status", "Status");
                        sqlBulkCopy.ColumnMappings.Add("AcceptedTerms", "AcceptedTerms");
                        sqlBulkCopy.ColumnMappings.Add("AcceptedUserTerms", "AcceptedUserTerms");
                        sqlBulkCopy.ColumnMappings.Add("DateCreated", "DateCreated");
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
        // GET: Admin Get/Edit Uploaded Clients
        public ActionResult EditUploadedClients()
        {
            return PartialView(GetUploadedClients());
        }

        //Get Registered Clients List
        public IEnumerable<UploadedClientsViewModel> GetUploadedClients()
        {
            using (DBModel db = new DBModel())
            {
                //Query List
                var userid = User.Identity.GetUserId();
                //Get Clients with completed profiles
                var CompletedClients = db.EMarketApplications.Select(t => t.ClientID).ToList();
                var Query = from a in db.RegisteredClients.Where(r => r.UploadedBy == userid && !CompletedClients.Contains(r.Id))
                            join b in db.tblStatus on a.Status equals b.Id
                            select new UploadedClientsViewModel
                            {
                                ClientID = a.Id,
                                //CompanyName = a.CompanyName,
                                CompanyRegistration = a.IDRegNumber,
                                PhoneNumber = a.PhoneNumber,
                                Status = b.StatusName,
                                AcceptedTAC = a.AcceptedTerms,
                                EmailAddress = a.EmailAddress.ToLower(),
                                DateCreated = a.DateCreated
                            };
                return Query.OrderByDescending(x => x.ClientID).ToList();
            }
        }

        //POST EditClient
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult EditClient(int clientId)
        {
            using (DBModel db = new DBModel())
            {
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientId);
                ViewBag.ClientInfo = clientDetails;

            }

            return PartialView();
        }

        // POST: EditClientDetails
        [HttpPost]
        [AllowAnonymous]
        public ActionResult PostEditClientDetails(EditUploadedClientViewModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            //Log SSI Value if settlement account exist
            SSI = (model.HaveSettlementAccount == "Yes") ? true : false;

            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //Update Client Details with provided form values
                    var updateClient = db.RegisteredClients.SingleOrDefault(c => c.Id == model.ClientID);
                    if (updateClient != null)
                    {
                        try
                        {
                            //Update Registered Client Details
                            //updateClient.CompanyName = model.CompanyName;
                            updateClient.IDRegNumber = model.CompanyRegistration;
                            //updateClient.CompanyTownCity = model.CompanyTownCity;
                            //updateClient.Building = model.Building;
                            //updateClient.Street = model.Street;
                            updateClient.PostalAddress = model.PostalAddress;
                            updateClient.PostalCode = model.PostalCode;
                            //updateClient.BusinessEmailAddress = model.BusinessEmailAddress;
                            updateClient.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        return Json("Unable to Update Client Details!", JsonRequestBehavior.AllowGet);
                    }

                    //Log Settlement Account Details if yes is selected
                    if (model.HaveSettlementAccount == "Yes")
                    {
                        var newAccountDetails = db.ClientSettlementAccounts.Create();
                        if (model.SettlementAccount1 != null || model.InputCurrencyType1 != null)
                        {
                            var SettlementAccount1Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount1);
                            if (!SettlementAccount1Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount1;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType1;
                                newAccountDetails.CurrencyID = model.SelectCurrency1;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate1 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount1);
                                SettlementAccountUpdate1.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount2 != null || model.InputCurrencyType2 != null)
                        {
                            var SettlementAccount2Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount2);
                            if (!SettlementAccount2Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount2;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType2;
                                newAccountDetails.CurrencyID = model.SelectCurrency2;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate2 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount2);
                                SettlementAccountUpdate2.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount3 != null || model.InputCurrencyType3 != null)
                        {
                            var SettlementAccount3Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount3);
                            if (!SettlementAccount3Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount3;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType3;
                                newAccountDetails.CurrencyID = model.SelectCurrency3;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate3 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount3);
                                SettlementAccountUpdate3.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount4 != null || model.InputCurrencyType4 != null)
                        {
                            var SettlementAccount4Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount4);
                            if (!SettlementAccount4Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount4;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType4;
                                newAccountDetails.CurrencyID = model.SelectCurrency4;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate4 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount4);
                                SettlementAccountUpdate4.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount5 != null || model.InputCurrencyType5 != null)
                        {
                            var SettlementAccount5Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount5);
                            if (!SettlementAccount5Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount5;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType5;
                                newAccountDetails.CurrencyID = model.SelectCurrency5;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate5 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount5);
                                SettlementAccountUpdate5.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount6 != null || model.InputCurrencyType6 != null)
                        {
                            var SettlementAccount6Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount6);
                            if (!SettlementAccount6Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount6;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType6;
                                newAccountDetails.CurrencyID = model.SelectCurrency6;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate6 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount6);
                                SettlementAccountUpdate6.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount7 != null || model.InputCurrencyType7 != null)
                        {
                            var SettlementAccount7Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount7);
                            if (!SettlementAccount7Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount7;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType7;
                                newAccountDetails.CurrencyID = model.SelectCurrency7;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate7 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount7);
                                SettlementAccountUpdate7.Status = 1;
                                db.SaveChanges();
                            }
                        }
                        if (model.SettlementAccount8 != null || model.InputCurrencyType8 != null)
                        {
                            var SettlementAccount8Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount8);
                            if (!SettlementAccount8Exists)
                            {
                                newAccountDetails.ClientID = model.ClientID;
                                newAccountDetails.AccountNumber = model.SettlementAccount8;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType8;
                                newAccountDetails.CurrencyID = model.SelectCurrency8;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            else
                            {
                                var SettlementAccountUpdate6 = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == model.SettlementAccount6);
                                SettlementAccountUpdate6.Status = 1;
                                db.SaveChanges();
                            }
                        }
                    }

                    //Create Account Signatory 1
                    if (model.SignatorySurname1 != null || model.SignatoryOtherNames1 != null || model.SignatoryEmail1 != null || model.SignatoryDesignation1 != null)
                    {
                        //check if exists and add is false
                        var SignatoryExists = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail1);
                        if (SignatoryExists == null)
                        {
                            try
                            {
                                var addSignatory1 = db.ClientSignatories.Create();
                                addSignatory1.ClientID = model.ClientID;
                                addSignatory1.Designation = model.SignatoryDesignation1;
                                addSignatory1.Surname = model.SignatorySurname1;
                                addSignatory1.OtherNames = model.SignatoryOtherNames1;
                                addSignatory1.EmailAddress = model.SignatoryEmail1.ToLower();
                                addSignatory1.PhoneNumber = model.SignatoryPhoneNumber1;
                                addSignatory1.DateCreated = DateTime.Now;
                                addSignatory1.Status = 1;
                                addSignatory1.AcceptedTerms = true;
                                db.ClientSignatories.Add(addSignatory1);
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                        else
                        {
                            var Signatory1ToUpdateStatus = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1);
                            Signatory1ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                    }

                    //Create Account Signatory 2
                    if (model.SignatorySurname2 != null || model.SignatoryOtherNames2 != null || model.SignatoryEmail2 != null || model.SignatoryDesignation2 != null)
                    {
                        //Check if Signatory already exists
                        var Signatory2Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail2);
                        if (Signatory2Exist)
                        {
                            //Update Status if signatory exists
                            var Signatory2ToUpdateStatus = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail2);
                            Signatory2ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                        else
                        {
                            //Create Signatory 2
                            try
                            {
                                var addSignatory2 = db.ClientSignatories.Create();
                                addSignatory2.ClientID = model.ClientID;
                                addSignatory2.Designation = model.SignatoryDesignation2;
                                addSignatory2.Surname = model.SignatorySurname2;
                                addSignatory2.OtherNames = model.SignatoryOtherNames2;
                                addSignatory2.EmailAddress = model.SignatoryEmail2.ToLower();
                                addSignatory2.PhoneNumber = model.SignatoryPhoneNumber2;
                                addSignatory2.DateCreated = DateTime.Now;
                                addSignatory2.Status = 0;
                                db.ClientSignatories.Add(addSignatory2);
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                    }

                    //Create Signatory and Send email notification and OTP (Signatory 3)
                    if (model.SignatorySurname3 != null || model.SignatoryOtherNames3 != null || model.SignatoryEmail3 != null || model.SignatoryDesignation3 != null)
                    {
                        //Check if Signatory already exists
                        var Signatory3Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail3);
                        if (Signatory3Exist)
                        {
                            //Update Status if signatory exists
                            var Signatory3ToUpdateStatus = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail3);
                            Signatory3ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                        else
                        {
                            try
                            {
                                var addSignatory3 = db.ClientSignatories.Create();
                                addSignatory3.ClientID = model.ClientID;
                                addSignatory3.Designation = model.SignatoryDesignation3;
                                addSignatory3.Surname = model.SignatorySurname3;
                                addSignatory3.OtherNames = model.SignatoryOtherNames3;
                                addSignatory3.EmailAddress = model.SignatoryEmail3.ToLower();
                                addSignatory3.PhoneNumber = model.SignatoryPhoneNumber3;
                                addSignatory3.DateCreated = DateTime.Now;
                                addSignatory3.Status = 0;
                                db.ClientSignatories.Add(addSignatory3);
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                    }

                    //Create Account Signatory 4
                    if (model.SignatorySurname4 != null || model.SignatoryOtherNames4 != null || model.SignatoryEmail4 != null || model.SignatoryDesignation4 != null)
                    {
                        //Check if Signatory already exists
                        var Signatory4Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail4);
                        if (Signatory4Exist)
                        {
                            //Update Status if signatory exists
                            var Signatory4ToUpdateStatus = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail4);
                            Signatory4ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                        else
                        {
                            try
                            {
                                var addSignatory4 = db.ClientSignatories.Create();
                                addSignatory4.ClientID = model.ClientID;
                                addSignatory4.Designation = model.SignatoryDesignation4;
                                addSignatory4.Surname = model.SignatorySurname4;
                                addSignatory4.OtherNames = model.SignatoryOtherNames4;
                                addSignatory4.EmailAddress = model.SignatoryEmail4.ToLower();
                                addSignatory4.PhoneNumber = model.SignatoryPhoneNumber4;
                                addSignatory4.DateCreated = DateTime.Now;
                                addSignatory4.Status = 0;
                                db.ClientSignatories.Add(addSignatory4);
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                    }

                    //Create Signatory and Send email notification And OTP (Signatory 5)
                    if (model.SignatorySurname5 != null || model.SignatoryOtherNames5 != null || model.SignatoryEmail5 != null || model.SignatoryDesignation5 != null)
                    {
                        //Check if Signatory already exists
                        var Signatory5Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail5);
                        if (Signatory5Exist)
                        {
                            //Update Status if signatory exists
                            var Signatory5ToUpdateStatus = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail5);
                            Signatory5ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                        else
                        {
                            try
                            {
                                var addSignatory5 = db.ClientSignatories.Create();
                                addSignatory5.ClientID = model.ClientID;
                                addSignatory5.Designation = model.SignatoryDesignation5;
                                addSignatory5.Surname = model.SignatorySurname5;
                                addSignatory5.OtherNames = model.SignatoryOtherNames5;
                                addSignatory5.EmailAddress = model.SignatoryEmail5.ToLower();
                                addSignatory5.PhoneNumber = model.SignatoryPhoneNumber5;
                                addSignatory5.DateCreated = DateTime.Now;
                                addSignatory5.Status = 0;
                                db.ClientSignatories.Add(addSignatory5);
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                    }

                    //Log authorised representative (Representative 1)
                    if (model.UserEmail1 != null && model.UserSurname1 != null && model.UserOthernames1 != null && model.UserMobileNumber1 != null)
                    {
                        //Check if representative exists
                        var Representative1Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail1);
                        if (Representative1Exists == null)
                        {
                            //Create new representative if false
                            try
                            {
                                var addDesignatedUser1 = db.DesignatedUsers.Create();
                                addDesignatedUser1.Status = 0;
                                addDesignatedUser1.ClientID = model.ClientID;
                                addDesignatedUser1.DateCreated = DateTime.Now;
                                addDesignatedUser1.DOB = model.DOB1;
                                addDesignatedUser1.Email = model.UserEmail1.ToLower();
                                addDesignatedUser1.Surname = model.UserSurname1;
                                addDesignatedUser1.Othernames = model.UserOthernames1;
                                addDesignatedUser1.Mobile = model.UserMobileNumber1;
                                addDesignatedUser1.POBox = model.UserPOBox1;
                                addDesignatedUser1.ZipCode = model.UserZipCode1;
                                addDesignatedUser1.Town = model.UserTownCity1;
                                addDesignatedUser1.TradingLimit = model.TransactionLimit1;
                                addDesignatedUser1.EMarketSignUp = model.EMarketSignUp1;
                                addDesignatedUser1.AcceptedTerms = false;
                                db.DesignatedUsers.Add(addDesignatedUser1);
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                        else
                        {
                            //Update Status if representative exists
                            var Representative1ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail1);
                            Representative1ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                    }

                    //Log authorised Representative 2
                    if (model.UserEmail2 != null && model.UserSurname2 != null && model.UserOthernames2 != null && model.UserMobileNumber2 != null)
                    {
                        var Representative2Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail2);
                        var RepresentativeIsAClient = db.RegisteredClients.SingleOrDefault(c => c.EmailAddress == model.UserEmail2);

                        if (Representative2Exists == null)
                        {
                            var addDesignatedUser2 = db.DesignatedUsers.Create();
                            addDesignatedUser2.Status = 0;
                            addDesignatedUser2.ClientID = model.ClientID;
                            addDesignatedUser2.DateCreated = DateTime.Now;
                            addDesignatedUser2.DOB = model.DOB2;
                            addDesignatedUser2.Email = model.UserEmail2.ToLower();
                            addDesignatedUser2.Surname = model.UserSurname2;
                            addDesignatedUser2.Othernames = model.UserOthernames2;
                            addDesignatedUser2.Mobile = model.UserMobileNumber2;
                            addDesignatedUser2.POBox = model.UserPOBox2;
                            addDesignatedUser2.ZipCode = model.UserZipCode2;
                            addDesignatedUser2.Town = model.UserTownCity2;
                            addDesignatedUser2.TradingLimit = model.TransactionLimit2;
                            addDesignatedUser2.EMarketSignUp = model.EMarketSignUp2;
                            addDesignatedUser2.AcceptedTerms = false;
                            db.DesignatedUsers.Add(addDesignatedUser2);
                            db.SaveChanges();
                        }
                        else
                        {
                            //Update Status if representative exists
                            var Representative2ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail2);
                            Representative2ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                    }

                    //Log authorised Representative 3
                    if (model.UserEmail3 != null && model.UserSurname3 != null && model.UserOthernames3 != null && model.UserMobileNumber3 != null)
                    {
                        var Representative3Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail3);
                        if (Representative3Exists == null)
                        {
                            var addDesignatedUser3 = db.DesignatedUsers.Create();
                            addDesignatedUser3.Status = 0;
                            addDesignatedUser3.ClientID = model.ClientID;
                            addDesignatedUser3.DateCreated = DateTime.Now;
                            addDesignatedUser3.DOB = model.DOB3;
                            addDesignatedUser3.Email = model.UserEmail3.ToLower();
                            addDesignatedUser3.Surname = model.UserSurname3;
                            addDesignatedUser3.Othernames = model.UserOthernames3;
                            addDesignatedUser3.Mobile = model.UserMobileNumber3;
                            addDesignatedUser3.POBox = model.UserPOBox3;
                            addDesignatedUser3.ZipCode = model.UserZipCode3;
                            addDesignatedUser3.Town = model.UserTownCity3;
                            addDesignatedUser3.TradingLimit = model.TransactionLimit3;
                            addDesignatedUser3.EMarketSignUp = model.EMarketSignUp3;
                            addDesignatedUser3.AcceptedTerms = false;
                            db.DesignatedUsers.Add(addDesignatedUser3);
                            db.SaveChanges();
                        }
                        else
                        {
                            //Update Status if representative exists
                            var Representative3ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail3);
                            Representative3ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                    }

                    //Log authorised Representative 4
                    if (model.UserEmail4 != null && model.UserSurname4 != null && model.UserOthernames4 != null && model.UserMobileNumber4 != null)
                    {
                        var Representative4Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail4);
                        if (Representative4Exists == null)
                        {
                            var addDesignatedUser4 = db.DesignatedUsers.Create();
                            addDesignatedUser4.Status = 0;
                            addDesignatedUser4.ClientID = model.ClientID;
                            addDesignatedUser4.DateCreated = DateTime.Now;
                            addDesignatedUser4.DOB = model.DOB4;
                            addDesignatedUser4.Email = model.UserEmail4.ToLower();
                            addDesignatedUser4.Surname = model.UserSurname4;
                            addDesignatedUser4.Othernames = model.UserOthernames4;
                            addDesignatedUser4.Mobile = model.UserMobileNumber4;
                            addDesignatedUser4.POBox = model.UserPOBox4;
                            addDesignatedUser4.Town = model.UserTownCity4;
                            addDesignatedUser4.ZipCode = model.UserZipCode4;
                            addDesignatedUser4.AcceptedTerms = false;
                            addDesignatedUser4.TradingLimit = model.TransactionLimit4;
                            addDesignatedUser4.EMarketSignUp = model.EMarketSignUp4;
                            db.DesignatedUsers.Add(addDesignatedUser4);
                            db.SaveChanges();
                        }
                        else
                        {
                            //Update Status if representative exists
                            var Representative4ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail4);
                            Representative4ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                    }

                    //Log authorised Representative 5
                    if (model.UserEmail5 != null && model.UserSurname5 != null && model.UserOthernames5 != null && model.UserMobileNumber5 != null)
                    {
                        var Representative5Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail5);
                        if (Representative5Exists == null)
                        {
                            var addDesignatedUser5 = db.DesignatedUsers.Create();
                            addDesignatedUser5.Status = 0;
                            addDesignatedUser5.ClientID = model.ClientID;
                            addDesignatedUser5.DateCreated = DateTime.Now;
                            addDesignatedUser5.DOB = model.DOB5;
                            addDesignatedUser5.Email = model.UserEmail5.ToLower();
                            addDesignatedUser5.Surname = model.UserSurname5;
                            addDesignatedUser5.Othernames = model.UserOthernames5;
                            addDesignatedUser5.Mobile = model.UserMobileNumber5;
                            addDesignatedUser5.POBox = model.UserPOBox5;
                            addDesignatedUser5.Town = model.UserTownCity5;
                            addDesignatedUser5.ZipCode = model.UserZipCode5;
                            addDesignatedUser5.TradingLimit = model.TransactionLimit5;
                            addDesignatedUser5.EMarketSignUp = model.EMarketSignUp5;
                            addDesignatedUser5.AcceptedTerms = false;
                            db.DesignatedUsers.Add(addDesignatedUser5);
                            db.SaveChanges();
                        }
                        else
                        {
                            //Update Status if representative exists
                            var Representative5ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail5);
                            Representative5ToUpdateStatus.Status = 1;
                            db.SaveChanges();
                        }
                    }

                    //Update Number of Application Signatories and Designated Users
                    var SignatoryCount = db.ClientSignatories.Count(c => c.ClientID == model.ClientID && c.Status != 2);
                    var DesignatedUsersCount = db.DesignatedUsers.Count(c => c.ClientID == model.ClientID && c.Status != 2);
                    try
                    {
                        //Log new application
                        var newApplication = db.EMarketApplications.Create();
                        newApplication.AcceptedTAC = true;
                        newApplication.ClientID = model.ClientID;
                        newApplication.DesignatedUsers = DesignatedUsersCount;
                        newApplication.DateCreated = DateTime.Now;
                        newApplication.Signatories = SignatoryCount;
                        newApplication.Status = 1;
                        newApplication.SignatoriesApproved = SignatoryCount;
                        newApplication.UsersApproved = DesignatedUsersCount;
                        newApplication.SignatoriesDateApproved = DateTime.Now;
                        newApplication.UsersDateApproved = DateTime.Now;
                        newApplication.Emt = true;
                        newApplication.SSI = SSI;
                        newApplication.OPSApproved = true;
                        newApplication.OPSDateApproved = DateTime.Now;
                        newApplication.OPSWhoApproved = User.Identity.GetUserId();
                        newApplication.OPSComments = "Uploaded Application";
                        newApplication.POAApproved = true;
                        newApplication.POADateApproved = DateTime.Now;
                        newApplication.POAWhoApproved = User.Identity.GetUserId();
                        newApplication.POAComments = "Uploaded Application";
                        db.EMarketApplications.Add(newApplication);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Model Invalid " + errors + " ", JsonRequestBehavior.AllowGet);
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
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE));").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%');").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 0 AND s.OPSDeclined = 0 AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE);").First();
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
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved) AND s.OPSApproved = 0 AND s.OPSDeclined = 0;";
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
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved) AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE);").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE));").First();
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
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, b.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, b.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND (s.Signatories <> s.SignatoriesApproved OR s.DesignatedUsers <> s.UsersApproved)";
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
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                }
                else
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;").First();
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
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, b.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, b.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1;";
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
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
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
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 4;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 4;)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 4;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, b.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, b.EmailAddress, c.StatusName Status, s.Emt, s.SSI, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 4;";
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
                    var _action = "DeclineApplication";
                    if (ApplicationUpdate != null)
                    {
                        try
                        {
                            //Update application status
                            ApplicationUpdate.DigitalDeskDeclined = true;
                            ApplicationUpdate.DateDeclined = DateTime.Now;
                            ApplicationUpdate.DigWhoDeclined = currentUserId;
                            ApplicationUpdate.Status = 4;
                            ApplicationUpdate.OPSComments = model.Comments;
                            db.SaveChanges();

                            //Mark HasApplication False for Client Company
                            var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == ApplicationUpdate.CompanyID);
                            updateClientCompany.HasApplication = false;
                            db.SaveChanges();

                            //Delete Signatories
                            db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(c => c.ClientID == ClientDetails.Id));
                            db.SaveChanges();

                            /*var newDeletedEntry = db.DeletedEntities.Create();
                            newDeletedEntry.EntityId = ClientDetails.Id;
                            newDeletedEntry.EntityTable = "ClientSignatories";
                            newDeletedEntry.EntityUId = ClientDetails.UserAccountID;
                            newDeletedEntry.DeletedBy = currentUserId;
                            newDeletedEntry.DateDeleted = DateTime.Now;
                            newDeletedEntry.EntityName = ClientDetails.Surname + " " + ClientDetails.OtherNames;
                            newDeletedEntry.EntityEmail = ClientDetails.EmailAddress;
                            newDeletedEntry.EntityPhone = ClientDetails.PhoneNumber;
                            db.DeletedEntities.Add(newDeletedEntry);
                            var recordDeleted = db.SaveChanges();
                            if (recordDeleted > 0)
                            {
                                db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(c => c.ClientID == ClientDetails.Id));
                                db.SaveChanges();
                            }
                            else
                            {
                                return Json("Unable to delete signatory details!", JsonRequestBehavior.AllowGet);
                            }*/

                            //Delete Representatives
                            db.DesignatedUsers.RemoveRange(db.DesignatedUsers.Where(c => c.ClientID == ClientDetails.Id));
                            db.SaveChanges();

                            //Delete Settlement Accounts
                            db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(c => c.ClientID == ClientDetails.Id));
                            var recordSaved = db.SaveChanges();

                            //Send email if true
                            if (recordSaved > 0)
                            {
                                //Send email notification to Client
                                var ApplicationApprovedEmailMessage = "Dear " + ClientDetails.Surname + ", <br/><br/> Your application has been declined. <br/>" +
                                "Kindly login to submit another application.<br/><br/>" +
                                "Kind Regards, <br /> Global Markets Digital Team <br/><img src=\"/Content/images/EmailSignature.png\"/>";
                                var ApplicationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, ClientDetails.EmailAddress, "Application Declined", ApplicationApprovedEmailMessage);

                                if (ApplicationCompleteEmail == true)
                                {
                                    //Log email sent notification
                                    LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApplicationApprovedEmailMessage, ClientDetails.EmailAddress, _action);
                                }
                                else
                                {
                                    //Log Email failed notification
                                    LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApplicationApprovedEmailMessage, ClientDetails.EmailAddress, _action);
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
                        return Json("Unable to update application details!", JsonRequestBehavior.AllowGet);
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
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) AS COUNT FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1 AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
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
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND s.Status = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (b.CompanyName LIKE '%" + searchString + "%' OR b.EmailAddress LIKE '%" + searchString + "%') AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND (s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1;)";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND s.Status = 1;";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1;";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT b.CompanyName, c.StatusName Status, s.AcceptedTAC, CAST(s.DateCreated AS DATE) DateCreated, s.OPSApproved, s.POAApproved FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND (s.OPSApproved = 0 OR s.POAApproved = 0) AND s.Status = 1;";
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
                IEnumerable<ExpiredOtpsViewModel> query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID,  a.CompanyName, s.StatusName Status, a.DateCreated, a.EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM RegisteredClients a INNER JOIN tblStatus s ON s.Id = a.Status " +
                  "WHERE DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.ClientOTPExpiry + "' AND a.Status = 0;");

                //Search 
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ExpiredOtpsViewModel>("SELECT a.Id as ClientID,  a.CompanyName, s.StatusName Status, a.DateCreated, a.EmailAddress, DATEDIFF(Hour,a.DateCreated, GETDATE()) TimeExpired FROM RegisteredClients a INNER JOIN tblStatus s ON s.Id = a.Status WHERE (a.CompanyName LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND DATEDIFF(Hour,a.DateCreated, GETDATE()) > '" + Properties.Settings.Default.ClientOTPExpiry + "' AND a.Status = 0;");
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
        public List<ExpiredOtpsViewModel> GetSignatoriesExpiredOTPList(string searchMessage, int jtStartIndex, int count, string jtSorting)
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
                var ActionPage = "UnlockUser";
                var recordSaved = db.SaveChanges();

                if (recordSaved > 0)
                {
                    //Send email notification to user
                    var callbackUrl = Url.Action("ResetAccount", "Account", null, Request.Url.Scheme);
                    var ApplicationApprovedEmailMessage = "Dear " + getUserToUnlock.CompanyName + ", <br/><br/> Your account has been unlocked. <br/>" +
                    "<a href=" + callbackUrl + "> Click here to reset your password. </a> <br/><br/>" +
                    "Kind Regards, <br /> Global Markets Digital Team <br/><img src=\"/Content/images/EmailSignature.png\"/>";
                    var ApplicationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, getUserToUnlock.Email, "Account Unlocked", ApplicationApprovedEmailMessage);

                    if (ApplicationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApplicationApprovedEmailMessage, getUserToUnlock.Email, ActionPage);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApplicationApprovedEmailMessage, getUserToUnlock.Email, ActionPage);
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
                IEnumerable<ClientApplicationsViewModel> query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.OPSDeclined = 0 AND s.POAApproved = 0 AND s.POADeclined = 0 AND s.Status = 1 ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client,  c.StatusName Status, s.Emt, s.SSI, s.DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.OPSDeclined = 0 AND s.POAApproved = 0 AND s.POADeclined = 0 AND s.Status = 1 (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

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
                    var recordCount = db.Database.SqlQuery<int>("SELECT COUNT(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.Signatories = s.SignatoriesApproved AND s.DesignatedUsers = s.UsersApproved AND s.OPSApproved = 1 AND s.OPSDeclined = 0 AND s.POAApproved = 0 AND s.POADeclined = 0 AND s.Status = 1 (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
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
                //Ops Approved
                ViewData["OPSNames"] = OpsApproved.CompanyName;
                ViewData["OPSEmail"] = OpsApproved.Email;
                ViewData["OPSPhone"] = OpsApproved.PhoneNumber;
                DateTime dtByUser = DateTime.Parse(getApplicationInfo.OPSDateApproved.ToString());
                ViewData["OPSDateApproved"] = dtByUser.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
            return PartialView();
        }
    }
}
﻿using Microsoft.AspNet.Identity;
using OnBoarding.Models;
using OnBoarding.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace OnBoarding.Controllers
{
    [Authorize]
    public class GmoController : Controller
    {
        // GET: 
        // GMO //Index
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var RegisteredClients = db.RegisteredClients.Count();
                var CompletedApplications = db.EMarketApplications.Count(a => a.Signatories == a.SignatoriesApproved && a.DesignatedUsers == a.UsersApproved && a.Status == 1 && a.OPSApproved == true && a.POAApproved == true);
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
        public int GetApprovedApplicationsCount()
        {
            using (var db = new DBModel())
            {
                return db.EMarketApplications.Where(a => a.OPSApproved == true && a.POAApproved == true && a.Status == 1).Count();
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
                    query = db.Database.SqlQuery<ClientApplicationsViewModel>("SELECT s.Id ApplicationID, b.CompanyName Client, c.StatusName Status, s.Emt, s.SSI, CAST(s.DateCreated AS DATE) DateCreated, s.Signatories, s.DesignatedUsers, s.SignatoriesApproved, s.UsersApproved RepresentativesApproved, s.OPSApproved, s.POAApproved, s.AcceptedTAC FROM EMarketApplications s INNER JOIN ClientCompanies b on b.Id = s.CompanyID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%') ORDER BY s.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

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
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(s.Id) count FROM EMarketApplications s INNER JOIN RegisteredClients b on b.Id = s.ClientID INNER JOIN tblStatus c on c.Id = s.Status WHERE s.Status = 1 AND s.OPSApproved = 1 AND s.POAApproved = 1 AND (b.CompanyName LIKE '%" + searchMessage + "%' OR b.EmailAddress LIKE '%" + searchMessage + "%')").First();
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
                ViewData["POANames"] = OpsApproved.CompanyName;
                ViewData["POAEmail"] = OpsApproved.Email;
                ViewData["POAPhone"] = OpsApproved.PhoneNumber;
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
        //Get InCompleteApplications
        public ActionResult IncompleteApplications()
        {
            return View();
        }

        //
        //Partial view InCompleteApplications
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult ViewIncompleteApplication(int applicationId)
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
        //Get InCompleteApplications
        public ActionResult PendingOpsApproval()
        {
            return View();
        }

        //
        //PendingPoaApproval
        public ActionResult PendingPoaApproval()
        {
            return View();
        }

        //
        //RegisteredClients
        public ActionResult RegisteredClients()
        {
            return View();
        }

        //
        //GET //ExportSearchResults
        [HttpGet]
        public void ExportRegisteredClients(string searchText, string DateFrom, string DateTo)
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
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchString + "%' OR n.EmailAddress LIKE '%" + searchString + "%') AND (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND n.Status = 1";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchString + "%' OR n.EmailAddress LIKE '%" + searchString + "%') AND n.Status = 1";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchString + "%' OR n.EmailAddress LIKE '%" + searchString + "%') AND n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.Status = 1";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE (n.CompanyName LIKE '%" + searchString + "%' OR n.EmailAddress LIKE '%" + searchString + "%') AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND n.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE (n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND n.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE n.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND n.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE n.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND n.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE n.Status = 1";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT n.CompanyName, n.AccountNumber,  n.IDRegNumber CompanyRegistration, n.EmailAddress, n.BusinessEmailAddress, n.PostalAddress, n.PostalCode, n.DateCreated FROM RegisteredClients n WHERE n.Status = 1";
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
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_RegisteredClients.csv");
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
        // POST: /ViewClient
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
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientID && a.CompanyID == companyDetails.Id).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientID && a.CompanyID == companyDetails.Id).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + clientID + "'" + " AND s.CompanyID =  " + "'" + companyDetails.Id + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                var clientHasApplication = db.EMarketApplications.Any(s => s.ClientID == clientID);
                ViewBag.clientHasApplication = clientHasApplication;
            }

            return PartialView();
        }

        //
        //Get //ClientSettlements
        public ActionResult ClientSettlements()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetClientSettlementsCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.ClientSettlementAccounts.Where(x => x.Status == 1).Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<ClientSettlementsViewModel> GetClientSettlementsList(string searchMessage, string searchFromDate, string searchToDate, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (DBModel db = new DBModel())
            {
                IEnumerable<ClientSettlementsViewModel> query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (r.CompanyName LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 AND (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID (r.CompanyName LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (r.CompanyName LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 AND c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID (r.CompanyName LIKE '%" + searchMessage + "%' OR a.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 AND c.DateCreated <= CAST('" + searchFromDate + "' AS DATE) ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 AND c.DateCreated <= CAST('" + searchFromDate + "' AS DATE) ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 AND c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    query = db.Database.SqlQuery<ClientSettlementsViewModel>("SELECT c.Id, c.CurrencyID, c.AccountNumber, c.Status, r.CompanyName, a.EmailAddress, r.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients a ON a.Id = c.ClientID INNER JOIN ClientCompanies r ON r.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 AND (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE)) ORDER BY c.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }

                //Sorting Ascending and Descending  
                if (string.IsNullOrEmpty(jtSorting) || jtSorting.Equals("CompanyName ASC"))
                {
                    query = query.OrderBy(p => p.CompanyName);
                }
                else if (jtSorting.Equals("CompanyName DESC"))
                {
                    query = query.OrderByDescending(p => p.CompanyName);
                }
                else if (jtSorting.Equals("DateCreated ASC"))
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
        public JsonResult GetClientSettlements(string searchMessage = "", string searchFromDate = "", string searchToDate = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetClientSettlementsList(searchMessage, searchFromDate, searchToDate, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (r.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 AND (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE))").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (r.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (r.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 AND c.DateCreated >= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (!string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (r.CompanyName LIKE '%" + searchMessage + "%' OR r.EmailAddress LIKE '%" + searchMessage + "%') AND c.Status = 1 AND c.DateCreated <= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 AND c.DateCreated <= CAST('" + searchFromDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 AND c.DateCreated >= CAST('" + searchFromDate + "' AS DATE").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE c.Status = 1 AND (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE)").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else if (string.IsNullOrEmpty(searchMessage) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(c.Id) AS COUNT FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID WHERE (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND c.Status = 1;").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetClientSettlementsCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        //GET //Gets Currencies
        [HttpPost]
        public JsonResult GetCurrencyOptions()
        {
            using (DBModel db = new DBModel())
            {
                var currencies = db.Currencies.ToList();
                var data = from currency in currencies
                           select new
                           {
                               DisplayText = currency.CurrencyName,
                               Value = currency.Id,
                           };
                return Json(new { Result = "OK", Options = data });
            }
        }

        //GET //ExportClientSettlements
        [HttpGet]
        public void ExportClientSettlements(string searchText, string DateFrom, string DateTo)
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
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE (a.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND c.Status = 1";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE (a.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND c.Status = 1";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE (a.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.Status = 1";
                    query = customquery;
                }
                else if (!string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE (a.CompanyName LIKE '%" + searchString + "%' OR r.EmailAddress LIKE '%" + searchString + "%') AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND c.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE (c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.DateCreated <= CAST('" + searchToDate + "' AS DATE)) AND c.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && !string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE c.DateCreated <= CAST('" + searchToDate + "' AS DATE) AND c.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE c.DateCreated >= CAST('" + searchFromDate + "' AS DATE) AND c.Status = 1";
                    query = customquery;
                }
                else if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(searchFromDate) && string.IsNullOrEmpty(searchToDate))
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE c.Status = 1";
                    query = customquery;
                }
                else
                {
                    var customquery = "SELECT c.AccountNumber, s.StatusName, a.CompanyName, r.EmailAddress, a.BusinessEmailAddress, CASE WHEN e.SSI = 1 THEN 'Yes' ELSE 'No' END AS SSI, CASE WHEN e.Emt = 1 THEN 'Yes' ELSE 'No' END AS EmarketSignUp, c.DateCreated FROM ClientSettlementAccounts c INNER JOIN RegisteredClients r ON r.Id = c.ClientID INNER JOIN ClientCompanies a ON a.Id = c.CompanyID INNER JOIN EMarketApplications e ON e.ClientID = c.ClientID and e.CompanyID = c.CompanyID INNER JOIN tblStatus s ON s.Id = c.Status WHERE c.Status = 1";
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
                            Response.AddHeader("content-disposition", "attachment;filename=GMOnBoarding_ClientSettlements.csv");
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
        //Get: //ApproveUploads
        public ActionResult ApproveUploads()
        {
            return View();
        }

        //
        //Get //Count
        public int GetUploadedClientsCount()
        {
            using (DBModel db = new DBModel())
            {
                return db.ExistingClientsUploads.Count();
            }
        }

        //
        //GET /Get Currencies List
        public List<ExistingClientsUploadViewModel> GetUploadedClientsList(int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext
            using (DBModel db = new DBModel())
            {
                IEnumerable<ExistingClientsUploadViewModel> query = db.Database.SqlQuery<ExistingClientsUploadViewModel>("SELECT a.[FileName], u.CompanyName UploadedBy, a.Status, a.DateCreated FROM ExistingClientsUploads a INNER JOIN AspNetUsers u ON u.Id = a.UploadedBy WHERE a.UploadedBy <> '" + User.Identity.GetUserId() + "' GROUP BY u.CompanyName, a.[FileName], a.Status, a.DateCreated ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

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
                var Query = db.Database.SqlQuery<ExistingClientsUpload>("SELECT n.* FROM ExistingClientsUploads n WHERE n.[FileName] = " + "'" + fileName + "'" + " AND n.Status = 0 ORDER BY " + jtSorting + " OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
                var approvals = Query.ToList();
                return Json(new { Result = "OK", Records = approvals, TotalRecordCount = recordCount });
            }
        }

        //
        //Approve records
        [HttpPost]
        public JsonResult ApproveSelected(int Id)
        {
            using (var db = new DBModel())
            {
                try
                {
                    //Update record as approved
                    var RecordToUpdate = db.ExistingClientsUploads.SingleOrDefault(c => c.Id == Id);
                    var clientId = 0;
                    var companyId = 0;

                    //1. Check if accountnumber exist
                    var clientExists = db.RegisteredClients.SingleOrDefault(c => c.EmailAddress == RecordToUpdate.CompanyEmail && c.Status == 1);
                    if (clientExists == null)
                    {
                        //1. Create new entry
                        try
                        {
                            var AcceptedTerms = (RecordToUpdate.AcceptedTerms == "YES" || RecordToUpdate.AcceptedTerms == "Yes" || RecordToUpdate.AcceptedTerms == "yes") ? true : false;
                            var newRegisteredClient = db.RegisteredClients.Create();
                            newRegisteredClient.Surname = RecordToUpdate.CompanyName;
                            newRegisteredClient.OtherNames = RecordToUpdate.CompanyEmail;
                            newRegisteredClient.EmailAddress = RecordToUpdate.CompanyEmail;
                            newRegisteredClient.AccountNumber = RecordToUpdate.AccountNumber;
                            newRegisteredClient.Status = 1;
                            newRegisteredClient.AcceptedTerms = AcceptedTerms;
                            newRegisteredClient.CreatedBy = User.Identity.GetUserId();
                            newRegisteredClient.UploadedBy = User.Identity.GetUserId();
                            db.RegisteredClients.Add(newRegisteredClient);
                            var clientSaved = db.SaveChanges();
                            clientId = newRegisteredClient.Id;
                        }
                        catch(Exception)
                        {
                            return Json("Error! Unable to create registered client", JsonRequestBehavior.AllowGet);
                        }

                        try
                        {
                            //2. Create Company
                            var newCompany = db.ClientCompanies.Create();
                            newCompany.ClientId = clientId;
                            newCompany.CompanyName = RecordToUpdate.CompanyName;
                            newCompany.Status = 1;
                            newCompany.HasApplication = false;
                            newCompany.DateCreated = DateTime.Now;
                            newCompany.CreatedBy = User.Identity.GetUserId();
                            db.ClientCompanies.Add(newCompany);
                            var companySaved = db.SaveChanges();
                            companyId = newCompany.Id;
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to create company details", JsonRequestBehavior.AllowGet);
                        }

                        try
                        {
                            //3. Create SSI
                            var newSSI = db.ClientSettlementAccounts.Create();
                            newSSI.ClientID = clientId;
                            newSSI.CompanyID = companyId;
                            newSSI.CurrencyID = 6; //6 is for other specified currency
                            newSSI.OtherCurrency = RecordToUpdate.Currency;
                            newSSI.AccountNumber = RecordToUpdate.AccountNumber;
                            newSSI.Status = 1;
                            newSSI.DateCreated = DateTime.Now;
                            db.ClientSettlementAccounts.Add(newSSI);
                            var ssiSaved = db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to create SSI details", JsonRequestBehavior.AllowGet);
                        }

                        try
                        {
                            //4. Create Representative
                            var EMTSignUp = (RecordToUpdate.IsEMTUser == "YES" || RecordToUpdate.IsEMTUser == "Yes" || RecordToUpdate.IsEMTUser == "yes") ? true : false;
                            var GMRep = (RecordToUpdate.IsGM == "YES" || RecordToUpdate.IsGM == "Yes" || RecordToUpdate.IsGM == "yes") ? true : false;
                            var newRepresentative = db.DesignatedUsers.Create();
                            newRepresentative.Surname = RecordToUpdate.RepresentativeName;
                            newRepresentative.TradingLimit = RecordToUpdate.RepresentativeLimit;
                            newRepresentative.Mobile = RecordToUpdate.RepresentativePhonenumber;
                            newRepresentative.Email = RecordToUpdate.RepresentativeEmail;
                            newRepresentative.ClientID = clientId;
                            newRepresentative.CompanyID = companyId;
                            newRepresentative.Status = 1;
                            newRepresentative.EMarketSignUp = EMTSignUp;
                            newRepresentative.GMRepresentative = GMRep;
                            newRepresentative.AcceptedTerms = true;
                            newRepresentative.DateCreated = DateTime.Now;
                            db.DesignatedUsers.Add(newRepresentative);
                            var repSaved = db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to create representative details", JsonRequestBehavior.AllowGet);
                        }
                        
                        try
                        {
                            //Update record as approved
                            RecordToUpdate.Status = 1;
                            RecordToUpdate.ApprovedBy = User.Identity.GetUserId();
                            RecordToUpdate.DateApproved = DateTime.Now;
                            var recordSaved = db.SaveChanges();
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception)
                        {
                            return Json("Error! Unable to approve selected records", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //Check if SSI exists and create if not
                        var SSIExists = db.ClientSettlementAccounts.SingleOrDefault(c => c.AccountNumber == RecordToUpdate.AccountNumber && c.Status == 1);
                        if(SSIExists == null)
                        {
                            //Create SSI
                            var clientDetails = db.RegisteredClients.SingleOrDefault(c => c.AccountNumber == RecordToUpdate.AccountNumber && c.Status == 1);
                            var companyDetails = db.ClientCompanies.SingleOrDefault(c => c.ClientId == clientDetails.Id && c.Status == 1);
                            var newSSI = db.ClientSettlementAccounts.Create();
                            newSSI.ClientID = clientDetails.Id;
                            newSSI.CompanyID = companyDetails.Id;
                            newSSI.CurrencyID = 6; //6 is for other specified currency
                            newSSI.OtherCurrency = RecordToUpdate.Currency;
                            newSSI.AccountNumber = RecordToUpdate.AccountNumber;
                            newSSI.Status = 1;
                            newSSI.DateCreated = DateTime.Now;
                            db.ClientSettlementAccounts.Add(newSSI);
                            var ssiSaved = db.SaveChanges();
                        }

                        //Check if representative exists and create if not
                        var RepExists = db.DesignatedUsers.SingleOrDefault(c => c.Email == RecordToUpdate.RepresentativeEmail && c.Status == 1);
                        if (RepExists == null)
                        {
                            //Create SSI
                            var clientDetails = db.RegisteredClients.SingleOrDefault(c => c.AccountNumber == RecordToUpdate.AccountNumber && c.Status == 1);
                            var companyDetails = db.ClientCompanies.SingleOrDefault(c => c.ClientId == clientDetails.Id && c.Status == 1);
                            var EMTSignUp = (RecordToUpdate.IsEMTUser == "YES" || RecordToUpdate.IsEMTUser == "Yes" || RecordToUpdate.IsEMTUser == "yes") ? true : false;
                            var GMRep = (RecordToUpdate.IsGM == "YES" || RecordToUpdate.IsGM == "Yes" || RecordToUpdate.IsGM == "yes") ? true : false;
                            var newRepresentative = db.DesignatedUsers.Create();
                            newRepresentative.Surname = RecordToUpdate.RepresentativeName;
                            newRepresentative.TradingLimit = RecordToUpdate.RepresentativeLimit;
                            newRepresentative.Mobile = RecordToUpdate.RepresentativePhonenumber;
                            newRepresentative.Email = RecordToUpdate.RepresentativeEmail;
                            newRepresentative.ClientID = clientDetails.Id;
                            newRepresentative.CompanyID = companyDetails.Id;
                            newRepresentative.Status = 1;
                            newRepresentative.EMarketSignUp = EMTSignUp;
                            newRepresentative.GMRepresentative = GMRep;
                            newRepresentative.AcceptedTerms = true;
                            newRepresentative.DateCreated = DateTime.Now;
                            db.DesignatedUsers.Add(newRepresentative);
                            var repSaved = db.SaveChanges();
                        }

                        //Update record as approved
                        RecordToUpdate.Status = 1;
                        RecordToUpdate.ApprovedBy = User.Identity.GetUserId();
                        RecordToUpdate.DateApproved = DateTime.Now;
                        var recordSaved = db.SaveChanges();
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json("Error! Unable to approve selected records", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //Approve records
        [HttpPost]
        public JsonResult DeclineSelected(int Id)
        {
            using (var db = new DBModel())
            {
                try
                {
                    var RecordToUpdate = db.ExistingClientsUploads.SingleOrDefault(c => c.Id == Id);
                    RecordToUpdate.Status = 2;
                    RecordToUpdate.ApprovedBy = User.Identity.GetUserId();
                    RecordToUpdate.DateApproved = DateTime.Now;
                    db.SaveChanges();

                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    return Json("Error! Unable to decline selected records", JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
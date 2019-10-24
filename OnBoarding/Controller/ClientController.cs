using System;
using System.Linq;
using System.Web.Mvc;
using OnBoarding.Services;
using OnBoarding.Models;
using System.IO;
using System.Web;
using OnBoarding.ViewModels;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;

namespace OnBoarding.Controllers
{
    public class ClientController : Controller
    {
        private bool? EMarketSignUp;
        private bool SSI;
        //private int LastApplicationId;

        private dynamic ConfirmationLinkURL => Url.Action("CompleteRegistration", "Account", null, Request.Url.Scheme);
        
        // GET: Client
        [Authorize]
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                {
                    var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId && c.Status == 1);
                    if (userInfo != null)
                    {
                        var LoggedInUserId = userInfo.Id;
                        //var CompanyName = userInfo.CompanyName;
                        var ClientNames = userInfo.Surname +" "+ userInfo.OtherNames;
                        var ApplicationsCount = db.EMarketApplications.Count(c => c.ClientID == LoggedInUserId);
                        var RegisteredCompanies = db.ClientCompanies.Count(c => c.ClientId == LoggedInUserId);
                        var ApprovedApplicationsCount = db.EMarketApplications.Where(a => a.OPSApproved == true && a.POAApproved == true).Count(c => c.ClientID == LoggedInUserId);
                        var DeclinedApplicationsCount = db.EMarketApplications.Where(a => a.Status == 4).Count(c => c.ClientID == LoggedInUserId);
                        var SignatoriesCount = db.ClientSignatories.Count(c => c.ClientID == LoggedInUserId && c.Status == 1);
                       
                        //ViewData["CompanyName"] = CompanyName;
                        ViewData["Applications"] = ApplicationsCount;
                        ViewData["Approved"] = ApprovedApplicationsCount;
                        ViewData["Declined"] = DeclinedApplicationsCount;
                        ViewData["Signatories"] = SignatoriesCount;
                        ViewData["RegisteredCompanies"] = RegisteredCompanies;
                        ViewData["Names"] = ClientNames;
                    }
                }

            }
            return View();
        }

        //
        // Check if Client has existing applications
        // GET: Admin Upolad Clients
        [HttpPost]
        public JsonResult HaveApplications()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId && c.Status == 1);
                var ApplicationsCount = db.ClientCompanies.Count(c => c.ClientId == userInfo.Id && c.Status == 1 && c.HasApplication == false);
                if (ApplicationsCount <= 0)
                {
                    //return Json("Error! All your companies have made an application.", JsonRequestBehavior.AllowGet);
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST //Clear all Representatives
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ClearRepresentatives(ApplicationViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var ClientRepresentativesExist = db.DesignatedUsers.Any(c => c.ClientID == userInfo.Id && c.Status == 0 && c.CompanyID == model.CompanyID);
                if (ClientRepresentativesExist)
                {
                    db.DesignatedUsers.RemoveRange(db.DesignatedUsers.Where(r => r.ClientID == userInfo.Id && r.CompanyID == model.CompanyID));
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to delete representatives", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("No representatives to delete", JsonRequestBehavior.AllowGet);
                }
            }
            
        }

        //
        //POST //Remove One Representative after Save
        [HttpPost]
        [AllowAnonymous]
        public ActionResult RemoveRepresentative(string email)
        {
            using (DBModel db = new DBModel())
            {
                var ClientRepresentativesExist = db.DesignatedUsers.Any(c => c.Email == email);
                if (ClientRepresentativesExist)
                {
                    db.DesignatedUsers.RemoveRange(db.DesignatedUsers.Where(r => r.Email == email));
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to delete representative", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("No representative to delete", JsonRequestBehavior.AllowGet);
                }
            }
        }


        //
        // GET: //New Applications Page
        [Authorize]
        public ActionResult NewApplications()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId && c.Status == 1);
                var RegisteredCompanies = db.ClientCompanies.Count(c => c.ClientId == userInfo.Id);
                ViewData["RegisteredCompanies"] = RegisteredCompanies;

                //Removed has application code

                if (userInfo != null)
                {
                    // Added Uploaded Details to reflect on the applications page
                    ViewBag.ClientInfo = userInfo;

                    var Signatory1Details = db.ClientSignatories.FirstOrDefault(a => a.ClientID == userInfo.Id && a.EmailAddress == userInfo.EmailAddress && a.Status == 1);
                    if (Signatory1Details != null)
                    {
                        ViewBag.SignatoryDetails = Signatory1Details;
                    }

                    var Representative1Details = db.DesignatedUsers.FirstOrDefault(a => a.ClientID == userInfo.Id && a.Email == userInfo.EmailAddress && a.Status == 1);
                    if (Representative1Details != null)
                    {
                        ViewBag.RepresentativesDetails = Representative1Details;
                    }

                    //Get the list of all client's settlement accounts
                    var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber, s.CurrencyId FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.Status = 1 AND s.ClientID =  " + "'" + userInfo.Id + "'" + " AND s.Status = 1");
                    ViewBag.SettlementAccounts = Query.ToList();

                    //Get the list of all saved signatories
                    List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == userInfo.Id && a.Status != 2).ToList();
                    ViewBag.ClientSignatory = SignatoryList;

                    //Get the list of saved authorized representatives
                    List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == userInfo.Id && a.Status != 2).ToList();
                    ViewBag.DesignatedUser = DesignatedUsersList;
                }
                return View();
            }
        }

        //
        // POST: Applications
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewApplication(ApplicationViewModel model, HttpPostedFileBase inputFile)
        {
            //Check for errors in model
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            //Check if there is any emt signup
            EMarketSignUp = (((model.EMarketSignUp1 == true) || (model.EMarketSignUp2 == true) || (model.EMarketSignUp3 == true) || (model.EMarketSignUp4 == true) || (model.EMarketSignUp5 == true))) ? true : false;

            //Log SSI Value if settlement account exist
            SSI = (model.HaveSettlementAccount == "Yes") ? true : false;

            //Removes if Is.State.Valid since we have Jquery Validate
            using (DBModel db = new DBModel())
            {
                //Update Registered Client Details with provided form values
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId && c.Status == 1);
                var RegisteredClientId = userInfo.Id;
                var CompanyEmail = userInfo.EmailAddress.ToLower();
                var _action = "AddNewApplication";

                var updateClient = db.RegisteredClients.SingleOrDefault(c => c.Id == RegisteredClientId && c.Status == 1);
                if (updateClient != null)
                {
                    try
                    {
                        //updateClient.CompanyName = model.CompanyName;
                        updateClient.PhoneNumber = model.SignatoryPhoneNumber1;
                        updateClient.AcceptedTerms = true;
                        updateClient.AcceptedUserTerms = true;
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        //throw (ex);
                        return Json("Unable to save your application company details", JsonRequestBehavior.AllowGet);
                    }
                }

                //Log Settlement Account Details when yes is selected
                if (model.HaveSettlementAccount == "Yes")
                {
                    //Log Settlement Accounts (1-5)
                    var newAccountDetails = db.ClientSettlementAccounts.Create();
                    if (model.SettlementAccount1 != null && model.SelectCurrency1 != null)
                    {
                        var SettlementAccount1Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount1 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                        if (!SettlementAccount1Exists) 
                        {
                            newAccountDetails.ClientID = RegisteredClientId;
                            newAccountDetails.CompanyID = model.CompanyID;
                            newAccountDetails.AccountNumber = model.SettlementAccount1;
                            newAccountDetails.OtherCurrency = model.InputCurrencyType1;
                            newAccountDetails.CurrencyID = model.SelectCurrency1;
                            newAccountDetails.Status = 1;
                            db.ClientSettlementAccounts.Add(newAccountDetails);
                            db.SaveChanges();
                        }
                    }
                    if (model.SettlementAccount2 != null && model.SelectCurrency2 != null)
                    {
                        var SettlementAccount2Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount2 && c.CompanyID == model.CompanyID);
                        if (!SettlementAccount2Exists)
                        {
                            newAccountDetails.ClientID = RegisteredClientId;
                            newAccountDetails.CompanyID = model.CompanyID;
                            newAccountDetails.AccountNumber = model.SettlementAccount2;
                            newAccountDetails.OtherCurrency = model.InputCurrencyType2;
                            newAccountDetails.CurrencyID = model.SelectCurrency2;
                            newAccountDetails.Status = 1;
                            db.ClientSettlementAccounts.Add(newAccountDetails);
                            db.SaveChanges();
                        }
                    }
                    if (model.SettlementAccount3 != null && model.SelectCurrency3 != null)
                    {
                        var SettlementAccount3Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount3 && c.CompanyID == model.CompanyID);
                        if (!SettlementAccount3Exists)
                        {
                            newAccountDetails.ClientID = RegisteredClientId;
                            newAccountDetails.CompanyID = model.CompanyID;
                            newAccountDetails.AccountNumber = model.SettlementAccount3;
                            newAccountDetails.OtherCurrency = model.InputCurrencyType3;
                            newAccountDetails.CurrencyID = model.SelectCurrency3;
                            newAccountDetails.Status = 1;
                            db.ClientSettlementAccounts.Add(newAccountDetails);
                            db.SaveChanges();
                        }
                    }
                    if (model.SettlementAccount4 != null && model.SelectCurrency4 != null)
                    {
                        var SettlementAccount4Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount4 && c.CompanyID == model.CompanyID);
                        if (!SettlementAccount4Exists)
                        {
                            newAccountDetails.ClientID = RegisteredClientId;
                            newAccountDetails.CompanyID = model.CompanyID;
                            newAccountDetails.AccountNumber = model.SettlementAccount4;
                            newAccountDetails.OtherCurrency = model.InputCurrencyType4;
                            newAccountDetails.CurrencyID = model.SelectCurrency4;
                            newAccountDetails.Status = 1;
                            db.ClientSettlementAccounts.Add(newAccountDetails);
                            db.SaveChanges();
                        }
                    }
                    if (model.SettlementAccount5 != null && model.SelectCurrency5 != null)
                    {
                        var SettlementAccount5Exists = db.ClientSettlementAccounts.Any(c => c.AccountNumber == model.SettlementAccount5 && c.CompanyID == model.CompanyID);
                        if (!SettlementAccount5Exists)
                        {
                            newAccountDetails.ClientID = RegisteredClientId;
                            newAccountDetails.CompanyID = model.CompanyID;
                            newAccountDetails.AccountNumber = model.SettlementAccount5;
                            newAccountDetails.OtherCurrency = model.InputCurrencyType5;
                            newAccountDetails.CurrencyID = model.SelectCurrency5;
                            newAccountDetails.Status = 1;
                            db.ClientSettlementAccounts.Add(newAccountDetails);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    var SettlementAccountExists = db.ClientSettlementAccounts.Any(c => c.ClientID == RegisteredClientId && c.CompanyID == model.CompanyID);
                    if (SettlementAccountExists)
                    {
                        //Clear any settlement accounts if saved
                        db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(r => r.ClientID == RegisteredClientId && r.CompanyID == model.CompanyID));
                        db.SaveChanges();
                    }
                }

                //Create First Signatory (Signatory 1)
                if (model.SignatorySurname1 != null && model.SignatoryOtherNames1 != null && model.SignatoryEmail1 != null && model.SignatoryDesignation1 != null)
                {
                    //check if Signatory1 Exists
                    var Signatory1Exists = db.ClientSignatories.Any(s => s.EmailAddress == model.SignatoryEmail1 && s.CompanyID == model.CompanyID);
                    if (Signatory1Exists == false) //Add New if not Exists
                    {
                        try
                        {
                            //Upload Signature
                            if (Request.Files.Count > 0)
                            {
                                var file = Request.Files[0];
                                if (file != null && inputFile.ContentLength > 0)
                                {
                                    var fileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + System.IO.Path.GetFileName(inputFile.FileName);
                                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), fileName);
                                    file.SaveAs(path);
                                }
                            }

                            //Add Details
                            var addSignatory1 = db.ClientSignatories.Create();
                            var newFileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + inputFile.FileName;
                            addSignatory1.ClientID = RegisteredClientId;
                            addSignatory1.CompanyID = model.CompanyID;
                            addSignatory1.Designation = model.SignatoryDesignation1;
                            addSignatory1.Surname = model.SignatorySurname1;
                            addSignatory1.OtherNames = model.SignatoryOtherNames1;
                            addSignatory1.EmailAddress = model.SignatoryEmail1.ToLower();
                            addSignatory1.PhoneNumber = model.SignatoryPhoneNumber1;
                            addSignatory1.Signature = newFileName;
                            addSignatory1.DateCreated = DateTime.Now;
                            addSignatory1.Status = 1;
                            addSignatory1.AcceptedTerms = true;
                            db.ClientSignatories.Add(addSignatory1);
                            var savedDetails = db.SaveChanges();
                            if(savedDetails <= 0)
                            {
                                return Json("Error! Unable to save signatory details", JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to save signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        try
                        {   //Update Signatory Details
                            //Upload Signature if it does not exist
                            var Signatory1ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1 && c.CompanyID == model.CompanyID);
                            if (Signatory1ToUpdate.Signature == null)
                            {
                                try
                                {
                                    if (Request.Files.Count > 0)
                                    {
                                        var file = Request.Files[0];
                                        if (file != null && inputFile.ContentLength > 0)
                                        {
                                            var fileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + System.IO.Path.GetFileName(inputFile.FileName);
                                            string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), fileName);
                                            file.SaveAs(path);
                                            Signatory1ToUpdate.Signature = fileName;
                                            Signatory1ToUpdate.Status = 1;
                                            Signatory1ToUpdate.CompanyID = model.CompanyID;
                                            Signatory1ToUpdate.Designation = model.SignatoryDesignation1;
                                            Signatory1ToUpdate.Surname = model.SignatorySurname1;
                                            Signatory1ToUpdate.OtherNames = model.SignatoryOtherNames1;
                                            Signatory1ToUpdate.EmailAddress = model.SignatoryEmail1.ToLower();
                                            Signatory1ToUpdate.PhoneNumber = model.SignatoryPhoneNumber1;
                                            Signatory1ToUpdate.DateCreated = DateTime.Now;
                                            Signatory1ToUpdate.AcceptedTerms = true;
                                            db.SaveChanges();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    //throw (ex);
                                    return Json("Error! Unable to update signatory details", JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                //Edit Details without the signature
                                Signatory1ToUpdate.Status = 1;
                                Signatory1ToUpdate.CompanyID = model.CompanyID;
                                Signatory1ToUpdate.Designation = model.SignatoryDesignation1;
                                Signatory1ToUpdate.Surname = model.SignatorySurname1;
                                Signatory1ToUpdate.OtherNames = model.SignatoryOtherNames1;
                                Signatory1ToUpdate.EmailAddress = model.SignatoryEmail1.ToLower();
                                Signatory1ToUpdate.PhoneNumber = model.SignatoryPhoneNumber1;
                                Signatory1ToUpdate.DateCreated = DateTime.Now;
                                Signatory1ToUpdate.AcceptedTerms = true;
                                db.SaveChanges();
                            } 
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Unable to save signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                //Create Signatory (Signatory 2)
                if (model.SignatorySurname2 != null && model.SignatoryOtherNames2 != null && model.SignatoryEmail2 != null && model.SignatoryDesignation2 != null)
                {
                    //Check if Signatory already exists in Signatories table
                    var Signatory2Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail2 && c.Status == 0 && c.CompanyID == model.CompanyID);
                    if (!Signatory2Exist)
                    {   
                        //Add new Signatory
                        try
                        {
                            var addSignatory2 = db.ClientSignatories.Create();
                            addSignatory2.ClientID = RegisteredClientId;
                            addSignatory2.CompanyID = model.CompanyID;
                            addSignatory2.Designation = model.SignatoryDesignation2;
                            addSignatory2.Surname = model.SignatorySurname2;
                            addSignatory2.OtherNames = model.SignatoryOtherNames2;
                            addSignatory2.EmailAddress = model.SignatoryEmail2.ToLower();
                            addSignatory2.PhoneNumber = model.SignatoryPhoneNumber2;
                            addSignatory2.DateCreated = DateTime.Now;
                            addSignatory2.AcceptedTerms = false;
                            addSignatory2.Status = 0;
                            db.ClientSignatories.Add(addSignatory2);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to save 2nd signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //Update Details
                        try
                        {
                            var Signatory2ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail2 && c.CompanyID == model.CompanyID);
                            Signatory2ToUpdate.Status = 0;
                            Signatory2ToUpdate.CompanyID = model.CompanyID;
                            Signatory2ToUpdate.DateCreated = DateTime.Now;
                            Signatory2ToUpdate.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to update 2nd signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                //Create Signatory (Signatory 3)
                if (model.SignatorySurname3 != null && model.SignatoryOtherNames3 != null && model.SignatoryEmail3 != null && model.SignatoryDesignation3 != null)
                {
                    //Check if Signatory already exists in Signatories table
                    var Signatory3Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail3 && c.Status == 0 && c.CompanyID == model.CompanyID);
                    if (!Signatory3Exist)
                    {
                        //Add new Signatory
                        try
                        {
                            var addSignatory3 = db.ClientSignatories.Create();
                            addSignatory3.ClientID = RegisteredClientId;
                            addSignatory3.CompanyID = model.CompanyID;
                            addSignatory3.Designation = model.SignatoryDesignation3;
                            addSignatory3.Surname = model.SignatorySurname3;
                            addSignatory3.OtherNames = model.SignatoryOtherNames3;
                            addSignatory3.EmailAddress = model.SignatoryEmail3.ToLower();
                            addSignatory3.PhoneNumber = model.SignatoryPhoneNumber3;
                            addSignatory3.DateCreated = DateTime.Now;
                            addSignatory3.AcceptedTerms = false;
                            addSignatory3.Status = 0;
                            db.ClientSignatories.Add(addSignatory3);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to save 3rd signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //Update Details
                        try
                        {
                            var Signatory3ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail3 && c.CompanyID == model.CompanyID);
                            Signatory3ToUpdate.Status = 0;
                            Signatory3ToUpdate.CompanyID = model.CompanyID;
                            Signatory3ToUpdate.DateCreated = DateTime.Now;
                            Signatory3ToUpdate.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to update 3rd signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                //Create Signatory (Signatory 4)
                if (model.SignatorySurname4 != null && model.SignatoryOtherNames4 != null && model.SignatoryEmail4 != null && model.SignatoryDesignation4 != null)
                {
                    //Check if Signatory already exists in Signatories table
                    var Signatory4Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail4 && c.Status == 0 && c.CompanyID == model.CompanyID);
                    if (!Signatory4Exist)
                    {
                        //Add new Signatory
                        try
                        {
                            var addSignatory4 = db.ClientSignatories.Create();
                            addSignatory4.ClientID = RegisteredClientId;
                            addSignatory4.CompanyID = model.CompanyID;
                            addSignatory4.Designation = model.SignatoryDesignation4;
                            addSignatory4.Surname = model.SignatorySurname4;
                            addSignatory4.OtherNames = model.SignatoryOtherNames4;
                            addSignatory4.EmailAddress = model.SignatoryEmail4.ToLower();
                            addSignatory4.PhoneNumber = model.SignatoryPhoneNumber4;
                            addSignatory4.DateCreated = DateTime.Now;
                            addSignatory4.AcceptedTerms = false;
                            addSignatory4.Status = 0;
                            db.ClientSignatories.Add(addSignatory4);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to save 4th signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //Update Details
                        try
                        {
                            var Signatory4ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail4 && c.CompanyID == model.CompanyID);
                            Signatory4ToUpdate.Status = 0;
                            Signatory4ToUpdate.CompanyID = model.CompanyID;
                            Signatory4ToUpdate.DateCreated = DateTime.Now;
                            Signatory4ToUpdate.AcceptedTerms = false;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to update 4th signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                //Create Signatory (Signatory 5)
                if (model.SignatorySurname5 != null && model.SignatoryOtherNames5 != null && model.SignatoryEmail5 != null && model.SignatoryDesignation5 != null)
                {
                    //Check if Signatory already exists in Signatories table
                    var Signatory5Exist = db.ClientSignatories.Any(c => c.EmailAddress == model.SignatoryEmail5 && c.Status == 0 && c.CompanyID == model.CompanyID);
                    if (!Signatory5Exist)
                    {
                        try
                        {
                            //Add new Signatory
                            var addSignatory5 = db.ClientSignatories.Create();
                            addSignatory5.ClientID = RegisteredClientId;
                            addSignatory5.CompanyID = model.CompanyID;
                            addSignatory5.Designation = model.SignatoryDesignation5;
                            addSignatory5.Surname = model.SignatorySurname5;
                            addSignatory5.OtherNames = model.SignatoryOtherNames5;
                            addSignatory5.EmailAddress = model.SignatoryEmail5.ToLower();
                            addSignatory5.PhoneNumber = model.SignatoryPhoneNumber5;
                            addSignatory5.DateCreated = DateTime.Now;
                            addSignatory5.AcceptedTerms = false;
                            addSignatory5.Status = 0;
                            db.ClientSignatories.Add(addSignatory5);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to save 5th signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        try
                        {
                            //Update Details
                            var Signatory5ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail5 && c.CompanyID == model.CompanyID);
                            Signatory5ToUpdate.Status = 0;
                            Signatory5ToUpdate.CompanyID = model.CompanyID;
                            Signatory5ToUpdate.DateCreated = DateTime.Now;
                            Signatory5ToUpdate.AcceptedTerms = false;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to update 5th signatory details", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                //Log authorised representative (Representative 1)
                if (model.UserEmail1 != null && model.UserSurname1 != null && model.UserOthernames1 != null && model.UserMobileNumber1 != null)
                {
                    //Check if representative exists
                    var Representative1Exists = db.DesignatedUsers.Any(c => c.Email == model.UserEmail1 && c.CompanyID == model.CompanyID);
                    if (Representative1Exists == false)
                    {
                        //Create new representative if false
                        try
                        {
                            var addDesignatedUser1 = db.DesignatedUsers.Create();
                            addDesignatedUser1.Status = 0;
                            addDesignatedUser1.ClientID = RegisteredClientId;
                            addDesignatedUser1.CompanyID = model.CompanyID;
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
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to save 1st representatives details", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        try
                        {
                            //Update Status if representative exists
                            var Representative1ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail1 && c.CompanyID == model.CompanyID);
                            Representative1ToUpdateStatus.Status = 0;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //throw (ex);
                            return Json("Error! Unable to update 1st representatives details", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                //Log authorised Representative (Representative 2)
                if (model.UserEmail2 != null && model.UserSurname2 != null && model.UserOthernames2 != null && model.UserMobileNumber2 != null)
                {
                    var Representative2Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail2 && c.Status != 2 && c.CompanyID == model.CompanyID);
                    var RepresentativeIsAClient = db.RegisteredClients.SingleOrDefault(c => c.EmailAddress == model.UserEmail2);
                        
                    if (Representative2Exists == null)
                    {
                        var addDesignatedUser2 = db.DesignatedUsers.Create();
                        addDesignatedUser2.Status = 0;
                        addDesignatedUser2.ClientID = RegisteredClientId;
                        addDesignatedUser2.CompanyID = model.CompanyID;
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
                        var Representative2ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail2 && c.CompanyID == model.CompanyID);
                        Representative2ToUpdateStatus.Status = 0;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative (Representative 3)
                if (model.UserEmail3 != null && model.UserSurname3 != null && model.UserOthernames3 != null && model.UserMobileNumber3 != null)
                {
                    var Representative3Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail3 && c.CompanyID == model.CompanyID);
                    if (Representative3Exists == null)
                    {
                        var addDesignatedUser3 = db.DesignatedUsers.Create();
                        addDesignatedUser3.Status = 0;
                        addDesignatedUser3.ClientID = RegisteredClientId;
                        addDesignatedUser3.CompanyID = model.CompanyID;
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
                        var Representative3ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail3 && c.CompanyID == model.CompanyID);
                        Representative3ToUpdateStatus.Status = 0;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative (Representative 4)
                if (model.UserEmail4 != null && model.UserSurname4 != null && model.UserOthernames4 != null && model.UserMobileNumber4 != null)
                {
                    var Representative4Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail4 && c.CompanyID == model.CompanyID);
                    if (Representative4Exists == null)
                    { 
                        var addDesignatedUser4 = db.DesignatedUsers.Create();
                        addDesignatedUser4.Status = 0;
                        addDesignatedUser4.ClientID = RegisteredClientId;
                        addDesignatedUser4.CompanyID = model.CompanyID;
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
                        var Representative4ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail4 && c.CompanyID == model.CompanyID);
                        Representative4ToUpdateStatus.Status = 0;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative (Representative 5)
                if (model.UserEmail5 != null && model.UserSurname5 != null && model.UserOthernames5 != null && model.UserMobileNumber5 != null)
                {
                    var Representative5Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail5 && c.CompanyID == model.CompanyID);
                    if (Representative5Exists == null)
                    { 
                        var addDesignatedUser5 = db.DesignatedUsers.Create();
                        addDesignatedUser5.Status = 0;
                        addDesignatedUser5.ClientID = RegisteredClientId;
                        addDesignatedUser5.CompanyID = model.CompanyID;
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
                        var Representative5ToUpdateStatus = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail5 && c.CompanyID == model.CompanyID);
                        Representative5ToUpdateStatus.Status = 0;
                        db.SaveChanges();
                    }
                }
                    
                //Update Number of Application Signatories and Designated Users
                var SignatoryCount = db.ClientSignatories.Count(c => c.ClientID == userInfo.Id && c.Status != 2 && c.CompanyID == model.CompanyID);
                var DesignatedUsersCount = db.DesignatedUsers.Count(c => c.ClientID == userInfo.Id && c.Status != 2 && c.CompanyID == model.CompanyID);

                //Create A new Application
                //1). Scenario (Sole signatory application)
                //If signatory and representative are the same and one
                //Application process is complete ops can approve the application
                if (SignatoryCount == 1 && DesignatedUsersCount == 1)
                {
                    //Check if the signatory is the representative
                    var signatory1Email = model.SignatoryEmail1.ToLower();
                    var designated1Email = model.UserEmail1.ToLower();

                    if (model.SignatoryEmail1.ToLower() == model.UserEmail1.ToLower())
                    {
                        try
                        {
                            //Update representative signature
                            if (inputFile != null)
                            {
                                var fileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + inputFile.FileName;
                                var RepresentativeToUpdate = db.DesignatedUsers.First(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                                RepresentativeToUpdate.Signature = fileName;
                                db.SaveChanges();
                            }
                            else
                            {
                                var sameSignatory = db.ClientSignatories.First(c => c.EmailAddress == model.UserEmail1.ToLower() && c.CompanyID == model.CompanyID);
                                var fileName = sameSignatory.Signature;
                                var RepresentativeToUpdate = db.DesignatedUsers.First(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                                RepresentativeToUpdate.Signature = fileName;
                                db.SaveChanges();
                            }

                            //Mark Signatory 1 as status 1
                            var updateSignatory1 = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                            updateSignatory1.Status = 1;
                            db.SaveChanges();

                            //Mark Representative 1 as status 1
                            var updateRepresentative1 = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                            updateRepresentative1.Status = 1;
                            db.SaveChanges();

                            //Log new application
                            var newApplication = db.EMarketApplications.Create();
                            newApplication.AcceptedTAC = true;
                            newApplication.CompanyID = model.CompanyID;
                            newApplication.ClientID = RegisteredClientId;
                            newApplication.DesignatedUsers = DesignatedUsersCount;
                            newApplication.DateCreated = DateTime.Now;
                            newApplication.Signatories = SignatoryCount;
                            newApplication.Status = 1;
                            newApplication.SignatoriesApproved = 1;
                            newApplication.SignatoriesDateApproved = DateTime.Now;
                            newApplication.UsersApproved = 1;
                            newApplication.Emt = EMarketSignUp;
                            newApplication.SSI = SSI;
                            newApplication.UsersDateApproved = DateTime.Now;
                            db.EMarketApplications.Add(newApplication);
                            var applicationSaved = db.SaveChanges();
                            if (applicationSaved > 0)
                            {
                                try
                                {
                                    //Log an active signatory approval
                                    var SignatoryID = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                                    var newApprovalDetail = db.SignatoryApprovals.Create();
                                    newApprovalDetail.SignatoryID = SignatoryID.Id;
                                    newApprovalDetail.ApplicationID = applicationSaved;
                                    newApprovalDetail.Status = 1;
                                    db.SaveChanges();

                                    //Log an inactive representative approval
                                    var RepresentativeID = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                                    var newDesignatedUserApproval = db.SignatoryApprovals.Create();
                                    newDesignatedUserApproval.SignatoryID = RepresentativeID.Id;
                                    newDesignatedUserApproval.ApplicationID = applicationSaved;
                                    newDesignatedUserApproval.Status = 1;
                                    db.SaveChanges();

                                    //Mark HasApplication True for Client Company
                                    var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                                    updateClientCompany.HasApplication = true;
                                    db.SaveChanges();
                                }
                                catch(Exception ex)
                                {
                                    return Json(""+ ex.Message +"", JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json("Error saving your application details!", JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }

                        //Send Application Complete Email to Company Email
                        string EmailBody = string.Empty;
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/SoleSignatoryApplication.html")))
                        {
                            EmailBody = reader.ReadToEnd();
                        }
                        EmailBody = EmailBody.Replace("{CompanyName}",userInfo.OtherNames);
                            
                        var ApplicationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, CompanyEmail, "Application Complete", EmailBody);

                        if (ApplicationCompleteEmail == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, CompanyEmail, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, CompanyEmail, _action);
                        }

                        //Send Email to Digital Desk Users
                        var DDUserRole = (from p in db.AspNetUserRoles
                                        join e in db.AspNetUsers on p.UserId equals e.Id
                                        where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc"
                                        select new
                                        {
                                            EmailID = e.Email
                                        });
                        foreach (var email in DDUserRole.ToList())
                        {
                            var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client has successfully submitted an application. <br/>" +
                                            "Company Name: " + model.CompanyName + ", Company Email: " + CompanyEmail + ", Application Date: " + DateTime.Now + " " +
                                            "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                            var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Registration Complete", DDMessageBody);
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
                    else
                    {
                        try
                        { 
                            //Mark Signatory 1 as status 1
                            var updateSignatory1 = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower());
                            updateSignatory1.Status = 1;
                            updateSignatory1.AcceptedTerms = true;
                            db.SaveChanges();

                            var newApplication = db.EMarketApplications.Create();
                            newApplication.AcceptedTAC = true; //model.terms;
                            newApplication.ClientID = RegisteredClientId;
                            newApplication.CompanyID = model.CompanyID;
                            newApplication.DesignatedUsers = DesignatedUsersCount;
                            newApplication.DateCreated = DateTime.Now;
                            newApplication.Signatories = SignatoryCount;
                            newApplication.Status = 1;
                            newApplication.SignatoriesApproved = 1;
                            newApplication.SignatoriesDateApproved = DateTime.Now;
                            newApplication.Emt = EMarketSignUp;
                            newApplication.SSI = SSI;
                            db.EMarketApplications.Add(newApplication);
                            var applicationSaved = db.SaveChanges();
                            if (applicationSaved > 0)
                            {
                                //Mark HasApplication True for Client Company
                                var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                                updateClientCompany.HasApplication = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                return Json("Error!", JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }

                        //Update Designated User with OTP to Login
                        var _OTPCode = OTPGenerator.GetUniqueKey(6);
                        string OTPCode = Shuffle.StringMixer(_OTPCode);
                        var UserToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail1.ToLower());
                        UserToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                        db.SaveChanges();

                        //Send Email To Authorized Representative
                        var callbackUrl = Url.Action("DesignatedUserConfirmation", "Account", null, Request.Url.Scheme);
                        string EmailBodyRep = string.Empty;
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeNomination.html")))
                        {
                            EmailBodyRep = reader.ReadToEnd();
                        }
                        EmailBodyRep = EmailBodyRep.Replace("{RepresentativeName}", model.UserOthernames1);
                        EmailBodyRep = EmailBodyRep.Replace("{ActivationCode}", OTPCode);
                        EmailBodyRep = EmailBodyRep.Replace("{URL}", callbackUrl);

                        var EmailToRepresentative = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.UserEmail1.ToLower(), "Authorized Representative Confirmation", EmailBodyRep);
                        if (EmailToRepresentative == true)
                        {
                            //Log email sent notification
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBodyRep, model.UserEmail1.ToLower(), _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBodyRep, model.UserEmail1.ToLower(), _action);
                        }
                    }
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                
                //2). Scenario (Sole signatory and representatives)
                //If signatory is one (sole) and representatives are more than one
                //Signatory is marked as approved and an email is sent to the representatives except the sole is also nominated under representatives
                else if (SignatoryCount == 1 && DesignatedUsersCount > 1)
                {
                    try
                    {
                        //1. Check if signatory is among the representatives
                        var newApplication = db.EMarketApplications.Create();
                        var signatoryIsARepresentative = db.DesignatedUsers.Any(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                        if (signatoryIsARepresentative)
                        {
                            //Update representative signature
                            var RepresentativeToUpdate = db.DesignatedUsers.First(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                            var signatoryToUpdate = db.ClientSignatories.First(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                            RepresentativeToUpdate.Signature = signatoryToUpdate.Signature;
                            db.SaveChanges();

                            //Mark Signatory 1 as status 1
                            var updateSignatory1 = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                            updateSignatory1.Status = 1;
                            db.SaveChanges();

                            newApplication.AcceptedTAC = true;//model.terms;
                            newApplication.ClientID = RegisteredClientId;
                            newApplication.CompanyID = model.CompanyID;
                            newApplication.DesignatedUsers = DesignatedUsersCount;
                            newApplication.DateCreated = DateTime.Now;
                            newApplication.Signatories = SignatoryCount;
                            newApplication.Status = 1;
                            newApplication.SignatoriesApproved = 1;
                            newApplication.UsersApproved = 1;
                            newApplication.UsersDateApproved = DateTime.Now;
                            newApplication.SignatoriesDateApproved = DateTime.Now;
                            newApplication.Emt = EMarketSignUp;
                            newApplication.SSI = SSI;
                            db.EMarketApplications.Add(newApplication);
                            var applicationSaved = db.SaveChanges();
                            if (applicationSaved > 0)
                            {
                                //Mark HasApplication True for Client Company
                                var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                                updateClientCompany.HasApplication = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                return Json("Error! Unable to add new application", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            //Mark Signatory 1 as status 1
                            var updateSignatory1 = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID);
                            updateSignatory1.Status = 1;
                            db.SaveChanges();

                            newApplication.AcceptedTAC = true; //model.terms;
                            newApplication.ClientID = RegisteredClientId;
                            newApplication.CompanyID = model.CompanyID;
                            newApplication.DesignatedUsers = DesignatedUsersCount;
                            newApplication.DateCreated = DateTime.Now;
                            newApplication.Signatories = SignatoryCount;
                            newApplication.Status = 1;
                            newApplication.SignatoriesApproved = 1;
                            newApplication.SignatoriesDateApproved = DateTime.Now;
                            newApplication.Emt = EMarketSignUp;
                            newApplication.SSI = SSI;
                            db.EMarketApplications.Add(newApplication);
                            var applicationSaved = db.SaveChanges();
                            if (applicationSaved > 0)
                            {
                                //Mark HasApplication True for Client Company
                                var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                                updateClientCompany.HasApplication = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                return Json("Error! Unable to add new application", JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json("" + ex.Message + "", JsonRequestBehavior.AllowGet);
                    }

                    //2. Send email to other representatives excluding the sole signatory
                    var _dontSendEmail = db.AspNetUsers.Select(x => x.Email).ToList();
                    var UserToExclude = db.RegisteredClients.SingleOrDefault(c => c.Id == RegisteredClientId);
                    foreach (var email in db.DesignatedUsers.Where(c => c.ClientID == RegisteredClientId && c.CompanyID == model.CompanyID && !_dontSendEmail.Contains(c.Email) && c.Email != UserToExclude.EmailAddress).ToList())
                    {
                        //Update Designated User with OTP to Login
                        var _OTPCode = OTPGenerator.GetUniqueKey(6);
                        string OTPCode = Shuffle.StringMixer(_OTPCode);
                        var UserToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == email.Email);
                        UserToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                        db.SaveChanges();

                        //Send Email
                        var callbackUrl = Url.Action("DesignatedUserConfirmation", "Account", null, Request.Url.Scheme);
                        string EmailBodyRep = string.Empty;
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/RepresentativeNomination.html")))
                        {
                            EmailBodyRep = reader.ReadToEnd();
                        }
                        EmailBodyRep = EmailBodyRep.Replace("{RepresentativeName}", email.Othernames);
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

                    //3. Send Application Complete Email to Company Email
                    string EmailBody = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ApplicationSubmitted.html")))
                    {
                        EmailBody = reader.ReadToEnd();
                    }
                    EmailBody = EmailBody.Replace("{CompanyName}", userInfo.OtherNames);
                    var ApplicationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, CompanyEmail, "Application Complete", EmailBody);

                    if (ApplicationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody, CompanyEmail, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody, CompanyEmail, _action);
                    }

                    //4. Send Email to Digital Desk Users
                    var DDUserRole = (from p in db.AspNetUserRoles
                                        join e in db.AspNetUsers on p.UserId equals e.Id
                                        where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc"
                                        select new
                                        {
                                            EmailID = e.Email
                                        });
                    foreach (var email in DDUserRole.ToList())
                    {
                        var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client has successfully submitted an application. <br/>" +
                                        "Company Name: " + model.CompanyName + ", Company Email: " + CompanyEmail + ", Application Date: " + DateTime.Now + " " +
                                        "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Registration Complete", DDMessageBody);
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
                
                //3) Scenario (Multiple signatories and representatives -- including first signatory)
                //If signatory are more than 1 including sole and representatives are also more than one
                //Signatory will approve after  which the representatives will be invited to complete registration and also approve
                else if (SignatoryCount > 1 && DesignatedUsersCount >= 1)
                {
                    //1. Check if first signatory is among the representatives
                    var signatoryIsARepresentative = db.DesignatedUsers.Any(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID && c.ClientID == model.ClientID);
                    if (signatoryIsARepresentative)
                    {
                        try
                        {
                            //1. Update representatives signature
                            var RepresentativeToUpdate = db.DesignatedUsers.First(c => c.Email == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID && c.ClientID == model.ClientID);
                            var Signatory1 = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID && c.ClientID == model.ClientID);
                            RepresentativeToUpdate.Signature = Signatory1.Signature;
                            db.SaveChanges();

                            //2. Log new application
                            var newApplication = db.EMarketApplications.Create();
                            newApplication.AcceptedTAC = true; //model.terms;
                            newApplication.ClientID = RegisteredClientId;
                            newApplication.CompanyID = model.CompanyID;
                            newApplication.DesignatedUsers = DesignatedUsersCount;
                            newApplication.DateCreated = DateTime.Now;
                            newApplication.Signatories = SignatoryCount;
                            newApplication.SignatoriesApproved = 1;
                            newApplication.UsersApproved = 1;
                            newApplication.UsersDateApproved = DateTime.Now;
                            newApplication.Status = 1;
                            newApplication.Emt = EMarketSignUp;
                            newApplication.SSI = SSI;
                            db.EMarketApplications.Add(newApplication);
                            var applicationSaved = db.SaveChanges();
                            if (applicationSaved > 0)
                            {
                                //Mark HasApplication True for Client Company
                                var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                                updateClientCompany.HasApplication = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                return Json("Error! Unable to create application details", JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json("" + ex.Message + "", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        // 1. Mark Signatory 1 as status 1
                        var updateSignatory1 = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1.ToLower() && c.CompanyID == model.CompanyID && c.ClientID == model.ClientID);
                        updateSignatory1.Status = 1;
                        db.SaveChanges();

                        //2. Log new application
                        var newApplication = db.EMarketApplications.Create();
                        newApplication.AcceptedTAC = true; //model.terms;
                        newApplication.ClientID = RegisteredClientId;
                        newApplication.CompanyID = model.CompanyID;
                        newApplication.DesignatedUsers = DesignatedUsersCount;
                        newApplication.DateCreated = DateTime.Now;
                        newApplication.Signatories = SignatoryCount;
                        newApplication.SignatoriesApproved = 1;
                        newApplication.Status = 1;
                        newApplication.Emt = EMarketSignUp;
                        newApplication.SSI = SSI;
                        db.EMarketApplications.Add(newApplication);
                        var applicationSaved = db.SaveChanges();
                        if (applicationSaved > 0)
                        {
                            //Mark HasApplication True for Client Company
                            var updateClientCompany = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID);
                            updateClientCompany.HasApplication = true;
                            db.SaveChanges();
                        }
                        else
                        {
                            return Json("Error! Unable to create application details", JsonRequestBehavior.AllowGet);
                        }
                    }

                    //3. Send Application Complete Email to Company Email
                    string ApplicationCompleteEmailMessage = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/ApplicationSubmitted.html")))
                    {
                        ApplicationCompleteEmailMessage = reader.ReadToEnd();
                    }
                    ApplicationCompleteEmailMessage = ApplicationCompleteEmailMessage.Replace("{CompanyName}", userInfo.OtherNames);

                    var ApplicationCompleteEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, CompanyEmail, "Application Complete", ApplicationCompleteEmailMessage);
                    if (ApplicationCompleteEmail == true)
                    {
                        //Log email sent notification
                        LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApplicationCompleteEmailMessage, CompanyEmail, _action);
                    }
                    else
                    {
                        //Log Email failed notification
                        LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApplicationCompleteEmailMessage, CompanyEmail, _action);
                    }

                    //4. Send All Nominated Saved Signatories an email (With CompanyID and ClientID)
                    var _dontSendEmail = db.AspNetUsers.Select(x => x.Email).ToList();
                    var SavedSignatories = (from p in db.ClientSignatories
                                            join e in db.RegisteredClients on p.ClientID equals e.Id
                                            where p.Status == 0 && p.ClientID == model.ClientID && p.CompanyID == model.CompanyID && !_dontSendEmail.Contains(p.EmailAddress)
                                            select new
                                            {
                                                EmailID = p.EmailAddress
                                            });
                    if (SavedSignatories != null)
                    {
                        foreach (var email in SavedSignatories.ToList())
                        {
                            var _OTPCode = OTPGenerator.GetUniqueKey(6);
                            string OTPCode = Shuffle.StringMixer(_OTPCode);
                            var SignatoryToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == email.EmailID && c.Status == 0 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                            SignatoryToUpdate.OTP = Functions.GenerateMD5Hash(OTPCode);
                            db.SaveChanges();

                            //Send Email With OTP logins
                            var callbackUrl = Url.Action("SignatoryConfirmation", "Account", null, Request.Url.Scheme);
                            string EmailBody2 = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/Content/emails/SignatoryNomination.html")))
                            {
                                EmailBody2 = reader.ReadToEnd();
                            }
                            EmailBody2 = EmailBody2.Replace("{SignatoryName}", SignatoryToUpdate.OtherNames);
                            EmailBody2 = EmailBody2.Replace("{URL}", callbackUrl);
                            EmailBody2 = EmailBody2.Replace("{ActivationCode}", OTPCode);

                            var EmailToSignatory2 = MailHelper.SendMailMessage(MailHelper.EmailFrom, SignatoryToUpdate.EmailAddress.ToLower(), "Confirm Signatory", EmailBody2);
                            if (EmailToSignatory2 == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, EmailBody2, SignatoryToUpdate.EmailAddress.ToLower(), _action);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, EmailBody2, SignatoryToUpdate.EmailAddress.ToLower(), _action);
                            }
                        }
                    }

                    //5. Send Email to Digital Desk Users
                    var DDUserRole = (from p in db.AspNetUserRoles
                                        join e in db.AspNetUsers on p.UserId equals e.Id
                                        where p.RoleId == "03d5e1e3-a8a9-441e-9122-30c3aafccccc"
                                        select new
                                        {
                                            EmailID = e.Email
                                        });
                    foreach (var email in DDUserRole.ToList())
                    {
                        var DDMessageBody = "Dear Team <br/><br/> Kindly note that the following client has successfully submitted an application. <br/>" +
                                        "Company Name: " + model.CompanyName + ", Company Email: " + CompanyEmail + ", Application Date: " + DateTime.Now + " " +
                                        "<br/><br/> Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                        var SendDDNotificationEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, email.EmailID, "Registration Complete", DDMessageBody);
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

                //If the 3 scenarios do not exist throw an error.
                else
                {
                    return Json("Error! Your application is invalid, please nominate signatories and representatives and try again.", JsonRequestBehavior.AllowGet);
                }
            } 
        }

        //
        // GET // Company Partial View
        public PartialViewResult _LoadCompanyDetails(int id)
        {
            using (DBModel db = new DBModel())
            {
                var CompanyDetails = db.ClientCompanies.SingleOrDefault(a => a.Id == id);
                ViewBag.CompanyDetails = CompanyDetails;

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == CompanyDetails.ClientId && a.CompanyID == id && a.Status != 4).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == CompanyDetails.ClientId && a.CompanyID == id && a.Status != 4).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.ClientID =  " + "'" + CompanyDetails.ClientId + "'" + " AND s.CompanyID =  " + "'" + id + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();
            }

            return PartialView();
        }

        //
        //Post
        //Save Signatories Details
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveSignatoriesInfo(ApplicationViewModel model, HttpPostedFileBase inputFile)
        {
            //Create account Signatory (Signatory 1)
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var RegisteredClientId = userInfo.Id;
                
                //Signatory 1
                if (model.SignatorySurname1 != null || model.SignatoryOtherNames1 != null || model.SignatoryEmail1 != null || model.SignatoryDesignation1 != null)
                {
                    //check if Signatory1 Exists
                    var Signatory1Exists = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail1 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                    if (Signatory1Exists == null) //Add New if not Exists
                    {
                        try
                        {
                            //Upload Signature
                            if (Request.Files.Count > 0)
                            {
                                var file = Request.Files[0];
                                if (file != null && inputFile.ContentLength > 0)
                                {
                                    var fileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + System.IO.Path.GetFileName(inputFile.FileName);
                                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), fileName);
                                    file.SaveAs(path);
                                }
                            }

                            //Add Details
                            var addSignatory1 = db.ClientSignatories.Create();
                            var newFileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + System.IO.Path.GetFileName(inputFile.FileName);
                            addSignatory1.ClientID = RegisteredClientId;
                            addSignatory1.Designation = model.SignatoryDesignation1;
                            addSignatory1.Surname = model.SignatorySurname1;
                            addSignatory1.OtherNames = model.SignatoryOtherNames1;
                            addSignatory1.EmailAddress = model.SignatoryEmail1.ToLower();
                            addSignatory1.PhoneNumber = model.SignatoryPhoneNumber1;
                            addSignatory1.Signature = newFileName;
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
                        try
                        {
                            //Upload Signature
                            if (Request.Files.Count > 0)
                            {
                                var file = Request.Files[0];
                                if (file != null && inputFile.ContentLength > 0)
                                {
                                    var fileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + System.IO.Path.GetFileName(inputFile.FileName);
                                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/signatures/"), fileName);
                                    file.SaveAs(path);
                                }
                            }
                            //Edit Details
                            var Signatory1ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail1);
                            var newFileName = DateTime.Now.ToString("yyyyMMdd") + RegisteredClientId + System.IO.Path.GetFileName(inputFile.FileName);
                            Signatory1ToUpdate.Status = 1;
                            Signatory1ToUpdate.Designation = model.SignatoryDesignation1;
                            Signatory1ToUpdate.Surname = model.SignatorySurname1;
                            Signatory1ToUpdate.OtherNames = model.SignatoryOtherNames1;
                            Signatory1ToUpdate.EmailAddress = model.SignatoryEmail1.ToLower();
                            Signatory1ToUpdate.PhoneNumber = model.SignatoryPhoneNumber1;
                            Signatory1ToUpdate.DateCreated = DateTime.Now;
                            Signatory1ToUpdate.AcceptedTerms = true;
                            Signatory1ToUpdate.Signature = newFileName;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }
                //Signatory 2
                if (model.SignatorySurname2 != null || model.SignatoryOtherNames2 != null || model.SignatoryEmail2 != null || model.SignatoryDesignation2 != null)
                {
                    //Check for signatory2
                    var Signatory2Exists = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail2 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                    if (Signatory2Exists == null) //Add New if not Exists
                    {
                        try
                        {
                            //Add New Details
                            var addSignatory2 = db.ClientSignatories.Create();
                            addSignatory2.ClientID = RegisteredClientId;
                            addSignatory2.Designation = model.SignatoryDesignation2;
                            addSignatory2.Surname = model.SignatorySurname2;
                            addSignatory2.OtherNames = model.SignatoryOtherNames2;
                            addSignatory2.EmailAddress = model.SignatoryEmail2.ToLower();
                            addSignatory2.PhoneNumber = model.SignatoryPhoneNumber2;
                            addSignatory2.DateCreated = DateTime.Now;
                            addSignatory2.Status = 0;
                            addSignatory2.AcceptedTerms = true;
                            db.ClientSignatories.Add(addSignatory2);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var Signatory2ToUpdate = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail2 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                            Signatory2ToUpdate.Status = 0;
                            Signatory2ToUpdate.Designation = model.SignatoryDesignation2;
                            Signatory2ToUpdate.Surname = model.SignatorySurname2;
                            Signatory2ToUpdate.OtherNames = model.SignatoryOtherNames2;
                            Signatory2ToUpdate.EmailAddress = model.SignatoryEmail2.ToLower();
                            Signatory2ToUpdate.PhoneNumber = model.SignatoryPhoneNumber2;
                            Signatory2ToUpdate.DateCreated = DateTime.Now;
                            Signatory2ToUpdate.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }
                //Signatory 3
                if (model.SignatorySurname3 != null || model.SignatoryOtherNames3 != null || model.SignatoryEmail3 != null || model.SignatoryDesignation3 != null)
                {
                    //Check for signatory3
                    var Signatory3Exists = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail3 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                    if (Signatory3Exists == null) 
                    {
                        //Add New if not Exists
                        try
                        {
                            //Add New Details
                            var addSignatory3 = db.ClientSignatories.Create();
                            addSignatory3.ClientID = RegisteredClientId;
                            addSignatory3.Designation = model.SignatoryDesignation3;
                            addSignatory3.Surname = model.SignatorySurname3;
                            addSignatory3.OtherNames = model.SignatoryOtherNames3;
                            addSignatory3.EmailAddress = model.SignatoryEmail3.ToLower();
                            addSignatory3.PhoneNumber = model.SignatoryPhoneNumber3;
                            addSignatory3.DateCreated = DateTime.Now;
                            addSignatory3.Status = 0;
                            addSignatory3.AcceptedTerms = true;
                            db.ClientSignatories.Add(addSignatory3);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var Signatory3ToUpdate = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail3 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                            Signatory3ToUpdate.Status = 0;
                            Signatory3ToUpdate.Designation = model.SignatoryDesignation3;
                            Signatory3ToUpdate.Surname = model.SignatorySurname3;
                            Signatory3ToUpdate.OtherNames = model.SignatoryOtherNames3;
                            Signatory3ToUpdate.EmailAddress = model.SignatoryEmail3.ToLower();
                            Signatory3ToUpdate.PhoneNumber = model.SignatoryPhoneNumber3;
                            Signatory3ToUpdate.DateCreated = DateTime.Now;
                            Signatory3ToUpdate.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }
                //Signatory 4
                if (model.SignatorySurname4 != null || model.SignatoryOtherNames4 != null || model.SignatoryEmail4 != null || model.SignatoryDesignation4 != null)
                { //Check for signatory4
                    var Signatory4Exists = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail4 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                    if (Signatory4Exists == null) //Add New if not Exists
                    {
                        try
                        {
                            //Add New Details
                            var addSignatory4 = db.ClientSignatories.Create();
                            addSignatory4.ClientID = RegisteredClientId;
                            addSignatory4.Designation = model.SignatoryDesignation4;
                            addSignatory4.Surname = model.SignatorySurname4;
                            addSignatory4.OtherNames = model.SignatoryOtherNames4;
                            addSignatory4.EmailAddress = model.SignatoryEmail4.ToLower();
                            addSignatory4.PhoneNumber = model.SignatoryPhoneNumber4;
                            addSignatory4.DateCreated = DateTime.Now;
                            addSignatory4.Status = 0;
                            addSignatory4.AcceptedTerms = true;
                            db.ClientSignatories.Add(addSignatory4);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var Signatory4ToUpdate = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail4 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                            Signatory4ToUpdate.Status = 0;
                            Signatory4ToUpdate.Designation = model.SignatoryDesignation4;
                            Signatory4ToUpdate.Surname = model.SignatorySurname4;
                            Signatory4ToUpdate.OtherNames = model.SignatoryOtherNames4;
                            Signatory4ToUpdate.EmailAddress = model.SignatoryEmail4.ToLower();
                            Signatory4ToUpdate.PhoneNumber = model.SignatoryPhoneNumber4;
                            Signatory4ToUpdate.DateCreated = DateTime.Now;
                            Signatory4ToUpdate.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }
                //Signatory 5
                if (model.SignatorySurname5 != null || model.SignatoryOtherNames5 != null || model.SignatoryEmail5 != null || model.SignatoryDesignation5 != null)
                {
                    //Check for signatory5
                    var Signatory5Exists = db.ClientSignatories.SingleOrDefault(s => s.EmailAddress == model.SignatoryEmail5 && s.ClientID == model.ClientID && s.CompanyID == model.CompanyID);
                    if (Signatory5Exists == null) //Add New if not Exists
                    {
                        try
                        {
                            //Add New Details
                            var addSignatory5 = db.ClientSignatories.Create();
                            addSignatory5.ClientID = RegisteredClientId;
                            addSignatory5.Designation = model.SignatoryDesignation5;
                            addSignatory5.Surname = model.SignatorySurname5;
                            addSignatory5.OtherNames = model.SignatoryOtherNames5;
                            addSignatory5.EmailAddress = model.SignatoryEmail5.ToLower();
                            addSignatory5.PhoneNumber = model.SignatoryPhoneNumber5;
                            addSignatory5.DateCreated = DateTime.Now;
                            addSignatory5.Status = 0;
                            addSignatory5.AcceptedTerms = true;
                            db.ClientSignatories.Add(addSignatory5);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }

                    }
                    else
                    {
                        try
                        {
                            var Signatory5ToUpdate = db.ClientSignatories.SingleOrDefault(c => c.EmailAddress == model.SignatoryEmail5 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                            Signatory5ToUpdate.Status = 0;
                            Signatory5ToUpdate.Designation = model.SignatoryDesignation5;
                            Signatory5ToUpdate.Surname = model.SignatorySurname5;
                            Signatory5ToUpdate.OtherNames = model.SignatoryOtherNames5;
                            Signatory5ToUpdate.EmailAddress = model.SignatoryEmail5.ToLower();
                            Signatory5ToUpdate.PhoneNumber = model.SignatoryPhoneNumber5;
                            Signatory5ToUpdate.DateCreated = DateTime.Now;
                            Signatory5ToUpdate.AcceptedTerms = true;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                return Json("success", JsonRequestBehavior.AllowGet);
            }
        }

        //Post
        //Save Signatories Details
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveRepresentativesInfo(ApplicationViewModel model)
        {
            //Create account Signatory (Signatory 1)
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var RegisteredClientId = userInfo.Id;

                //Log authorised representative (Representative 1)
                if (model.UserEmail1 != null && model.UserSurname1 != null && model.UserOthernames1 != null && model.UserMobileNumber1 != null)
                {
                    //Check if representative exists
                    var Representative1Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail1 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                    if (Representative1Exists == null)
                    {
                        //Create new representative if false
                        try
                        {
                            var addDesignatedUser1 = db.DesignatedUsers.Create();
                            addDesignatedUser1.Status = 0;
                            addDesignatedUser1.ClientID = RegisteredClientId;
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
                        //Update representative exists
                        var Representative1ToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail1 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                        Representative1ToUpdate.Status = 0;
                        Representative1ToUpdate.DateCreated = DateTime.Now;
                        Representative1ToUpdate.DOB = model.DOB1;
                        Representative1ToUpdate.Email = model.UserEmail1.ToLower();
                        Representative1ToUpdate.Surname = model.UserSurname1;
                        Representative1ToUpdate.Othernames = model.UserOthernames1;
                        Representative1ToUpdate.Mobile = model.UserMobileNumber1;
                        Representative1ToUpdate.POBox = model.UserPOBox1;
                        Representative1ToUpdate.ZipCode = model.UserZipCode1;
                        Representative1ToUpdate.Town = model.UserTownCity1;
                        Representative1ToUpdate.TradingLimit = model.TransactionLimit1;
                        Representative1ToUpdate.EMarketSignUp = model.EMarketSignUp1;
                        Representative1ToUpdate.AcceptedTerms = false;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative 2
                if (model.UserEmail2 != null && model.UserSurname2 != null && model.UserOthernames2 != null && model.UserMobileNumber2 != null)
                {
                    var Representative2Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail2 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                    if (Representative2Exists == null)
                    {
                        var addDesignatedUser2 = db.DesignatedUsers.Create();
                        addDesignatedUser2.Status = 0;
                        addDesignatedUser2.ClientID = RegisteredClientId;
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
                        //Update representative exists
                        var Representative2ToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail2 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                        Representative2ToUpdate.Status = 0;
                        Representative2ToUpdate.DateCreated = DateTime.Now;
                        Representative2ToUpdate.DOB = model.DOB2;
                        Representative2ToUpdate.Email = model.UserEmail2.ToLower();
                        Representative2ToUpdate.Surname = model.UserSurname2;
                        Representative2ToUpdate.Othernames = model.UserOthernames2;
                        Representative2ToUpdate.Mobile = model.UserMobileNumber2;
                        Representative2ToUpdate.POBox = model.UserPOBox2;
                        Representative2ToUpdate.ZipCode = model.UserZipCode2;
                        Representative2ToUpdate.Town = model.UserTownCity2;
                        Representative2ToUpdate.TradingLimit = model.TransactionLimit2;
                        Representative2ToUpdate.EMarketSignUp = model.EMarketSignUp2;
                        Representative2ToUpdate.AcceptedTerms = false;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative 3
                if (model.UserEmail3 != null && model.UserSurname3 != null && model.UserOthernames3 != null && model.UserMobileNumber3 != null)
                {
                    var Representative3Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail3 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                    if (Representative3Exists == null)
                    {
                        var addDesignatedUser3 = db.DesignatedUsers.Create();
                        addDesignatedUser3.Status = 0;
                        addDesignatedUser3.ClientID = RegisteredClientId;
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
                        //Update representative exists
                        var Representative3ToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail3 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                        Representative3ToUpdate.Status = 0;
                        Representative3ToUpdate.DateCreated = DateTime.Now;
                        Representative3ToUpdate.DOB = model.DOB3;
                        Representative3ToUpdate.Email = model.UserEmail3.ToLower();
                        Representative3ToUpdate.Surname = model.UserSurname3;
                        Representative3ToUpdate.Othernames = model.UserOthernames3;
                        Representative3ToUpdate.Mobile = model.UserMobileNumber3;
                        Representative3ToUpdate.POBox = model.UserPOBox3;
                        Representative3ToUpdate.ZipCode = model.UserZipCode3;
                        Representative3ToUpdate.Town = model.UserTownCity3;
                        Representative3ToUpdate.TradingLimit = model.TransactionLimit3;
                        Representative3ToUpdate.EMarketSignUp = model.EMarketSignUp3;
                        Representative3ToUpdate.AcceptedTerms = false;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative 4
                if (model.UserEmail4 != null && model.UserSurname4 != null && model.UserOthernames4 != null && model.UserMobileNumber4 != null)
                {
                    var Representative4Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail4 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                    if (Representative4Exists == null)
                    {
                        var addDesignatedUser4 = db.DesignatedUsers.Create();
                        addDesignatedUser4.Status = 0;
                        addDesignatedUser4.ClientID = RegisteredClientId;
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
                        var Representative4ToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail4 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                        Representative4ToUpdate.Status = 0;
                        Representative4ToUpdate.DateCreated = DateTime.Now;
                        Representative4ToUpdate.DOB = model.DOB4;
                        Representative4ToUpdate.Email = model.UserEmail4.ToLower();
                        Representative4ToUpdate.Surname = model.UserSurname4;
                        Representative4ToUpdate.Othernames = model.UserOthernames4;
                        Representative4ToUpdate.Mobile = model.UserMobileNumber4;
                        Representative4ToUpdate.POBox = model.UserPOBox4;
                        Representative4ToUpdate.ZipCode = model.UserZipCode4;
                        Representative4ToUpdate.Town = model.UserTownCity4;
                        Representative4ToUpdate.TradingLimit = model.TransactionLimit4;
                        Representative4ToUpdate.EMarketSignUp = model.EMarketSignUp4;
                        Representative4ToUpdate.AcceptedTerms = false;
                        db.SaveChanges();
                    }
                }

                //Log authorised Representative 5
                if (model.UserEmail5 != null && model.UserSurname5 != null && model.UserOthernames5 != null && model.UserMobileNumber5 != null)
                {
                    var Representative5Exists = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail5 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                    if (Representative5Exists == null)
                    {
                        var addDesignatedUser5 = db.DesignatedUsers.Create();
                        addDesignatedUser5.Status = 0;
                        addDesignatedUser5.ClientID = RegisteredClientId;
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
                        //Update representative exists
                        var Representative5ToUpdate = db.DesignatedUsers.SingleOrDefault(c => c.Email == model.UserEmail5 && c.ClientID == model.ClientID && c.CompanyID == model.CompanyID);
                        Representative5ToUpdate.Status = 0;
                        Representative5ToUpdate.DateCreated = DateTime.Now;
                        Representative5ToUpdate.DOB = model.DOB5;
                        Representative5ToUpdate.Email = model.UserEmail5.ToLower();
                        Representative5ToUpdate.Surname = model.UserSurname5;
                        Representative5ToUpdate.Othernames = model.UserOthernames5;
                        Representative5ToUpdate.Mobile = model.UserMobileNumber5;
                        Representative5ToUpdate.POBox = model.UserPOBox5;
                        Representative5ToUpdate.ZipCode = model.UserZipCode5;
                        Representative5ToUpdate.Town = model.UserTownCity5;
                        Representative5ToUpdate.TradingLimit = model.TransactionLimit5;
                        Representative5ToUpdate.EMarketSignUp = model.EMarketSignUp5;
                        Representative5ToUpdate.AcceptedTerms = false;
                        db.SaveChanges();
                    }
                }

                return Json("success", JsonRequestBehavior.AllowGet);
            }
        }

        //Post
        //Save First Draft Application
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveUserInfo(ApplicationViewModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            using (DBModel db = new DBModel())
            {
                //Update User Company information
                var updateCompanyInfo = db.ClientCompanies.SingleOrDefault(c => c.Id == model.CompanyID && c.Status == 1);
                if (updateCompanyInfo != null)
                {
                    try
                    {
                        //Update Registered Client Details
                        updateCompanyInfo.CompanyName = model.CompanyName;
                        updateCompanyInfo.CompanyRegNumber = model.CompanyRegistration;
                        updateCompanyInfo.CompanyBuilding = model.Building;
                        updateCompanyInfo.CompanyStreet = model.Street;
                        updateCompanyInfo.CompanyTownCity = model.CompanyTownCity;
                        updateCompanyInfo.BusinessEmailAddress = model.BusinessEmailAddress;
                        updateCompanyInfo.AttentionTo = model.AttentionTo;
                        updateCompanyInfo.Fax = model.CompanyFax;
                        updateCompanyInfo.PostalAddress = model.PostalAddress;
                        updateCompanyInfo.PostalCode = model.PostalCode;
                        updateCompanyInfo.TownCity = model.AddressTownCity;
                        var savedCompany = db.SaveChanges();
                        if (savedCompany > 0)
                        {
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Unable to update comany details!", JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
                else
                {
                    return Json("Unable to update client details!", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //Post //Save SaveSettlementAccounts
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveSettlementAccounts(ApplicationViewModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var RegisteredClientId = userInfo.Id;

                //Log New Settlement Account Details when yes is selected
                if (model.HaveSettlementAccount == "Yes")
                {
                    //Check if settlements exist
                    var SettlementAccountExists = db.ClientSettlementAccounts.Any(c => c.ClientID == RegisteredClientId && c.CompanyID == model.CompanyID);
                    if (SettlementAccountExists)
                    {
                        //Clear all if exists exist
                        db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(r => r.ClientID == RegisteredClientId && r.CompanyID == model.CompanyID));
                        db.SaveChanges();

                        //Add new SettlementAccounts 1-5 after clear
                        try
                        {
                            //Add SettlementAccount1
                            var newAccountDetails = db.ClientSettlementAccounts.Create();
                            if (model.SettlementAccount1 != null && model.SelectCurrency1 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount1;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType1;
                                newAccountDetails.CurrencyID = model.SelectCurrency1;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                                 
                            if (model.SettlementAccount2 != null && model.SelectCurrency2 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount2;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType2;
                                newAccountDetails.CurrencyID = model.SelectCurrency2;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount3 != null && model.SelectCurrency3 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount3;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType3;
                                newAccountDetails.CurrencyID = model.SelectCurrency3;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount4 != null && model.SelectCurrency4 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount4;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType4;
                                newAccountDetails.CurrencyID = model.SelectCurrency4;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount5 != null && model.SelectCurrency5 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount5;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType5;
                                newAccountDetails.CurrencyID = model.SelectCurrency5;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }

                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            return Json("" + ex.Message + "", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        try
                        {
                            //Add SettlementAccount 1-8 if exists
                            var newAccountDetails = db.ClientSettlementAccounts.Create();
                            if (model.SettlementAccount1 != null && model.SelectCurrency1 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount1;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType1;
                                newAccountDetails.CurrencyID = model.SelectCurrency1;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount2 != null && model.SelectCurrency2 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount2;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType2;
                                newAccountDetails.CurrencyID = model.SelectCurrency2;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount3 != null && model.SelectCurrency3 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount3;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType3;
                                newAccountDetails.CurrencyID = model.SelectCurrency3;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount4 != null && model.SelectCurrency4 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount4;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType4;
                                newAccountDetails.CurrencyID = model.SelectCurrency4;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }
                            if (model.SettlementAccount5 != null && model.SelectCurrency5 != null)
                            {
                                newAccountDetails.ClientID = RegisteredClientId;
                                newAccountDetails.CompanyID = model.CompanyID;
                                newAccountDetails.AccountNumber = model.SettlementAccount5;
                                newAccountDetails.OtherCurrency = model.InputCurrencyType5;
                                newAccountDetails.CurrencyID = model.SelectCurrency5;
                                newAccountDetails.Status = 1;
                                db.ClientSettlementAccounts.Add(newAccountDetails);
                                db.SaveChanges();
                            }

                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            return Json("" + ex.Message + "", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                //If the client has chosen NO (does not to have settlement accounts) and in case he had indicated
                else
                {
                    try
                    {
                        var SettlementAccountsExist = db.ClientSettlementAccounts.Any(c => c.ClientID == RegisteredClientId && c.CompanyID == model.CompanyID);
                        if (SettlementAccountsExist)
                        {
                            db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(r => r.ClientID == RegisteredClientId && r.CompanyID == model.CompanyID));
                            db.SaveChanges(); 
                        }
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        return Json(ex, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        //Post
        //Clear Settlement accounts for users to add new
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ClearSettlementAccounts(ApplicationViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var SettlementAccountsExist = db.ClientSettlementAccounts.Any(c => c.ClientID == userInfo.Id && c.CompanyID == model.CompanyID);
                if (SettlementAccountsExist)
                {
                    db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(r => r.ClientID == userInfo.Id && r.CompanyID == model.CompanyID));
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to delete settlement accounts", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("No settlement accounts to delete", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Post
        //Clear Settlement accounts for users to add new
        [HttpPost]
        [AllowAnonymous]
        public ActionResult RemoveSettlementAccount(string account)
        {
            using (DBModel db = new DBModel())
            {
                var SettlementAccountsExist = db.ClientSettlementAccounts.Any(c => c.AccountNumber == account);
                if (SettlementAccountsExist)
                {
                    db.ClientSettlementAccounts.RemoveRange(db.ClientSettlementAccounts.Where(r => r.AccountNumber == account));
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to delete settlement", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("No settlement to delete", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ClearSignatories(ApplicationViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var ClientSignatoriesExist = db.ClientSignatories.Any(c => c.ClientID == userInfo.Id && c.CompanyID == model.CompanyID);
                if (ClientSignatoriesExist)
                {
                    db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(r => r.ClientID == userInfo.Id && r.CompanyID == model.CompanyID));
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to delete signatories", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("No signatories to delete", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //Remove Signatories on click of removebtn1-5
        [HttpPost]
        [AllowAnonymous]
        public ActionResult RemoveSignatory(string email)
        {
            using (DBModel db = new DBModel())
            {
                var ClientSignatoriesExist = db.ClientSignatories.Any(c => c.EmailAddress == email);
                if (ClientSignatoriesExist)
                {
                    db.ClientSignatories.RemoveRange(db.ClientSignatories.Where(r => r.EmailAddress == email));
                    var recordSaved = db.SaveChanges();
                    if (recordSaved > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to delete signatory", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("No signatory to delete", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //List Applications
        [HttpGet]
        public ActionResult ViewAll()
        {
            return View(GetAllApplications());
        }

        //
        //Get All Applications List from table
        public IEnumerable<ClientApplicationsViewModel> GetAllApplications()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                var Query = from a in db.EMarketApplications.Where(a => a.ClientID == userInfo.Id)
                            join b in db.RegisteredClients on a.ClientID equals b.Id
                            join c in db.tblStatus on a.Status equals c.Id
                            join d in db.ClientCompanies on a.CompanyID equals d.Id
                            orderby a.DateCreated descending
                            select new ClientApplicationsViewModel
                            {
                                ApplicationID = a.Id,
                                Client = d.CompanyName,
                                CompanyID = d.Id,
                                Status = c.StatusName,
                                AcceptedTAC = a.AcceptedTAC,
                                DateCreated = a.DateCreated,
                                OPSApproved = a.OPSApproved,
                                OPSDeclined = a.OPSDeclined,
                                OPSComments = a.OPSComments,
                                POAApproved = a.POAApproved,
                                POADeclined = a.POADeclined,
                                POADateApproved = a.POADateApproved,
                                Signatories = a.Signatories,
                                DesignatedUsers = a.DesignatedUsers,
                                DateDeclined = a.DateDeclined
                            };
                return Query.ToList();
            }
        }

        //
        //Get All Currencies
        public JsonResult FetchCurrency()
        {
            using (DBModel db = new DBModel())
            {
                var currencies = db.Currencies.ToList();
                var json = from currency in currencies
                           select new
                           {
                               text = currency.CurrencyName,
                               id = currency.Id,
                           };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }

        //
        //GET //GetCurrencyList Dropdown Select 2
        public ActionResult GetCurrencyList(string q)
        {
            using (DBModel db = new DBModel())
            {
                var list = (from a in db.Currencies.Where(r => r.Status == 1).OrderBy(x => x.CurrencyShort)
                         select new Select2Model
                         {
                             text = a.CurrencyShort + " - " + a.CurrencyName,
                             id = a.Id
                         }).ToList();

                if (!(string.IsNullOrEmpty(q) || string.IsNullOrWhiteSpace(q)))
                {
                    list = list.Where(x => x.text.ToLower().StartsWith(q.ToLower())).ToList();
                }

                return Json(new { items = list }, JsonRequestBehavior.AllowGet);
            }
        }


        //
        //GET //GetCompanyList Dropdown Select 2
        public ActionResult GetCompanyList(string q)
        {
            using (DBModel db = new DBModel())
            {
                var userId = User.Identity.GetUserId();
                var userInfo = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == userId);
                var list = (from a in db.ClientCompanies.Where(r => r.Status == 1 && r.ClientId == userInfo.Id && r.HasApplication == false).OrderByDescending(x => x.Id)
                            select new Select2Model
                            {
                                text = a.CompanyName + " - " + a.CompanyRegNumber,
                                id = a.Id
                            }).ToList();

                if (!(string.IsNullOrEmpty(q) || string.IsNullOrWhiteSpace(q)))
                {
                    list = list.Where(x => x.text.ToLower().StartsWith(q.ToLower())).ToList();
                }
                return Json(new { items = list }, JsonRequestBehavior.AllowGet);
            }
        }
        //
        //EditApplication 
        public ActionResult LoadDeleteSignatory(int getSignatoryId)
        {
            using (DBModel db = new DBModel())
            {
                var getSignatoryInfo = db.ClientSignatories.SingleOrDefault(c => c.Id == getSignatoryId && c.Status == 1);
                //Data For View Display
                ViewData["Names"] = getSignatoryInfo.Surname + " " + getSignatoryInfo.OtherNames;
                ViewData["EmailAddress"] = getSignatoryInfo.EmailAddress;
                ViewData["UserId"] = getSignatoryInfo.Id;
                ViewData["ClientId"] = getSignatoryInfo.ClientID;
            }
            return PartialView();
        }

        //
        //EditApplication //LoadDeleteRepresentative
        public ActionResult LoadDeleteRepresentative(int getRepresentativeId)
        {
            using (DBModel db = new DBModel())
            {
                var getRepresentativeInfo = db.DesignatedUsers.SingleOrDefault(c => c.Id == getRepresentativeId && c.Status == 1);
                
                //Data For View Display
                ViewData["Names"] = getRepresentativeInfo.Surname + " " + getRepresentativeInfo.Othernames;
                ViewData["EmailAddress"] = getRepresentativeInfo.Email;
                ViewData["UserId"] = getRepresentativeInfo.Id;
                ViewData["ClientId"] = getRepresentativeInfo.ClientID;
            }
            return PartialView();
        }

        //
        //EditApplication //ExcludeSignatoryFromApplication
        public ActionResult ExcludeSignatoryFromApplication(DeleteSignatoryViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    //Mark Signatory as Deleted
                    var getSignatoryToExclude = db.ClientSignatories.SingleOrDefault(c => c.Id == model.SignatoryId);
                    getSignatoryToExclude.Status = 4;
                    var userDeleted = db.SaveChanges();
                    if (userDeleted > 0)
                    {
                        bool isThereApproval = db.SignatoryApprovals.Any(c => c.SignatoryID == model.SignatoryId);
                        if (isThereApproval)
                        {
                            //Remove accepted nomination details if exist
                            db.SignatoryApprovals.RemoveRange(db.SignatoryApprovals.Where(r => r.SignatoryID == model.SignatoryId));
                            var nominationDeleted = db.SaveChanges();
                            if (nominationDeleted > 0)
                            {
                                //Reduce the number of signatories in application
                                var applicationId = db.SignatoryApprovals.SingleOrDefault(c => c.ApplicationID == model.SignatoryId);
                                var applicationToEdit = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId.ApplicationID && c.Status == 1);
                                applicationToEdit.Signatories = applicationToEdit.Signatories - 1;
                                var applicationEdited = db.SaveChanges();
                                if (applicationEdited > 0)
                                {
                                    //Send an email
                                    return Json("success", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json("Error! Unable to edit application details", JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json("Error! Unable to clear signatory nomination", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            //2. If user has not yet approved nomination
                            var applicationToEdit = db.EMarketApplications.SingleOrDefault(c => c.ClientID == model.ClientId && c.Status == 1);
                            applicationToEdit.Signatories = applicationToEdit.Signatories - 1;
                            var applicationEdited = db.SaveChanges();
                            if (applicationEdited > 0)
                            {
                                //Send an email
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("Error! Unable to edit application details", JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to delete signatory", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json("Error! Unable to exclude signatory from application", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //EditApplication //ExcludeSignatoryFromApplication
        public ActionResult ExcludeRepresentativeFromApplication(DeleteSignatoryViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    //Mark Signatory as Deleted
                    var getSignatoryToExclude = db.DesignatedUsers.SingleOrDefault(c => c.Id == model.SignatoryId);
                    getSignatoryToExclude.Status = 4;
                    var userDeleted = db.SaveChanges();
                    if (userDeleted > 0)
                    {
                        bool isThereApproval = db.DesignatedUserApprovals.Any(c => c.UserID == model.SignatoryId);
                        if (isThereApproval)
                        {
                            //1. If user has approved nomination
                            //Remove accepted nomination details if exist
                            db.DesignatedUserApprovals.RemoveRange(db.DesignatedUserApprovals.Where(r => r.UserID == model.SignatoryId));
                            var nominationDeleted = db.SaveChanges();
                            if (nominationDeleted > 0)
                            {
                                //Reduce the number of signatories in application by Id
                                var applicationId = db.DesignatedUserApprovals.SingleOrDefault(c => c.ApplicationID == model.SignatoryId);
                                var applicationToEdit = db.EMarketApplications.SingleOrDefault(c => c.Id == applicationId.ApplicationID && c.Status == 1);
                                applicationToEdit.Signatories = applicationToEdit.DesignatedUsers - 1;
                                var applicationEdited = db.SaveChanges();
                                if (applicationEdited > 0)
                                {
                                    //Send an email
                                    return Json("success", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json("Error! Unable to edit application details", JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json("Error! Unable to clear signatory nomination", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            //2. If user has not yet approved nomination
                            var applicationToEdit = db.EMarketApplications.SingleOrDefault(c => c.ClientID == model.ClientId && c.Status == 1);
                            applicationToEdit.DesignatedUsers = applicationToEdit.DesignatedUsers - 1;
                            var applicationEdited = db.SaveChanges();
                            if (applicationEdited > 0)
                            {
                                //Send an email
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("Error! Unable to edit application details", JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json("Error! Unable to delete signatory", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST: DeleteSignatory
        public ActionResult DeleteSignatory(DeleteUserViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var getSignatoryToDelete = db.AspNetUsers.SingleOrDefault(c => c.Id == model.UserId);
                    getSignatoryToDelete.Status = 4;
                    var userDeleted = db.SaveChanges();
                    if (userDeleted > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! Unable to delete user", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST: //EditApplication
        public ActionResult EditApplication(int ApplicationID, int CompanyID)
        {
            using (DBModel db = new DBModel())
            {
                var getApplicationInfo = db.EMarketApplications.SingleOrDefault(c => c.Id == ApplicationID && c.CompanyID == CompanyID && c.Status == 1);
                var clientID = getApplicationInfo.ClientID;
                var clientDetails = db.RegisteredClients.SingleOrDefault(s => s.Id == clientID);
                var clientCompanyDetails = db.ClientCompanies.SingleOrDefault(s => s.Id == CompanyID);
                ViewBag.ApplicationInfo = clientDetails;
                ViewBag.CompanyInfo = clientCompanyDetails;
                
                //Data For Controller Post
                ViewData["ApplicationId"] = getApplicationInfo.Id;
                ViewData["CompanyEmail"] = clientDetails.EmailAddress;

                //Get the list of all client's settlement accounts
                var Query = db.Database.SqlQuery<SettlementAccountsViewModel>("SELECT c.CurrencyName, s.AccountNumber, s.CurrencyId FROM ClientSettlementAccounts s INNER JOIN Currencies c ON c.Id = s.CurrencyID WHERE s.Status = 1 AND s.ClientID =  " + "'" + clientDetails.Id + "'" + " AND  s.CompanyID =  " + "'" + CompanyID + "'" + " AND s.Status = 1");
                ViewBag.SettlementAccounts = Query.ToList();

                //Signatories List
                List<ClientSignatory> SignatoryList = db.ClientSignatories.Where(a => a.ClientID == clientDetails.Id && a.CompanyID == CompanyID && a.Status != 4).ToList();
                ViewBag.ClientSignatory = SignatoryList;

                //Designated Users List
                List<DesignatedUser> DesignatedUsersList = db.DesignatedUsers.Where(a => a.ClientID == clientDetails.Id && a.CompanyID == CompanyID && a.Status != 4).ToList();
                ViewBag.DesignatedUser = DesignatedUsersList;
            }
            return PartialView();
        }

        //
        // GET: Client Companies
        [Authorize]
        public ActionResult AddCompanies()
        {
            using (DBModel db = new DBModel())
            {
                var currentUserId = User.Identity.GetUserId();
                var _userId = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                ViewData["ClientID"] = _userId.Id;
            }

            return View();
        }

        //
        //Post EditApplication
        public ActionResult PostAddCompanies(ApplicationViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var currentUserId = User.Identity.GetUserId();
                    var _userId = db.RegisteredClients.SingleOrDefault(c => c.UserAccountID == currentUserId);
                    var newClientCompany = db.ClientCompanies.Create();
                    newClientCompany.ClientId = model.ClientID;
                    newClientCompany.CompanyName = model.CompanyName;
                    newClientCompany.CompanyRegNumber = model.CompanyRegistration;
                    newClientCompany.CompanyBuilding = model.Building;
                    newClientCompany.CompanyStreet = model.Street;
                    newClientCompany.CompanyTownCity = model.CompanyTownCity;
                    newClientCompany.BusinessEmailAddress = model.BusinessEmailAddress;
                    newClientCompany.AttentionTo = model.AttentionTo;
                    newClientCompany.Fax = model.CompanyFax;
                    newClientCompany.PostalAddress = model.PostalAddress;
                    newClientCompany.PostalCode = model.PostalCode;
                    newClientCompany.TownCity = model.AddressTownCity;
                    newClientCompany.CreatedBy = _userId.EmailAddress;
                    newClientCompany.Status = 1;
                    db.ClientCompanies.Add(newClientCompany);
                    var savedCompany = db.SaveChanges();
                    if(savedCompany > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Unable to create company. Please contact systems administrator", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception)
                {
                    return Json("Unable to create company. Please contact systems administrator", JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
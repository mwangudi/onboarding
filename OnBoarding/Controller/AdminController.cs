using OnBoarding.Models;
using OnBoarding.Services;
using OnBoarding.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;

namespace OnBoarding.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminController() { }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: Admin index
        public ActionResult Index()
        {
            using (DBModel db = new DBModel())
            {
                var TotalUsers = db.AspNetUsers.Count(a => a.Status == 1);
                var TotalUserRoles = db.AspNetRoles.Count(a => a.Status == 1);
                var TotalLockedDisabledUsers = db.AspNetUsers.Count(a => a.Status != 1);

                ViewData["TotalUsers"] = TotalUsers;
                ViewData["TotalUserRoles"] = TotalUserRoles;
                ViewData["TotalLockedDisabledUsers"] = TotalLockedDisabledUsers;
            }
            return View();
        }

        //
        //Get: ManageUsers
        public ActionResult ManageInternalUsers()
        {
            return View();
        }

        //
        //GET /Get Users Count
        public int GetInternalUsersCount()
        {
            using (var db = new DBModel())
            {
                var UserId = User.Identity.GetUserId();
                var Query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.Id <> '3c162824-045d-429a-b530-173adecb7585' AND r.Id NOT IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25', '4796025b-6669-4b61-88eb-23662e0aab58');");
                return Query.Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<UserListViewModel> GetInternalUsersList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                var UserId = User.Identity.GetUserId();
                IEnumerable<UserListViewModel> query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.Id <> '3c162824-045d-429a-b530-173adecb7585' AND r.Id NOT IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25', '4796025b-6669-4b61-88eb-23662e0aab58') ORDER BY u.DateCreated DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search 
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.Email LIKE '%" + searchMessage + "%' OR u.UserName LIKE '%" + searchMessage + "%' OR u.CompanyName LIKE '%" + searchMessage + "%' ORDER BY u.DateCreated DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                else if (jtSorting.Equals("Email ASC"))
                {
                    query = query.OrderBy(p => p.Email);
                }
                else if (jtSorting.Equals("Email DESC"))
                {
                    query = query.OrderByDescending(p => p.Email);
                }
                else if (jtSorting.Equals("RoleName ASC"))
                {
                    query = query.OrderBy(p => p.RoleName);
                }
                else if (jtSorting.Equals("RoleName DESC"))
                {
                    query = query.OrderByDescending(p => p.RoleName);
                }
                else if (jtSorting.Equals("UserName ASC"))
                {
                    query = query.OrderBy(p => p.UserName);
                }
                else if (jtSorting.Equals("UserName DESC"))
                {
                    query = query.OrderByDescending(p => p.UserName);
                }
                else if (jtSorting.Equals("PhoneNumber ASC"))
                {
                    query = query.OrderBy(p => p.PhoneNumber);
                }
                else if (jtSorting.Equals("PhoneNumber DESC"))
                {
                    query = query.OrderByDescending(p => p.PhoneNumber);
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
        //GET //Get Data List
        [HttpPost]
        public JsonResult GetSystemUsers(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = GetInternalUsersList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(u.Id) count FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.Email LIKE '%" + searchMessage + "%' OR u.UserName LIKE '%" + searchMessage + "%' OR u.CompanyName LIKE '%" + searchMessage + "%'").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetInternalUsersCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }
        
        //
        // POST: /Admin/LoadEditUser
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadEditUser(string getUserId)
        {
            using (DBModel db = new DBModel())
            {
                var getUserInfo = db.AspNetUsers.SingleOrDefault(c => c.Id == getUserId);
                var getUserRoleId = db.AspNetUserRoles.SingleOrDefault(c => c.UserId == getUserId);
                var getRoleName = db.AspNetRoles.SingleOrDefault(r => r.Id == getUserRoleId.RoleId);
                ViewBag.UserDetails = getUserInfo;
                ViewData["RoleName"] = getRoleName.Name;
            }
            return PartialView();
        }

        //
        // POST: /Admin/EditUser
        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditUser(EditUserViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    //Update User Details
                    var getUserUpdate = db.AspNetUsers.SingleOrDefault(c => c.Id == model.getUserId);
                    getUserUpdate.CompanyName = model.EditUserNames;
                    getUserUpdate.Email = model.EditEmail;
                    getUserUpdate.StaffNumber = model.EditStaffNumber;
                    getUserUpdate.PhoneNumber = model.EditPhoneNumber;
                    getUserUpdate.Status = model.EditTModeStatus;
                    db.SaveChanges();

                    //Update User Role Details
                    var getNewRole = db.AspNetRoles.SingleOrDefault(c => c.Name == model.EditUserRole);
                    var getUserRoleToUpdate = db.AspNetUserRoles.SingleOrDefault(c => c.UserId == model.getUserId);
                    getUserRoleToUpdate.RoleId = getNewRole.Id;
                    db.SaveChanges();

                    return Json("success", JsonRequestBehavior.AllowGet);

                }
                catch (Exception)
                {
                    return Json("Error! Unable to edit user details", JsonRequestBehavior.AllowGet);

                }
            }
        }

        //
        //Get: Manage User Roles
        public ActionResult ManageUserRoles()
        {
            return View();
        }

        //
        //GET //Get Users Count
        public int GetUserRolesCount()
        {
            using (var db = new DBModel())
            {
                var UserId = User.Identity.GetUserId();
                var Query = db.Database.SqlQuery<UserRolesViewModel>("SELECT count(a.id) count FROM AspNetRoles a INNER JOIN tblStatus s ON s.Id = a.Status;");
                return Query.Count();
            }
        }

        //
        //GET /Get Notifications List
        public List<UserRolesViewModel> UserRolesList(string searchMessage, int jtStartIndex, int jtPageSize, int count, string jtSorting)
        {
            // Instance of DatabaseContext  
            using (var db = new DBModel())
            {
                var UserId = User.Identity.GetUserId();
                IEnumerable<UserRolesViewModel> query = db.Database.SqlQuery<UserRolesViewModel>("SELECT a.id, a.Name, a.DateCreated, s.StatusName FROM AspNetRoles a INNER JOIN tblStatus s ON s.Id = a.Status ORDER BY a.DateCreated DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");

                //Search 
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<UserRolesViewModel>("SELECT a.id, a.Name, a.DateCreated, s.StatusName FROM AspNetRoles a INNER JOIN tblStatus s ON s.Id = a.Status WHERE a.Name LIKE '%" + searchMessage + "%' ORDER BY a.DateCreated DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY;");
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
                else if (jtSorting.Equals("StatusName ASC"))
                {
                    query = query.OrderBy(p => p.StatusName);
                }
                else if (jtSorting.Equals("StatusName DESC"))
                {
                    query = query.OrderByDescending(p => p.StatusName);
                }
                else if (jtSorting.Equals("Name ASC"))
                {
                    query = query.OrderBy(p => p.Name);
                }
                else if (jtSorting.Equals("Name DESC"))
                {
                    query = query.OrderByDescending(p => p.Name);
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
        //GET //Get Data List
        [HttpPost]
        public JsonResult GetUserRoles(string searchMessage = "", int jtStartIndex = 0, int jtPageSize = 0, int count = 0, string jtSorting = null)
        {
            using (var db = new DBModel())
            {
                var data = UserRolesList(searchMessage, jtStartIndex, jtPageSize, count, jtSorting);
                //Search  
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    var recordCount = db.Database.SqlQuery<int>("SELECT count(a.id) count FROM AspNetRoles a INNER JOIN tblStatus s ON s.Id = a.Status WHERE a.Name LIKE '%" + searchMessage + "%'").First();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
                else
                {
                    var recordCount = GetUserRolesCount();
                    return Json(new { Result = "OK", Records = data, TotalRecordCount = recordCount });
                }
            }
        }

        //
        //Post: //AddNew User Role
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewRole(PostRolesViewModel model)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (ModelState.IsValid)
            {
                if (!roleManager.RoleExists(model.Name))
                {
                    try
                    {
                        // Creating New Role   
                        var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                        role.Name = model.Name;
                        roleManager.Create(role);
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        return Json( ex.Message, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Error! Role exists. Unable to create new user role", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Error! Invalid inputs. Unable to create new user", JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /Admin/EditUserRoles
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadEditUserRoles(string getRoleId)
        {
            using (DBModel db = new DBModel())
            {
                var getRoleInfo = db.AspNetRoles.SingleOrDefault(c => c.Id == getRoleId);

                //Data For View Display
                ViewData["RoleId"] = getRoleInfo.Id;
                ViewData["RoleName"] = getRoleInfo.Name;
                ViewData["Status"] = getRoleInfo.Status;

            }
            return PartialView();
        }

        //
        // POST: /Admin/EditUserRoles
        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditUserRoles(EditRoleViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    var getRoleUpdate = db.AspNetRoles.SingleOrDefault(c => c.Id == model.EditId);
                    getRoleUpdate.Name = model.EditName;
                    getRoleUpdate.Status = model.EditTModeStatus;
                    db.SaveChanges();

                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                catch(Exception)
                {
                    return Json("Error! Unable to create new user", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST: //Get All User Roles
        public ActionResult GetUserRoles(string q)
        {
            using (DBModel db = new DBModel())
            {
                var list = (from a in db.AspNetRoles.Where(r => r.Status == 1)
                            select new GetUserRolesSelect2Model
                            {
                                text = a.Name,
                                id = a.Name
                            }).ToList();

                if (!(string.IsNullOrEmpty(q) || string.IsNullOrWhiteSpace(q)))
                {
                    list = list.Where(x => x.text.ToLower().StartsWith(q.ToLower())).ToList();
                }

                return Json(new { items = list }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        //POST: //AddNewUser
        public async Task<ActionResult> AddNewUser(PostUsersViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (DBModel db = new DBModel())
                {
                    //Check if Email or Username provided Exists
                    var CheckEmailExists = db.AspNetUsers.Any(i => i.Email == model.Email || i.UserName == model.Email);
                    var _action = "AddNewUser";
                    if (CheckEmailExists)
                    {
                        //Return error
                        return Json("User exists! Unable to create new user", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //Create User
                        var PasswordResetCode = OTPGenerator.GetUniqueKey(6);
                        string mixedOriginal = Shuffle.StringMixer(PasswordResetCode);
                        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, CompanyName = model.CompanyName, PhoneNumber = model.PhoneNumber, StaffNumber = model.StaffNumber, PasswordResetCode = Functions.GenerateMD5Hash(mixedOriginal) };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            //Add EMT user role
                            UserManager.AddToRole(user.Id, model.UserRole);

                            //Send Success Email to reset password with OTP
                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ResetPassword", "Account", null, Request.Url.Scheme);
                            var PasswordResetMessageBody = "Dear " + model.CompanyName + ", <br/><br/> You have been created as a user at Global Markets Onboarding Portal." +
                                        "<a href=" + callbackUrl + "> Click here to reset your password. </a>  Your Reset Code is: " + mixedOriginal + " <br/><br/>" +
                                        "Kind Regards,<br/><img src=\"https://e-documents.stanbicbank.co.ke/Content/images/EmailSignature.png\"/>";
                            var PasswordResetEmail = MailHelper.SendMailMessage(MailHelper.EmailFrom, model.Email, "New User: Password Reset", PasswordResetMessageBody);
                            if (PasswordResetEmail == true)
                            {
                                //Log email sent notification
                                LogNotification.AddSucsessNotification(MailHelper.EmailFrom, PasswordResetMessageBody, model.Email, _action);
                            }
                            else
                            {
                                //Log Email failed notification
                                LogNotification.AddFailureNotification(MailHelper.EmailFrom, PasswordResetMessageBody, model.Email, _action);
                            }
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Unable to create new user", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            else
            {
                return Json("Invalid form input. Unable to create new user", JsonRequestBehavior.AllowGet);
            }
        }

        //
        //Get: Manage User Roles
        public ActionResult UnlockUsers()
        {
            return View();
        }

        //
        //GET /Get Notifications Count
        public int GetLockedUsersCount()
        {
            using (var db = new DBModel())
            {
                var queryCount = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName, u.AccessFailedCount FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.LockoutEndDateUtc IS NOT NULL AND r.Id NOT IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25')").Count();
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
                IEnumerable<UserListViewModel> query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName, u.AccessFailedCount FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.LockoutEndDateUtc IS NOT NULL AND r.Id NOT IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25') ORDER BY u.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY");

                //Search 
                if (!string.IsNullOrEmpty(searchMessage))
                {
                    query = db.Database.SqlQuery<UserListViewModel>("SELECT u.Id, u.Email, u.PhoneNumber, u.CompanyName, u.DateCreated, u.UserName, s.StatusName, r.Name AS RoleName, u.AccessFailedCount FROM AspNetUsers u LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId INNER JOIN tblStatus s ON s.Id = u.Status WHERE u.Email LIKE '%" + searchMessage + "%' AND u.LockoutEndDateUtc IS NOT NULL AND r.Id NOT IN ('8f70018b-22c2-4ef8-b465-ceefc7df3afb','aa145382-378e-49df-bf06-c96e081d2466','d97260b8-3879-403e-9f08-b388e91c0a25') ORDER BY u.Id DESC OFFSET " + jtStartIndex + " ROWS FETCH NEXT " + jtPageSize + " ROWS ONLY");
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
                else if (jtSorting.Equals("DateCreated ASC"))
                {
                    query = query.OrderByDescending(p => p.DateCreated);
                }
                else if(jtSorting.Equals("StatusName ASC"))
                {
                    query = query.OrderBy(p => p.StatusName);
                }
                else if (jtSorting.Equals("StatusName DESC"))
                {
                    query = query.OrderByDescending(p => p.StatusName);
                }
                else
                {
                    query = query.OrderByDescending(p => p.DateCreated); //Default!  
                }
                //StatusName RoleName
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
        //POST //Load Partial View
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
                try
                {
                    var getUserToUnlock = db.AspNetUsers.SingleOrDefault(c => c.Id == model.UserId);
                    var _action = "UnlockUser";
                    getUserToUnlock.AccessFailedCount = 0;
                    getUserToUnlock.LockoutEndDateUtc = null;
                    getUserToUnlock.Status = 1;
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
                            LogNotification.AddSucsessNotification(MailHelper.EmailFrom, ApplicationApprovedEmailMessage, getUserToUnlock.Email, _action);
                        }
                        else
                        {
                            //Log Email failed notification
                            LogNotification.AddFailureNotification(MailHelper.EmailFrom, ApplicationApprovedEmailMessage, getUserToUnlock.Email, _action);
                        }
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! Unable to unlock user", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        //POST LoadDeleteUser
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LoadDeleteUser(string getUserId)
        {
            using (DBModel db = new DBModel())
            {
                var getUserInfo = db.AspNetUsers.SingleOrDefault(c => c.Id == getUserId);

                //Data For View Display
                ViewData["CompanyName"] = getUserInfo.CompanyName;
                ViewData["UserEmail"] = getUserInfo.Email;
                ViewData["UserId"] = getUserInfo.Id;
            }

            return PartialView();
        }

        //
        //Delete User
        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeleteUser(DeleteUserViewModel model)
        {
            using (DBModel db = new DBModel())
            {
                try
                {
                    //db.AspNetUsers.RemoveRange(db.AspNetUsers.Where(r => r.Id == model.UserId));
                    //var getUserToDelete = db.AspNetUsers.SingleOrDefault(c => c.Id == model.UserId);
                    var getUserToDelete = db.AspNetUsers.SingleOrDefault(c => c.Id == model.UserId);
                    getUserToDelete.AccessFailedCount = 4;
                    getUserToDelete.LockoutEndDateUtc = Convert.ToDateTime("2030/12/31 11:43:52 AM");
                    getUserToDelete.Status = 2;
                    var userDeleted = db.SaveChanges();
                    if(userDeleted > 0)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error! Unable to delete user", JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
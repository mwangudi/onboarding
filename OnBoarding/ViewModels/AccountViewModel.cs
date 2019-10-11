using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnBoarding.ViewModels
{
    public class UserAccountModel
    {
       public LoginViewModel loginModel { get; set; }
       public RegisterViewModel registerModel { get; set; }
    }

    public class ExternalLoginConfirmationViewModel
    {       
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {       
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    public class ResendOTPViewModel
    {
        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public string CompanyName { get; set; }
    }

    public class LoginViewModel
    {
        [Display(Name = "Email/Username")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Enter Password")]
        [Required]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Surname is required")]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Other names are required")]
        [Display(Name = "Other Names")]
        public string Othernames { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        //[RegularExpression(@"^0[0-9]{12}$", ErrorMessage = "Please enter a valid Stanbic Bank Account Number.")]
        [RegularExpression(@"^010[0-9]{10}$", ErrorMessage = "Please enter a valid Stanbic Bank Account Number.")]
        [DataType(DataType.Text)]
        [Display(Name = "Stanbic Account Number")]
        public string StanbicAccountNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [DataType(DataType.Text)]
        [Display(Name = "Your Company Name")]
        public string CompanyBusinessName { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the Terms and Conditions to proceed")]
        public bool terms { get; set; }
    }
    public class UploadedClientCompleteRegistrationViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string UploadedEmail { get; set; }

        [Required]
        //[RegularExpression(@"^[0-9]{13}", ErrorMessage = "Please enter a valid Stanbic Bank Account Number.")]
        //[RegularExpression(@"^0[0-9]{12}$", ErrorMessage = "Please enter a valid Stanbic Bank Account Number.")]
        [RegularExpression(@"^010[0-9]{10}$", ErrorMessage = "Please enter a valid Stanbic Bank Account Number.")]
        [DataType(DataType.Text)]
        [Display(Name = "Your Stanbic Account Number")]
        public string StanbicAccountNumber { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [Display(Name = "One Time Pin")]
        [RegularExpression(@"^[a-zA-Z0-9\s,]*$", ErrorMessage = "Incorrect OTP")]
        [StringLength(6, ErrorMessage = "Incorrect OTP", MinimumLength = 2)]
        public string OTP { get; set; }

        [Display(Name = "Create New Password")]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,24}$", ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        private DateTime _createdOn = DateTime.Now;
        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime RegistrationConfirmationDate { get { return (_createdOn == DateTime.MinValue) ? DateTime.Now : _createdOn; } set { _createdOn = value; } }

        [Column(TypeName = "date")]
        public DateTime RegistrationDate { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the Terms and Conditions to proceed")]
        public bool terms { get; set; }

    }
    public class CompleteRegistrationViewModel
    {
        [Display(Name = "Registration Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string RegistrationEmail { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [Display(Name = "One Time Pin")]
        [RegularExpression(@"^[a-zA-Z0-9\s,]*$", ErrorMessage = "Incorrect OTP")]
        [StringLength(6, ErrorMessage = "Incorrect OTP", MinimumLength = 2)]
        public string OTP { get; set; }

        [Display(Name = "Create New Password")]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,24}$", ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        
        private DateTime _createdOn = DateTime.Now;
        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime RegistrationConfirmationDate { get { return (_createdOn == DateTime.MinValue) ? DateTime.Now : _createdOn; } set { _createdOn = value; } }

        [Column(TypeName = "date")]
        public DateTime RegistrationDate { get; set; }

    }
    public class SignatoryConfirmationViewModel
    {
        [Display(Name = "Signatory Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string SignatoryEmail { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [Display(Name = "One Time Pin")]
        [RegularExpression(@"^[a-zA-Z0-9\s,]*$", ErrorMessage = "Incorrect OTP")]
        [StringLength(6, ErrorMessage = "Incorrect OTP", MinimumLength = 2)]
        public string OTP { get; set; }

        [Display(Name = "Create New Password")]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,24}$", ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the Terms and Conditions to proceed")]
        public bool terms { get; set; }

    }

    public class DesignatedUserConfirmationViewModel
    {
        [Display(Name = "Authorised Representative's Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string SignatoryEmail { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [Display(Name = "One Time Pin")]
        [RegularExpression(@"^[a-zA-Z0-9\s,]*$", ErrorMessage = "Incorrect OTP")]
        [StringLength(6, ErrorMessage = "Incorrect OTP", MinimumLength = 2)]
        public string OTP { get; set; }

        [Display(Name = "Create New Password")]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,24}$", ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format: Your password must be atleast 8, alphanumeric upper and lowercase letters with a special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the Terms and Conditions to proceed")]
        public bool terms { get; set; }

    }
    
    public class ResetPasswordViewModel
    {        
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,24}$", ErrorMessage = "Invalid password format: Your password must be atleast 8, alphaneumeric upper and lowercase!")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format")]
        public string Password { get; set; }

        [Required(ErrorMessage = "PasswordResetCode is required")]
        [Display(Name = "Password Reset Code")]
        public string PasswordResetCode { get; set; }

        [Required(ErrorMessage = "Confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {      
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    public class ConfirmRegistration
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace OnBoarding.ViewModels

{
    public class ApplicationApprovalsViewModel
    {
        public int ApplicationID { get; set; }
        public int SignatoryID { get; set; }
        public bool terms { get; set; }
        public string inputFile { get; set; } //Signature
        public string CompanyEmail { get; set; }
        public string CompanyName { get; set; }
    }
    
    public class UploadedUsersViewModel
    {
        public string AccountName { get; set; }
        public string CompanyName { get; set; }
        public string EmailAddress { get; set; }
        public bool UploadedBy { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class UploadedClientsViewModel
    {
        public int ClientID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistration { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public bool? AcceptedTAC { get; set; }
        public string UploadedBy { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class NotificationsViewModel
    {
        public int Id { get; set; }
        public bool Sent { get; set; }
        public int? Count { get; set; }
        [AllowHtml]
        public string MessageBody { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string DateCreated { get; set; }
        public string Action { get; set; }
    }

    // Roles Post Model
    public class UserRolesViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public String StatusName { get; set; }
    }

    //Add Role
    public class PostRolesViewModel
    {
        public string Name { get; set; }
    }

    //Edit Role
    public class EditRoleViewModel
    {
        public string EditId { get; set; }
        public string EditName { get; set; }
        public int EditTModeStatus { get; set; }
        public int Status { get; set; }
    }

    //Curencies Get
    public class SystemCurrencyViewModel
    {
        public int Id { get; set; }
        public string CurrencyShort { get; set; }
        public string CurrencyName { get; set; }
        public String StatusName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
    }
    
    //Curencies Get
    public class PostCurrencyViewModel
    {
        public int Id { get; set; }
        public string CurrencyShortName { get; set; }
        public string CurrencyName { get; set; }
    }

    //Edit Curencies
    public class EditCurrencyViewModel
    {
        public int EditId { get; set; }
        public string EditCurrencyShortName { get; set; }
        public string EditCurrencyName { get; set; }
        public int EditTModeStatus { get; set; }
        public int EditStatus { get; set; }
    }

    //Edit User
    public class EditUserViewModel
    {
        public string getUserId { get; set; }
        public string EditUserNames { get; set; }
        public string EditStaffNumber { get; set; }
        public string EditEmail { get; set; }
        public string EditPhoneNumber { get; set; }
        public string EditUserRole { get; set; }
        public int EditTModeStatus { get; set; }
        public int LockUnlockUser { get; set; }
    }

    // User Post Model
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        public string StatusName { get; set; }
        public string RoleName { get; set; }
        public DateTime DateCreated { get; set; }
    }
    
    public class PostUsersViewModel
    {
        public string StaffNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string UserRole { get; set; }
        public string Password { get; set; }
    }

    public class AdminApproveViewModel
    {
        public int ApplicationID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string Comments { get; set; }
    }

    public class DeleteUserViewModel
    {
        public string UserId { get; set; }
    }

    public class DeleteClientViewModel
    {
        public int ClientId { get; set; }
        public string UserAccountId { get; set; }
        public string EmailAddress { get; set; }
    }

    public class DeclineApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public string Comments { get; set; }
    }

    public class DeleteSignatoryViewModel
    {
        public int SignatoryId { get; set; }
        public string UserAccountId { get; set; }
        public int ClientId { get; set; }
    }

    public class GetUserRolesSelect2Model
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class SystemNotificationsViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string MessageBody { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastDateResent { get; set; }
        public bool Sent { get; set; }
        public int? Attempts { get; set; }
    }

    public class ResendMessageViewModel
    {
        public int getNotificationId { get; set; }
        public string ResendEmail { get; set; }
        [AllowHtml]
        public string ResendMessage { get; set; }
    }

    public class EditUploadedClientViewModel
    {
        public int ClientID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistration { get; set; }
        public string CompanyTownCity { get; set; }
        public string Building { get; set; }
        public string Street { get; set; }
        public string CompanyEmail { get; set; }

        public string PostalCode { get; set; }
        public string PostalAddress { get; set; }
        public string BusinessEmailAddress { get; set; }

        public int? SelectCurrency1 { get; set; }
        public int? SelectCurrency2 { get; set; }
        public int? SelectCurrency3 { get; set; }
        public int? SelectCurrency4 { get; set; }
        public int? SelectCurrency5 { get; set; }
        public int? SelectCurrency6 { get; set; }
        public int? SelectCurrency7 { get; set; }
        public int? SelectCurrency8 { get; set; }

        public string InputCurrencyType1 { get; set; }
        public string InputCurrencyType2 { get; set; }
        public string InputCurrencyType3 { get; set; }
        public string InputCurrencyType4 { get; set; }
        public string InputCurrencyType5 { get; set; }
        public string InputCurrencyType6 { get; set; }
        public string InputCurrencyType7 { get; set; }
        public string InputCurrencyType8 { get; set; }

        public string SettlementAccount1 { get; set; }
        public string SettlementAccount2 { get; set; }
        public string SettlementAccount3 { get; set; }
        public string SettlementAccount4 { get; set; }
        public string SettlementAccount5 { get; set; }
        public string SettlementAccount6 { get; set; }
        public string SettlementAccount7 { get; set; }
        public string SettlementAccount8 { get; set; }

        public string HaveSettlementAccount { get; set; }

        public string SignatorySurname1 { get; set; }
        public string SignatorySurname2 { get; set; }
        public string SignatorySurname3 { get; set; }
        public string SignatorySurname4 { get; set; }
        public string SignatorySurname5 { get; set; }

        public string SignatoryOtherNames1 { get; set; }
        public string SignatoryOtherNames2 { get; set; }
        public string SignatoryOtherNames3 { get; set; }
        public string SignatoryOtherNames4 { get; set; }
        public string SignatoryOtherNames5 { get; set; }

        public string SignatoryDesignation1 { get; set; }
        public string SignatoryDesignation2 { get; set; }
        public string SignatoryDesignation3 { get; set; }
        public string SignatoryDesignation4 { get; set; }
        public string SignatoryDesignation5 { get; set; }

        public string SignatoryEmail1 { get; set; }
        public string SignatoryEmail2 { get; set; }
        public string SignatoryEmail3 { get; set; }
        public string SignatoryEmail4 { get; set; }
        public string SignatoryEmail5 { get; set; }

        public string SignatoryPhoneNumber1 { get; set; }
        public string SignatoryPhoneNumber2 { get; set; }
        public string SignatoryPhoneNumber3 { get; set; }
        public string SignatoryPhoneNumber4 { get; set; }
        public string SignatoryPhoneNumber5 { get; set; }

        public string UserSurname1 { get; set; }
        public string UserSurname2 { get; set; }
        public string UserSurname3 { get; set; }
        public string UserSurname4 { get; set; }
        public string UserSurname5 { get; set; }

        public string UserOthernames1 { get; set; }
        public string UserOthernames2 { get; set; }
        public string UserOthernames3 { get; set; }
        public string UserOthernames4 { get; set; }
        public string UserOthernames5 { get; set; }

        public string UserPOBox1 { get; set; }
        public string UserPOBox2 { get; set; }
        public string UserPOBox3 { get; set; }
        public string UserPOBox4 { get; set; }
        public string UserPOBox5 { get; set; }

        public string UserTownCity1 { get; set; }
        public string UserTownCity2 { get; set; }
        public string UserTownCity3 { get; set; }
        public string UserTownCity4 { get; set; }
        public string UserTownCity5 { get; set; }

        public string UserZipCode1 { get; set; }
        public string UserZipCode2 { get; set; }
        public string UserZipCode3 { get; set; }
        public string UserZipCode4 { get; set; }
        public string UserZipCode5 { get; set; }

        public string TransactionLimit1 { get; set; }
        public string TransactionLimit2 { get; set; }
        public string TransactionLimit3 { get; set; }
        public string TransactionLimit4 { get; set; }
        public string TransactionLimit5 { get; set; }

        public bool? EMarketSignUp1 { get; set; }
        public bool? EMarketSignUp2 { get; set; }
        public bool? EMarketSignUp3 { get; set; }
        public bool? EMarketSignUp4 { get; set; }
        public bool? EMarketSignUp5 { get; set; }

        public DateTime? DOB1 { get; set; }
        public DateTime? DOB2 { get; set; }
        public DateTime? DOB3 { get; set; }
        public DateTime? DOB4 { get; set; }
        public DateTime? DOB5 { get; set; }

        public string UserMobileNumber1 { get; set; }
        public string UserMobileNumber2 { get; set; }
        public string UserMobileNumber3 { get; set; }
        public string UserMobileNumber4 { get; set; }
        public string UserMobileNumber5 { get; set; }

        public string UserEmail1 { get; set; }
        public string UserEmail2 { get; set; }
        public string UserEmail3 { get; set; }
        public string UserEmail4 { get; set; }
        public string UserEmail5 { get; set; }
    }
}
using System;

namespace OnBoarding.ViewModels
{
    public class RegisteredClientsViewModel
    {
        public int ClientId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string Status { get; set; }
        public bool? AcceptedTAC { get; set; }
        public string AccountNumber { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ClientSignatoriesViewModel
    {
        public int Id { get; set; }

        public int SignatoryId { get; set; }
        public string Names { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public bool? AcceptedTAC { get; set; }
        public string DateCreated { get; set; }
        public string PhoneNumber { get; set; }
        public string Signature { get; set; }
        public string Designation { get; set; }
    }
    
    public class ClientApplicationsViewModel
    {
        public int ApplicationID { get; set; }
        public int CompanyID { get; set; }
        public string Client { get; set; }
        public int NominationType { get; set; }
        public int NominationStatus { get; set; }
        public string Status { get; set; }
        public string OPSComments { get; set; }
        public bool AcceptedTAC { get; set; }
        public bool OPSApproved { get; set; }
        public bool OPSDeclined { get; set; }
        public bool POAApproved { get; set; }
        public bool POADeclined { get; set; }
        public bool? EMT { get; set; }
        public bool? SSI { get; set; }
        public int SignatoriesApproved { get; set; }
        public int RepresentativesApproved { get; set; }
        public DateTime? POADateApproved { get; set; }
        public int Signatories { get; set; }
        public int DesignatedUsers { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateDeclined { get; set; }
    }

    public class ClientSettlementsViewModel
    {
        public int Id { get; set; }
        public int CurrencyID { get; set; }
        public string CompanyName { get; set; }
        public string AccountNumber { get; set; }
        public string EmailAddress { get; set; }
        public string BusinessEmailAddress { get; set; }
        public string SSI { get; set; }
        public string EmarketSignUp { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class SignatoryApprovalsViewModel
    {
        public int ApplicationID { get; set; }
        public string Client { get; set; }
        public string Signatory { get; set; }
        public string SignatoryEmail { get; set; }
        public bool AcceptedTAC { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
    public class ConfirmApproveViewModel
    {
        public int ApplicationID { get; set; }
        public int SignatoryID { get; set; }
        public int CompanyID { get; set; }
        public bool terms { get; set; }
        public string inputFile { get; set; } //Signature Upload
        public string CompanyEmail { get; set; }
        public string CompanyName { get; set; }
        public string CompanySurname { get; set; }
        public string VerifyPhone { get; set; }
        public int? PostalAddress { get; set; }
        public int? ZipCode { get; set; }
        public string TownCity { get; set; }
        public DateTime? DOB { get; set; }
    }

    public class DeclineNominationViewModel
    {
        public int ApplicationID { get; set; }
        public int SignatoryID { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyName { get; set; }
        public string Comments { get; set; }
    }

    public class UserDeclineNominationViewModel
    {
        public int ApplicationID { get; set; }
        public int UserID { get; set; }
        public int CompanyID { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyName { get; set; }
        public string Comments { get; set; }
        public bool terms { get; set; }
        public string VerifyPhone { get; set; }
    }

    public class Select2Model
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class ExpiredOtpsViewModel
    {
        public int ClientID { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public int? TimeExpired { get; set; }
    }

    public class ResetClientOTPViewModel
    {
        public int getClientId { get; set; }
    }

    public class UploadedClientViewModel
    {
        public int ClientID { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ExistingClientsUploadViewModel
    {
        public string UploadedBy { get; set; }
        public string FileName { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class UploadedClientSSIViewModel
    {
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
    }

    public class UploadedClientRepresentativeViewModel
    {
        public string RepresentativeName { get; set; }
        public string RepresentativeEmail { get; set; }
        public string RepresentativePhonenumber { get; set; }
        public string RepresentativeLimit { get; set; }
        public string IsGM { get; set; }
        public string IsEMTUser { get; set; }
    }

    public class ClientCompaniesViewModel
    {
        public int Id {  get; set; }
        public string CompanyName {  get; set; }
        public string CompanyRegNumber {  get; set; }
        public string CompanyEmail {  get; set; }
        public int Status {  get; set; }
        public DateTime DateCreated {  get; set; }
        public bool HasApplication {  get; set; }

    }
}
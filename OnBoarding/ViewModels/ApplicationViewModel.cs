using System;

namespace OnBoarding.ViewModels
{
    public class ApplicationViewModel
    {
        public int ClientID { get; set; }
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistration { get; set; }
        public string CompanyTownCity { get; set; }
        public string Building { get; set; }
        public string Street { get; set; }
        public string CompanyEmail { get; set; }

        public string PostalCode { get; set; }
        public string PostalAddress { get; set; }
        public string BusinessEmailAddress { get; set; }
        public string AttentionTo { get; set; }
        public string CompanyFax { get; set; }
        public string AddressTownCity { get; set; }

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

        public bool terms { get; set; }
        
        public bool? EMarketSignUp1 { get; set; } 
        public bool? EMarketSignUp2 { get; set; }  
        public bool? EMarketSignUp3 { get; set; }
        public bool? EMarketSignUp4 { get; set; }
        public bool? EMarketSignUp5 { get; set; }

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

        public string inputFile { get; set; }
    }

    public class EditSignatoriesViewModel
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string OtherNames { get; set; }
        public string Designation { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Signature { get; set; }
    }

    public class EditRepresentativesViewModel
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Othernames { get; set; }
        public string TradingLimit { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public bool EMarketSignUp { get; set; }
        public string Signature { get; set; }
    }
}